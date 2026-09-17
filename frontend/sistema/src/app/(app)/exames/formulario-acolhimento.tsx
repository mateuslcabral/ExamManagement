"use client";

import { useActionState, useState } from "react";
import { formatarData, somarDias } from "@/lib/formatos";
import { acolherAmostra, type EstadoAmostra } from "./actions";
import { Botao } from "@/components/ui/botao";

/**
 * Registro do acolhimento. A data é digitável (hoje por padrão) porque a amostra pode chegar num dia e ser lançada
 * noutro (Q1.2, a confirmar). A previsão mostrada é a que a API vai gravar: prazo atual do catálogo + revisão.
 */
export function FormularioAcolhimento({
  exameId,
  dataEntrada,
  prazoEntregaDias,
  hoje,
  compacto = false,
}: {
  exameId: string;
  dataEntrada: string;
  /** Prazo de execução + dias de revisão vigentes. Ausente quando não se conhece (a API calcula de qualquer forma). */
  prazoEntregaDias?: number;
  hoje: string;
  compacto?: boolean;
}) {
  const [data, setData] = useState(hoje);
  const [estado, acao, salvando] = useActionState<EstadoAmostra, FormData>(acolherAmostra, {});
  const dataValida = /^\d{4}-\d{2}-\d{2}$/.test(data) && data >= dataEntrada && data <= hoje;

  return (
    <form action={acao} className={compacto ? "flex flex-wrap items-center justify-end gap-2" : "space-y-3"}>
      <input type="hidden" name="exameId" value={exameId} />
      <div className={compacto ? "" : "flex flex-wrap items-end gap-3"}>
        <div>
          <label htmlFor={`acolhimento-${exameId}`} className={compacto ? "sr-only" : "mb-1 block text-sm font-medium text-texto"}>
            Data de acolhimento
          </label>
          <input
            id={`acolhimento-${exameId}`}
            type="date"
            name="dataAcolhimento"
            required
            min={dataEntrada}
            max={hoje}
            value={data}
            onChange={(e) => setData(e.target.value)}
            className="rounded-lg border border-borda bg-white px-3 py-2 text-sm outline-none transition focus:border-primaria focus:ring-2 focus:ring-primaria/40"
          />
        </div>
        {!compacto && (
          <Botao type="submit" carregando={salvando} disabled={!dataValida}>
            Registrar acolhimento
          </Botao>
        )}
      </div>
      {compacto && (
        <Botao type="submit" tamanho="sm" carregando={salvando} disabled={!dataValida}>
          Acolher
        </Botao>
      )}

      {prazoEntregaDias !== undefined && dataValida && !estado.sucesso && (
        <p className={`text-xs text-texto-suave ${compacto ? "w-full text-right" : ""}`}>
          Liberação prevista: <span className="font-semibold text-texto">{formatarData(somarDias(data, prazoEntregaDias))}</span>
          {!compacto && ` (${prazoEntregaDias} dias corridos após o acolhimento). Depois de gravada, a data não muda.`}
        </p>
      )}
      {estado.erro && (
        <p role="alert" className={`rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro ${compacto ? "w-full" : ""}`}>
          {estado.erro}
        </p>
      )}
      {estado.sucesso && (
        <p role="status" className={`rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso ${compacto ? "w-full" : ""}`}>
          {estado.sucesso}
        </p>
      )}
    </form>
  );
}
