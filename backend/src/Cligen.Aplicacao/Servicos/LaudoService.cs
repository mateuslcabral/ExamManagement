using System.Security.Cryptography;
using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Fluxo do laudo (CLG-ESP §8, P32–P34): etapas com arquivo, substituição e disponibilização manual.</summary>
public sealed class LaudoService(
    IExameRepositorio exames,
    IArmazenamentoArquivos armazenamento,
    IEnvioEmail envioEmail,
    IEnvioWhatsApp envioWhatsApp)
{
    /// <summary>
    /// Registra a etapa (avança o estado, data automática) ou substitui o arquivo de etapa já registrada.
    /// O PDF é gravado sem transformação (P33). Após o estado 6 a substituição não notifica (R2).
    /// </summary>
    public async Task<ExameDto> EnviarArquivoEtapaAsync(
        Guid exameId, TipoEtapaLaudo tipo, string nomeOriginal, Stream conteudo, long tamanhoBytes, Guid usuarioId, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(tipo)) throw new ValidacaoException("Etapa inválida.");
        var exame = await ObterOuFalharAsync(exameId, ct);

        // Falha cedo, antes de gravar em disco.
        if (exame.Excluido) throw new ExameExcluidoException();
        EtapaAndamento.ValidarArquivo(nomeOriginal, tamanhoBytes);
        if (exame.Etapa(tipo) is null && exame.Estado != (EstadoExame)((int)tipo - 1))
            throw new EstadoInvalidoException($"registrar '{Exame.DescreverEtapa(tipo)}'", Exame.DescreverEstado(exame.Estado));

        string hash;
        using (var sha = SHA256.Create())
            hash = Convert.ToHexString(await sha.ComputeHashAsync(conteudo, ct)).ToLowerInvariant();
        conteudo.Position = 0;

        var caminhoRelativo = $"laudos/{exame.Id}/{(int)tipo}-{Guid.NewGuid()}.pdf";
        var caminho = await armazenamento.SalvarAsync(caminhoRelativo, conteudo, ct);

        string? anterior;
        try
        {
            (_, anterior) = exame.RegistrarOuSubstituirEtapa(tipo, nomeOriginal, caminho, tamanhoBytes, hash, usuarioId);
            await exames.AtualizarAsync(exame, ct);
        }
        catch
        {
            await armazenamento.RemoverAsync(caminho, CancellationToken.None);
            throw;
        }

        // Arquivo substituído: o anterior sai do storage (sem histórico de versão — R2 aceito).
        if (anterior is not null)
            await armazenamento.RemoverAsync(anterior, CancellationToken.None);

        return ExameService.MapearCompleto(exame);
    }

    public async Task<ArquivoAnexo> AbrirArquivoEtapaAsync(Guid exameId, TipoEtapaLaudo tipo, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(exameId, ct);
        var etapa = exame.Etapa(tipo) ?? throw new RegistroNaoEncontradoException("Arquivo da etapa");
        var stream = await armazenamento.AbrirAsync(etapa.Caminho, ct);
        return new ArquivoAnexo(stream, etapa.NomeOriginal, "application/pdf");
    }

    /// <summary>5→6 (D10): grava a data efetiva e dispara e-mail + WhatsApp no modelo do médico (D11, D13, P34).</summary>
    public async Task<ExameDto> DisponibilizarAsync(Guid exameId, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(exameId, ct);
        exame.Disponibilizar();
        await exames.AtualizarAsync(exame, ct);

        var interno = exame.TipoMedico == TipoMedicoSolicitante.Interno;
        var paciente = exame.Paciente;
        await envioEmail.EnviarLaudoDisponivelAsync(paciente.Email, paciente.Nome, exame.ExameCatalogo.Nome, interno, ct);
        await envioWhatsApp.EnviarLaudoDisponivelAsync(paciente.Telefone, paciente.Nome, exame.ExameCatalogo.Nome, interno, ct);

        return ExameService.MapearCompleto(exame);
    }

    private async Task<Exame> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await exames.ObterPorIdAsync(id, ct) ?? throw new RegistroNaoEncontradoException("Exame");
}
