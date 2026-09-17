using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Comum;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Fluxo do laudo (docs/05-regras-negocio/fluxo-laudo.md): etapas com arquivo, substituição e disponibilização manual.</summary>
public sealed class LaudoService(
    IExameRepositorio repositorio,
    IPacienteRepositorio pacientes,
    IExameCatalogoRepositorio catalogo,
    IArmazenamentoArquivos armazenamento,
    IEnvioEmail envioEmail,
    IEnvioWhatsApp envioWhatsApp,
    IUrlPortalPaciente urlPortal,
    IUsuarioAtual usuarioAtual,
    ExameService exames)
{
    /// <summary>
    /// Registra a etapa (avança o estado) ou substitui o arquivo dela. Só PDF, conferido pelos primeiros bytes.
    /// O arquivo substituído não é apagado do armazenamento: fica sem referência (nada do prontuário é removido).
    /// </summary>
    /// <param name="conteudo">Precisa permitir Seek: o formato é conferido antes de gravar.</param>
    public async Task<ExameDto> EnviarArquivoEtapaAsync(
        Guid exameId, TipoEtapaLaudo tipo, Stream conteudo, string? nomeOriginal, long tamanhoBytes, CancellationToken ct = default)
    {
        if (!conteudo.CanSeek)
            throw new ArgumentException("O conteúdo do laudo precisa permitir Seek.", nameof(conteudo));

        var exame = await ObterOuFalharAsync(exameId, ct);
        exame.GarantirPodeReceberEtapa(tipo);

        var cabecalho = new byte[FormatoArquivo.BytesCabecalho];
        var lidos = await conteudo.ReadAtLeastAsync(cabecalho, cabecalho.Length, throwOnEndOfStream: false, ct);
        conteudo.Position = 0;
        var formato = FormatoArquivo.Detectar(cabecalho.AsSpan(0, lidos), tamanhoBytes);
        if (formato != FormatoArquivo.Pdf)
            throw new ValidacaoException("O laudo deve ser um arquivo PDF.");

        var armazenado = await armazenamento.SalvarAsync(conteudo, formato.Extensao, ct);
        exame.RegistrarOuSubstituirEtapa(
            tipo, new ArquivoLaudo(nomeOriginal, armazenado.Caminho, armazenado.TamanhoBytes, armazenado.HashSha256), usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
        return await exames.MapearAsync(exame, ct);
    }

    /// <returns>Nulo se o exame ou a etapa não existir.</returns>
    public async Task<(EtapaLaudoDto Etapa, Stream Conteudo)?> AbrirArquivoEtapaAsync(Guid exameId, TipoEtapaLaudo tipo, CancellationToken ct = default)
    {
        var exame = await repositorio.ObterPorIdAsync(exameId, ct);
        var etapa = exame?.Etapa(tipo);
        if (etapa is null) return null;
        return (ExameService.Mapear(etapa), await armazenamento.AbrirLeituraAsync(etapa.Caminho, ct));
    }

    /// <summary>
    /// 5→6 (D10): grava a data efetiva e dispara e-mail e WhatsApp no modelo do médico solicitante (D11, D13).
    /// Envio síncrono, em log, até existirem provedores e fila.
    /// </summary>
    public async Task<ExameDto> DisponibilizarAsync(Guid exameId, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(exameId, ct);
        exame.Disponibilizar();
        await repositorio.AtualizarAsync(exame, ct);

        var paciente = await pacientes.ObterPorIdAsync(exame.PacienteId, ct)
                       ?? throw new InvalidOperationException($"Paciente {exame.PacienteId} do exame {exame.Id} não existe.");
        var itemCatalogo = await catalogo.ObterPorIdAsync(exame.ExameCatalogoId, ct)
                           ?? throw new InvalidOperationException($"Catálogo {exame.ExameCatalogoId} do exame {exame.Id} não existe.");

        // R1: o nome do exame trafega na mensagem por decisão do cliente (Q35).
        var aviso = new LaudoDisponivel(
            paciente.Email, paciente.Telefone, paciente.ResponsavelLegal?.Nome ?? paciente.Nome, paciente.Nome,
            itemCatalogo.Nome, exame.TipoMedico == TipoMedico.Interno, urlPortal.Obter());
        await envioEmail.EnviarLaudoDisponivelAsync(aviso, ct);
        await envioWhatsApp.EnviarLaudoDisponivelAsync(aviso, ct);

        return await exames.MapearAsync(exame, ct);
    }

    private async Task<Exame> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Exame não encontrado.");
}
