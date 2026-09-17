"use client";

import { useActionState, useState, useTransition } from "react";
import type { ExameCatalogo } from "@/lib/api";
import { desativarExame, reativarExame, salvarExame, type EstadoSalvarExame } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Etiqueta } from "@/components/ui/etiqueta";
import { formatarMoeda, valorParaCampo } from "@/lib/formatos";

const dias = (n: number) => `${n} ${n === 1 ? "dia" : "dias"}`;

/** Formulário (criar/editar) e tabela compartilham o exame em edição, por isso vivem no mesmo componente. */
export function CatalogoExames({
  exames,
  diasRevisao,
  painelLateral,
}: {
  exames: ExameCatalogo[];
  diasRevisao: number;
  painelLateral: React.ReactNode;
}) {
  const [emEdicao, setEmEdicao] = useState<ExameCatalogo | null>(null);
  const [prazo, setPrazo] = useState("");
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
    setPrazo("");
    setVersaoFormulario((v) => v + 1);
  }

  function editar(exame: ExameCatalogo) {
    setEmEdicao(exame);
    setPrazo(String(exame.prazoExecucaoDias));
    setVersaoFormulario((v) => v + 1);
  }

  function alternarSituacao(exame: ExameCatalogo) {
    setMensagemTabela(null);
    iniciar(async () => {
      const r = exame.ativo ? await desativarExame(exame.id) : await reativarExame(exame.id);
      setMensagemTabela(
        r.ok
          ? { tipo: "ok", texto: `${exame.nome} ${exame.ativo ? "desativado" : "reativado"}.` }
          : { tipo: "erro", texto: r.erro ?? "Falha." },
      );
    });
  }

  const alvo = `${emEdicao?.id ?? "novo"}#${versaoFormulario}`;
  // Erros e valores digitados só valem para o formulário que os gerou.
  const estadoDoAlvo = estado.alvo === alvo ? estado : {};
  const prazoNumero = Number(prazo);
  const prazoValido = prazo.trim() !== "" && Number.isInteger(prazoNumero) && prazoNumero > 0;

  return (
    <div className="mt-6 grid gap-6 lg:grid-cols-[360px_1fr]">
      <div className="space-y-6">
        <section className="h-fit rounded-2xl border border-borda bg-white p-5 shadow-sm">
          <h2 className="font-semibold text-texto">{emEdicao ? "Editar exame" : "Novo exame"}</h2>
          <p className="mt-1 text-xs text-texto-suave">
            {emEdicao
              ? "A alteração vale para os próximos exames solicitados; os já registrados mantêm preço e data prevista."
              : "O prazo informado é o de execução. A entrega ao paciente soma os dias de revisão."}
          </p>

          <form key={versaoFormulario} action={acao} className="mt-4 space-y-4">
            <input type="hidden" name="alvo" value={alvo} />
            {emEdicao && <input type="hidden" name="id" value={emEdicao.id} />}
            <Campo
              label="Nome do exame"
              name="nome"
              required
              maxLength={200}
              autoComplete="off"
              defaultValue={estadoDoAlvo.valores?.nome ?? emEdicao?.nome}
              erro={estadoDoAlvo.campos?.nome}
            />
            <Campo
              label="Prazo de execução (dias)"
              name="prazo"
              type="number"
              min={1}
              step={1}
              required
              defaultValue={estadoDoAlvo.valores?.prazo ?? emEdicao?.prazoExecucaoDias}
              onChange={(e) => setPrazo(e.target.value)}
              erro={estadoDoAlvo.campos?.prazo}
              dica={
                prazoValido
                  ? `Entrega ao paciente: ${dias(prazoNumero + diasRevisao)} (${prazoNumero} de execução + ${diasRevisao} de revisão).`
                  : `Dias corridos. A entrega ao paciente soma ${dias(diasRevisao)} de revisão.`
              }
            />
            <Campo
              label="Preço de referência (R$)"
              name="preco"
              inputMode="decimal"
              required
              autoComplete="off"
              placeholder="0,00"
              defaultValue={
                estadoDoAlvo.valores?.preco ?? (emEdicao ? valorParaCampo(emEdicao.precoReferencia) : undefined)
              }
              erro={estadoDoAlvo.campos?.preco}
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

        {painelLateral}
      </div>

      <section className="h-fit rounded-2xl border border-borda bg-white shadow-sm">
        {mensagemTabela && (
          <p
            role={mensagemTabela.tipo === "erro" ? "alert" : "status"}
            className={`m-4 rounded-lg px-3 py-2 text-sm ${
              mensagemTabela.tipo === "erro" ? "bg-erro/10 text-erro" : "bg-sucesso/10 text-sucesso"
            }`}
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
                <th className="px-4 py-3 text-right font-semibold">Preço de referência</th>
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
                  className={`border-b border-borda/60 last:border-0 ${
                    emEdicao?.id === e.id ? "bg-primaria/5" : "hover:bg-fundo"
                  } ${e.ativo ? "" : "text-texto-suave"}`}
                >
                  <td className="px-4 py-3 font-medium">{e.nome}</td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <p>{dias(e.prazoExecucaoDias)} de execução</p>
                    <p className="text-xs text-texto-suave">Entrega em {dias(e.prazoEntregaDias)}</p>
                  </td>
                  <td className="px-4 py-3 text-right whitespace-nowrap tabular-nums">
                    {formatarMoeda(e.precoReferencia)}
                  </td>
                  <td className="px-4 py-3">
                    {e.ativo ? <Etiqueta cor="verde">Ativo</Etiqueta> : <Etiqueta cor="cinza">Inativo</Etiqueta>}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <Botao
                        variante="fantasma"
                        tamanho="sm"
                        disabled={alterandoSituacao || emEdicao?.id === e.id}
                        onClick={() => editar(e)}
                      >
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
