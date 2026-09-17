"use client";

import { useActionState, useState } from "react";
import { excluirExame, type EstadoExcluirExame } from "../actions";
import { Botao } from "@/components/ui/botao";

/** Exclusão lógica (C2): qualquer funcionário, em qualquer etapa, com motivo obrigatório (P10). */
export function ExclusaoExame({ exameId }: { exameId: string }) {
  const [aberto, setAberto] = useState(false);
  const [motivo, setMotivo] = useState("");
  const [estado, acao, excluindo] = useActionState<EstadoExcluirExame, FormData>(excluirExame, {});

  return (
    <section className="rounded-2xl border border-erro/30 bg-white p-5 shadow-sm">
      <h2 className="font-semibold text-erro">Excluir exame</h2>
      <p className="mt-1 text-xs text-texto-suave">
        O exame sai das telas e dos números da operação, mas continua guardado com quem excluiu, quando e por quê.
      </p>

      {!aberto ? (
        <Botao variante="perigo" tamanho="sm" className="mt-4" onClick={() => setAberto(true)}>
          Excluir…
        </Botao>
      ) : (
        <form action={acao} className="mt-4 space-y-3">
          <input type="hidden" name="id" value={exameId} />
          <div>
            <label htmlFor="motivo" className="mb-1 block text-sm font-medium text-texto">
              Motivo da exclusão
            </label>
            <textarea
              id="motivo"
              name="motivo"
              required
              maxLength={500}
              rows={3}
              value={motivo}
              onChange={(e) => setMotivo(e.target.value)}
              placeholder="Ex.: cadastrado no paciente errado"
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
            <Botao type="submit" variante="perigo" tamanho="sm" carregando={excluindo} disabled={!motivo.trim()}>
              Confirmar exclusão
            </Botao>
          </div>
        </form>
      )}
    </section>
  );
}
