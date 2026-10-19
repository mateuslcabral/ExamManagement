"use client";

import { useState, useTransition } from "react";
import { restaurarExame } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Dialogo } from "@/components/ui/dialogo";

/** Desfaz a exclusão lógica (C2: reversível). Redireciona para o exame restaurado. */
export function BotaoRestaurar({ exameId, nome }: { exameId: string; nome: string }) {
  const [pendente, iniciar] = useTransition();
  const [confirmando, setConfirmando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  return (
    <div className="text-right">
      <Botao variante="secundaria" tamanho="sm" carregando={pendente} onClick={() => setConfirmando(true)}>
        Restaurar
      </Botao>
      {erro && (
        <p role="alert" className="mt-1 text-xs text-erro">
          {erro}
        </p>
      )}
      <Dialogo
        aberto={confirmando}
        titulo="Restaurar o exame?"
        confirmar="Restaurar"
        carregando={pendente}
        onCancelar={() => setConfirmando(false)}
        onConfirmar={() => {
          setErro(null);
          iniciar(async () => {
            const r = await restaurarExame(exameId);
            setConfirmando(false);
            if (r && !r.ok) setErro(r.erro ?? "Falha.");
          });
        }}
      >
        <span className="font-medium text-texto">{nome}</span> volta às listas e ao fluxo normal.
      </Dialogo>
    </div>
  );
}
