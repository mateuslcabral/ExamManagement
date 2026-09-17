"use client";

import { useState, useTransition } from "react";
import { restaurarExame } from "./actions";
import { Botao } from "@/components/ui/botao";

/** Desfaz a exclusão lógica (C2: reversível). Redireciona para o exame restaurado. */
export function BotaoRestaurar({ exameId, nome }: { exameId: string; nome: string }) {
  const [pendente, iniciar] = useTransition();
  const [erro, setErro] = useState<string | null>(null);

  return (
    <div className="text-right">
      <Botao
        variante="secundaria"
        tamanho="sm"
        carregando={pendente}
        onClick={() => {
          if (!confirm(`Restaurar o exame "${nome}"? Ele volta às listas e ao fluxo normal.`)) return;
          setErro(null);
          iniciar(async () => {
            const r = await restaurarExame(exameId);
            if (r && !r.ok) setErro(r.erro ?? "Falha.");
          });
        }}
      >
        Restaurar
      </Botao>
      {erro && (
        <p role="alert" className="mt-1 text-xs text-erro">
          {erro}
        </p>
      )}
    </div>
  );
}
