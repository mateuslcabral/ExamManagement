"use client";

import { useActionState, useState, useTransition } from "react";
import type { Cadastro, ExameCatalogo } from "@/lib/api";
import { formatarMoeda, valorParaCampo } from "@/lib/formatos";
import { desativarExame, reativarExame, salvarExame, type EstadoSalvarExame } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Etiqueta } from "@/components/ui/etiqueta";
import { Selecao } from "@/components/ui/selecao";

const dias = (n: number) => `${n} ${n === 1 ? "dia" : "dias"}`;

/** Gerência do catálogo: formulário (criar/editar) e tabela compartilham o exame em edição. */
export function CatalogoExames({ exames, destinos }: { exames: ExameCatalogo[]; destinos: Cadastro[] }) {
  const [emEdicao, setEmEdicao] = useState<ExameCatalogo | null>(null);
  // Troca a key do formulário para limpá-lo após salvar ou cancelar.
  const [versaoFormulario, setVersaoFormulario] = useState(0);

  const [estado, acao, salvando] = useActionState<EstadoSalvarExame, FormData>(async (anterior, dados) => {
    const r = await salvarExame(anterior, dados);
    if (r.sucesso) limparFormulario();
    return r;
  }, {});

  const [alterandoSituacao, iniciar] = useTransition();
  const [mensagemTabela, setMensagemTabela] = useState<{ tipo: "ok" | "erro"; texto: string } | null>(null);

  function limparFormulario() {
    setEmEdicao(null);
    setVersaoFormulario((v) => v + 1);
  }

  function editar(exame: ExameCatalogo) {
    setEmEdicao(exame);
    setVersaoFormulario((v) => v + 1);
  }

  function alternarSituacao(exame: ExameCatalogo) {
    setMensagemTabela(null);
    iniciar(async () => {
      const r = exame.ativo ? await desativarExame(exame.id) : await reativarExame(exame.id);
      setMensagemTabela(
        r.ok ? { tipo: "ok", texto: `${exame.nome} ${exame.ativo ? "desativado" : "reativado"}.` } : { tipo: "erro", texto: r.erro ?? "Falha." },
      );
    });
  }

  const alvo = `${emEdicao?.id ?? "novo"}#${versaoFormulario}`;
  // Erros e valores digitados só valem para o formulário que os gerou.
  const estadoDoAlvo = estado.alvo === alvo ? estado : {};
  const v = estadoDoAlvo.valores;
  const erro = estadoDoAlvo.campos ?? {};
  const destinosAtivos = destinos.filter((d) => d.ativo || d.id === emEdicao?.destinoId);

  return (
    <div className="mt-6 grid gap-6 lg:grid-cols-[400px_1fr]">
      <section className="h-fit rounded-2xl border border-borda bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-texto">{emEdicao ? "Editar exame" : "Novo exame"}</h2>
        <p className="mt-1 text-xs text-texto-suave">
          {emEdicao
            ? "A alteração vale para os próximos exames solicitados; os já registrados mantêm preço e data prevista."
            : "O prazo é o de entrega ao paciente, em dias corridos a partir do acolhimento, já contando a revisão."}
        </p>

        <form key={versaoFormulario} action={acao} className="mt-4 space-y-4">
          <input type="hidden" name="alvo" value={alvo} />
          {emEdicao && <input type="hidden" name="id" value={emEdicao.id} />}
          <Campo label="Nome do exame" name="nome" required maxLength={200} autoComplete="off" defaultValue={v?.nome ?? emEdicao?.nome} erro={erro.nome} />
          <div className="grid grid-cols-2 gap-3">
            <Campo
              label="Prazo de entrega (dias)"
              name="prazo"
              type="number"
              min={1}
              step={1}
              required
              defaultValue={v?.prazo ?? emEdicao?.prazoDias}
              erro={erro.prazo}
              dica="Dias corridos após o acolhimento."
            />
            <Campo
              label="Nº de genes (opcional)"
              name="numeroGenes"
              type="number"
              min={0}
              step={1}
              defaultValue={v?.numeroGenes ?? emEdicao?.numeroGenes ?? ""}
              erro={erro.numeroGenes}
            />
            <Campo
              label="Preço de referência (R$)"
              name="precoReferencia"
              inputMode="decimal"
              required
              autoComplete="off"
              placeholder="0,00"
              defaultValue={v?.precoReferencia ?? (emEdicao ? valorParaCampo(emEdicao.precoReferencia) : undefined)}
              erro={erro.precoReferencia}
            />
            <Campo
              label="Preço limite (R$)"
              name="precoLimite"
              inputMode="decimal"
              autoComplete="off"
              placeholder="0,00"
              defaultValue={v?.precoLimite ?? (emEdicao ? valorParaCampo(emEdicao.precoLimite) : undefined)}
              erro={erro.precoLimite}
              dica="Piso: o desconto não pode levar o valor abaixo dele."
            />
          </div>
          <Selecao label="Destino padrão (opcional)" name="destinoId" defaultValue={v?.destinoId ?? emEdicao?.destinoId ?? ""} erro={erro.destinoId}>
            <option value="">Sem destino padrão</option>
            {destinosAtivos.map((d) => (
              <option key={d.id} value={d.id}>
                {d.nome}
                {!d.ativo && " (inativo)"}
              </option>
            ))}
          </Selecao>
          <AreaTexto label="Genes (opcional)" name="genes" defaultValue={v?.genes ?? emEdicao?.genes ?? ""} erro={erro.genes} placeholder="Ex.: BRCA1, BRCA2, TP53" />
          <AreaTexto
            label="Informações técnicas (opcional)"
            name="informacoesTecnicas"
            defaultValue={v?.informacoesTecnicas ?? emEdicao?.informacoesTecnicas ?? ""}
            erro={erro.informacoesTecnicas}
            placeholder="Método, cobertura, material, observações"
          />

          {estadoDoAlvo.erro && (
            <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
              {estadoDoAlvo.erro}
            </p>
          )}
          {estado.sucesso && !emEdicao && (
            <p role="status" className="rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
              {estado.sucesso}
            </p>
          )}

          <div className="flex gap-3">
            {emEdicao && (
              <Botao type="button" variante="secundaria" className="flex-1" onClick={limparFormulario}>
                Cancelar
              </Botao>
            )}
            <Botao type="submit" className="flex-1" carregando={salvando}>
              {emEdicao ? "Salvar alterações" : "Cadastrar"}
            </Botao>
          </div>
        </form>
      </section>

      <section className="h-fit rounded-2xl border border-borda bg-white shadow-sm">
        {mensagemTabela && (
          <p
            role={mensagemTabela.tipo === "erro" ? "alert" : "status"}
            className={`m-4 rounded-lg px-3 py-2 text-sm ${mensagemTabela.tipo === "erro" ? "bg-erro/10 text-erro" : "bg-sucesso/10 text-sucesso"}`}
          >
            {mensagemTabela.texto}
          </p>
        )}

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
                <th className="px-4 py-3 font-semibold">Exame</th>
                <th className="px-4 py-3 font-semibold">Prazo</th>
                <th className="px-4 py-3 text-right font-semibold">Preços</th>
                <th className="px-4 py-3 font-semibold">Situação</th>
                <th className="px-4 py-3 text-right font-semibold">Ações</th>
              </tr>
            </thead>
            <tbody>
              {exames.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-8 text-center text-texto-suave">
                    Nenhum exame cadastrado.
                  </td>
                </tr>
              )}
              {exames.map((e) => (
                <tr
                  key={e.id}
                  className={`border-b border-borda/60 last:border-0 ${emEdicao?.id === e.id ? "bg-primaria/5" : "hover:bg-fundo"} ${e.ativo ? "" : "text-texto-suave"}`}
                >
                  <td className="px-4 py-3">
                    <p className="font-medium">{e.nome}</p>
                    <p className="text-xs text-texto-suave">
                      {e.destinoNome ?? "sem destino padrão"}
                      {e.numeroGenes != null && ` · ${e.numeroGenes} genes`}
                    </p>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">{dias(e.prazoDias)}</td>
                  <td className="px-4 py-3 text-right whitespace-nowrap tabular-nums">
                    <p>{formatarMoeda(e.precoReferencia)}</p>
                    <p className="text-xs text-texto-suave">mín. {formatarMoeda(e.precoLimite)}</p>
                  </td>
                  <td className="px-4 py-3">{e.ativo ? <Etiqueta cor="verde">Ativo</Etiqueta> : <Etiqueta cor="cinza">Inativo</Etiqueta>}</td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <Botao variante="fantasma" tamanho="sm" disabled={alterandoSituacao || emEdicao?.id === e.id} onClick={() => editar(e)}>
                        Editar
                      </Botao>
                      <Botao
                        variante={e.ativo ? "perigo" : "secundaria"}
                        tamanho="sm"
                        disabled={alterandoSituacao}
                        title={e.ativo ? "Deixa de aparecer para novos exames solicitados" : undefined}
                        onClick={() => alternarSituacao(e)}
                      >
                        {e.ativo ? "Desativar" : "Reativar"}
                      </Botao>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}

function AreaTexto({ label, name, defaultValue, erro, placeholder }: { label: string; name: string; defaultValue?: string; erro?: string; placeholder?: string }) {
  return (
    <div>
      <label htmlFor={name} className="mb-1 block text-sm font-medium text-texto">
        {label}
      </label>
      <textarea
        id={name}
        name={name}
        rows={3}
        maxLength={4000}
        defaultValue={defaultValue}
        placeholder={placeholder}
        aria-invalid={!!erro}
        className={`w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-texto outline-none transition placeholder:text-texto-suave/60 focus:ring-2 focus:ring-primaria/40 ${erro ? "border-erro" : "border-borda focus:border-primaria"}`}
      />
      {erro && <p className="mt-1 text-xs text-erro">{erro}</p>}
    </div>
  );
}
