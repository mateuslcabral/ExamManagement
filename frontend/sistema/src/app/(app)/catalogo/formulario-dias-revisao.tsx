"use client";

import { useActionState } from "react";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { atualizarDiasRevisao, type EstadoParametro } from "./actions";

export function FormularioDiasRevisao({ valorAtual }: { valorAtual: string }) {
  const [estado, acao, pendente] = useActionState<EstadoParametro, FormData>(atualizarDiasRevisao, {});
  return (
    <form action={acao} className="mt-4 flex items-end gap-3">
      <Campo label="Dias" name="valor" type="number" min={0} required defaultValue={valorAtual} className="w-28" />
      <Botao type="submit" variante="secundaria" carregando={pendente}>
        Salvar
      </Botao>
      {(estado.erro || estado.sucesso) && (
        <p role={estado.erro ? "alert" : "status"} className={`pb-2 text-xs ${estado.erro ? "text-erro" : "text-sucesso"}`}>
          {estado.erro ?? estado.sucesso}
        </p>
      )}
    </form>
  );
}
