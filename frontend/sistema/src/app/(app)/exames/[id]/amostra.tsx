"use client";

import { useActionState, useState, useTransition } from "react";
import type { Exame } from "@/lib/api";
import { formatarData } from "@/lib/idade";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Etiqueta } from "@/components/ui/etiqueta";
import { acolherAmostra, rejeitarAmostra, type EstadoAcao } from "../actions";

function hojeIso() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
}

/** Acolhimento (1→2), rejeição (2→1) e histórico de amostras — P29–P31. */
export function PainelAmostra({ exame }: { exame: Exame }) {
  const [estado, acao, pendente] = useActionState<EstadoAcao, FormData>(acolherAmostra.bind(null, exame.id), {});
  const [rejeitando, setRejeitando] = useState(false);
  const [motivo, setMotivo] = useState("");
  const [erro, setErro] = useState<string | null>(null);
  const [pendenteRejeicao, iniciar] = useTransition();

  const podeAcolher = !exame.excluido && exame.estado === "AguardandoAmostra";
  const podeRejeitar = !exame.excluido && exame.estado === "AmostraAcolhida";
  const ativa = exame.amostraAtiva;

  return (
    <div>
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <h2 className="font-semibold text-texto">Amostra e prazo</h2>
        <p className="text-xs text-texto-suave">Execução: {exame.prazoExecucaoDias} dias corridos + revisão</p>
      </div>

      <dl className="mt-3 space-y-2 text-sm">
        <div className="flex gap-3">
          <dt className="w-40 shrink-0 text-texto-suave">Acolhimento</dt>
          <dd className="text-texto">{ativa ? formatarData(ativa.dataAcolhimento) : "Aguardando amostra"}</dd>
        </div>
        <div className="flex gap-3">
          <dt className="w-40 shrink-0 text-texto-suave">Liberação prevista</dt>
          <dd className="text-texto">
            {exame.dataLiberacaoPrevista ? formatarData(exame.dataLiberacaoPrevista) : "— definida no acolhimento"}
          </dd>
        </div>
        <div className="flex gap-3">
          <dt className="w-40 shrink-0 text-texto-suave">Liberação efetiva</dt>
          <dd className="text-texto">{exame.dataLiberacaoEfetiva ? formatarData(exame.dataLiberacaoEfetiva) : "—"}</dd>
        </div>
      </dl>

      {podeAcolher && (
        <form action={acao} className="mt-4 flex flex-wrap items-end gap-3 border-t border-borda/60 pt-4">
          <Campo
            label="Data de acolhimento"
            name="dataAcolhimento"
            type="date"
            required
            defaultValue={hojeIso()}
            max={hojeIso()}
            min={exame.dataEntrada.slice(0, 10)}
            className="w-48"
            dica="A contagem do prazo começa aqui"
          />
          <Botao type="submit" carregando={pendente} className="mb-5">
            Registrar acolhimento
          </Botao>
          {(estado.erro || estado.sucesso) && (
            <p role={estado.erro ? "alert" : "status"} className={`mb-6 text-xs ${estado.erro ? "text-erro" : "text-sucesso"}`}>
              {estado.erro ?? estado.sucesso}
            </p>
          )}
        </form>
      )}

      {podeRejeitar && (
        <div className="mt-4 border-t border-borda/60 pt-4">
          {!rejeitando ? (
            <Botao variante="fantasma" tamanho="sm" onClick={() => setRejeitando(true)}>
              Rejeitar amostra
            </Botao>
          ) : (
            <div className="max-w-md rounded-xl border border-alerta/40 bg-alerta/5 p-4">
              <p className="text-sm font-semibold text-texto">Rejeitar a amostra?</p>
              <p className="mt-1 text-xs text-texto-suave">
                O prazo é zerado e o exame volta a aguardar amostra. A recoleta gera um novo acolhimento e nova previsão.
              </p>
              <label htmlFor="motivo-rejeicao" className="mt-3 block text-sm font-medium text-texto">
                Motivo (obrigatório)
              </label>
              <textarea
                id="motivo-rejeicao"
                value={motivo}
                onChange={(e) => setMotivo(e.target.value)}
                rows={2}
                maxLength={500}
                className="mt-1 w-full rounded-lg border border-borda px-3 py-2 text-sm text-texto outline-none focus:border-primaria focus:ring-2 focus:ring-primaria/40"
              />
              {erro && (
                <p role="alert" className="mt-2 text-xs text-erro">
                  {erro}
                </p>
              )}
              <div className="mt-3 flex justify-end gap-2">
                <Botao variante="fantasma" tamanho="sm" disabled={pendenteRejeicao} onClick={() => setRejeitando(false)}>
                  Cancelar
                </Botao>
                <Botao
                  variante="perigo"
                  tamanho="sm"
                  carregando={pendenteRejeicao}
                  disabled={!motivo.trim()}
                  onClick={() => {
                    setErro(null);
                    iniciar(async () => {
                      const r = await rejeitarAmostra(exame.id, motivo);
                      if (!r.ok) setErro(r.erro ?? "Falha.");
                      else {
                        setRejeitando(false);
                        setMotivo("");
                      }
                    });
                  }}
                >
                  Confirmar rejeição
                </Botao>
              </div>
            </div>
          )}
        </div>
      )}

      {exame.amostras.length > 1 || exame.amostras.some((a) => a.situacao === "Rejeitada") ? (
        <div className="mt-4 border-t border-borda/60 pt-3">
          <p className="text-xs font-semibold uppercase tracking-wider text-texto-suave">Histórico</p>
          <ul className="mt-2 space-y-1 text-xs text-texto">
            {exame.amostras.map((a) => (
              <li key={a.id} className="flex flex-wrap items-center gap-2">
                <span>Acolhida em {formatarData(a.dataAcolhimento)}</span>
                {a.situacao === "Rejeitada" ? (
                  <>
                    <Etiqueta cor="vermelho">Rejeitada</Etiqueta>
                    <span className="text-texto-suave">
                      {a.rejeitadaEm && `em ${formatarData(a.rejeitadaEm)} · `}
                      {a.motivoRejeicao}
                    </span>
                  </>
                ) : (
                  <Etiqueta cor="verde">Ativa</Etiqueta>
                )}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </div>
  );
}
