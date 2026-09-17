"use client";

import { useActionState, useState } from "react";
import type { Exame } from "@/lib/api";
import { formatarData } from "@/lib/formatos";
import { rejeitarAmostra, type EstadoAmostra } from "../actions";
import { FormularioAcolhimento } from "../formulario-acolhimento";
import { Botao } from "@/components/ui/botao";
import { Etiqueta } from "@/components/ui/etiqueta";

export function AmostraExame({ exame, hoje }: { exame: Exame; hoje: string }) {
  const vigente = exame.amostras.find((a) => !a.rejeitadaEm);
  const rejeitadas = exame.amostras.filter((a) => a.rejeitadaEm);

  return (
    <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="font-semibold text-texto">Amostra</h2>
          <p className="mt-1 text-xs text-texto-suave">
            O prazo conta a partir do acolhimento no laboratório, em dias corridos: {exame.prazoExecucaoDias} de execução
            mais a revisão Cligen.
          </p>
        </div>
        {exame.dataLiberacaoPrevista && (
          <div className="text-right">
            <p className="text-xs text-texto-suave">Liberação prevista</p>
            <p className="text-lg font-semibold text-primaria">{formatarData(exame.dataLiberacaoPrevista)}</p>
          </div>
        )}
      </div>

      <div className="mt-4">
        {exame.estado === "AguardandoAmostra" ? (
          <>
            {rejeitadas.length > 0 && (
              <p className="mb-3 rounded-lg bg-alerta/10 px-3 py-2 text-sm text-alerta">
                Aguardando recoleta: a amostra anterior foi rejeitada. O novo acolhimento gera uma nova previsão.
              </p>
            )}
            <FormularioAcolhimento
              exameId={exame.id}
              dataEntrada={exame.dataEntrada}
              prazoEntregaDias={exame.prazoEntregaDias}
              hoje={hoje}
            />
          </>
        ) : vigente ? (
          <div className="space-y-3">
            <p className="text-sm text-texto">
              Acolhida em <span className="font-semibold">{formatarData(vigente.dataAcolhimento)}</span>
              {vigente.recoleta && (
                <span className="ml-2 align-middle">
                  <Etiqueta cor="amarelo">Recoleta</Etiqueta>
                </span>
              )}
            </p>
            <p className="text-xs text-texto-suave">
              Previsão calculada com {vigente.prazoExecucaoDias} dias de execução + {vigente.diasRevisao} de revisão,
              vigentes no acolhimento. Lançado em {new Date(vigente.registradoEm).toLocaleString("pt-BR")}.
            </p>
            {exame.estado === "AmostraAcolhida" && <RejeicaoAmostra exameId={exame.id} />}
          </div>
        ) : null}
      </div>

      {rejeitadas.length > 0 && (
        <div className="mt-5 border-t border-borda pt-4">
          <h3 className="text-xs font-semibold uppercase tracking-wider text-texto-suave">Amostras rejeitadas</h3>
          <ul className="mt-2 space-y-2">
            {rejeitadas.map((a) => (
              <li key={a.id} className="text-sm text-texto">
                Acolhida em {formatarData(a.dataAcolhimento)}, rejeitada em{" "}
                {new Date(a.rejeitadaEm!).toLocaleDateString("pt-BR")}
                <span className="text-texto-suave"> — {a.motivoRejeicao}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  );
}

/** Rejeição zera o prazo e o exame volta a aguardar amostra (Q19). */
function RejeicaoAmostra({ exameId }: { exameId: string }) {
  const [aberto, setAberto] = useState(false);
  const [motivo, setMotivo] = useState("");
  const [estado, acao, rejeitando] = useActionState<EstadoAmostra, FormData>(rejeitarAmostra, {});

  if (!aberto)
    return (
      <Botao variante="fantasma" tamanho="sm" className="text-erro hover:bg-erro/10" onClick={() => setAberto(true)}>
        Rejeitar amostra…
      </Botao>
    );

  return (
    <form action={acao} className="space-y-3 rounded-lg border border-erro/30 p-4">
      <input type="hidden" name="exameId" value={exameId} />
      <p className="text-xs text-texto-suave">
        A previsão atual é descartada e o exame volta a aguardar amostra. A amostra rejeitada fica no histórico.
      </p>
      <div>
        <label htmlFor="motivo-rejeicao" className="mb-1 block text-sm font-medium text-texto">
          Motivo da rejeição
        </label>
        <textarea
          id="motivo-rejeicao"
          name="motivo"
          required
          maxLength={500}
          rows={2}
          value={motivo}
          onChange={(e) => setMotivo(e.target.value)}
          placeholder="Ex.: material hemolisado, volume insuficiente"
          className="w-full rounded-lg border border-borda bg-white px-3 py-2.5 text-sm outline-none transition focus:border-primaria focus:ring-2 focus:ring-primaria/40"
        />
      </div>
      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}
      <div className="flex gap-3">
        <Botao type="button" variante="secundaria" tamanho="sm" onClick={() => setAberto(false)}>
          Cancelar
        </Botao>
        <Botao type="submit" variante="perigo" tamanho="sm" carregando={rejeitando} disabled={!motivo.trim()}>
          Confirmar rejeição
        </Botao>
      </div>
    </form>
  );
}
