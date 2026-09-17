"use client";

import { useActionState } from "react";
import { alterarDiasRevisao, type EstadoDiasRevisao } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";

export function FormularioDiasRevisao({ dias }: { dias: number }) {
  const [estado, acao, pendente] = useActionState<EstadoDiasRevisao, FormData>(alterarDiasRevisao, {});

  return (
    <section className="h-fit rounded-2xl border border-borda bg-white p-5 shadow-sm">
      <h2 className="font-semibold text-texto">Dias de revisão</h2>
      <p className="mt-1 text-xs text-texto-suave">
        Somados ao prazo de execução de todos os exames para chegar à entrega ao paciente. Mudar aqui não altera a
        data prevista de exames já acolhidos.
      </p>

      <form action={acao} className="mt-4 flex items-end gap-3">
        <Campo
          label="Dias corridos"
          name="dias"
          type="number"
          min={0}
          step={1}
          required
          defaultValue={estado.valor ?? dias}
          className="flex-1"
        />
        <Botao type="submit" variante="secundaria" carregando={pendente}>
          Salvar
        </Botao>
      </form>

      {estado.erro && (
        <p role="alert" className="mt-3 rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}
      {estado.sucesso && (
        <p role="status" className="mt-3 rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
          {estado.sucesso}
        </p>
      )}
    </section>
  );
}
