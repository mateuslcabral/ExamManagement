"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import type { Paciente } from "@/lib/api";
import { Botao } from "@/components/ui/botao";
import { excluirPaciente, restaurarPaciente } from "../actions";

/** Editar / excluir (lógica, com motivo — P16) / restaurar. Sem diálogos nativos do navegador. */
export function AcoesPaciente({ paciente }: { paciente: Paciente }) {
  const router = useRouter();
  const [pendente, iniciar] = useTransition();
  const [confirmando, setConfirmando] = useState(false);
  const [motivo, setMotivo] = useState("");
  const [erro, setErro] = useState<string | null>(null);

  function executar(fn: () => Promise<{ ok: boolean; erro?: string }>) {
    setErro(null);
    iniciar(async () => {
      const r = await fn();
      if (!r.ok) {
        setErro(r.erro ?? "Falha.");
        return;
      }
      setConfirmando(false);
      setMotivo("");
      router.replace(`/pacientes/${paciente.id}`);
      router.refresh();
    });
  }

  if (paciente.excluido) {
    return (
      <div className="flex flex-col items-end gap-2">
        <Botao variante="secundaria" carregando={pendente} onClick={() => executar(() => restaurarPaciente(paciente.id))}>
          Restaurar cadastro
        </Botao>
        {erro && (
          <p role="alert" className="text-xs text-erro">
            {erro}
          </p>
        )}
      </div>
    );
  }

  return (
    <div className="flex flex-col items-end gap-2">
      <div className="flex gap-2">
        <Link
          href={`/pacientes/${paciente.id}?editar=1`}
          className="inline-flex items-center rounded-pill border border-primaria bg-white px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt"
        >
          Editar
        </Link>
        {!confirmando && (
          <Botao variante="perigo" onClick={() => setConfirmando(true)}>
            Excluir
          </Botao>
        )}
      </div>

      {confirmando && (
        <div className="w-full max-w-sm rounded-xl border border-erro/30 bg-white p-4 shadow-sm">
          <p className="text-sm font-semibold text-texto">Excluir {paciente.nome}?</p>
          <p className="mt-1 text-xs text-texto-suave">
            A exclusão é lógica: o cadastro sai das listagens, mas permanece armazenado com autor, data e motivo.
          </p>
          <label htmlFor="motivo-exclusao" className="mt-3 block text-sm font-medium text-texto">
            Motivo (obrigatório)
          </label>
          <textarea
            id="motivo-exclusao"
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
            <Botao variante="fantasma" tamanho="sm" disabled={pendente} onClick={() => setConfirmando(false)}>
              Cancelar
            </Botao>
            <Botao
              variante="perigo"
              tamanho="sm"
              carregando={pendente}
              disabled={!motivo.trim()}
              onClick={() => executar(() => excluirPaciente(paciente.id, motivo))}
            >
              Confirmar exclusão
            </Botao>
          </div>
        </div>
      )}
    </div>
  );
}
