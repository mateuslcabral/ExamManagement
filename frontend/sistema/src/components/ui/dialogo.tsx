"use client";

import { useEffect, useRef } from "react";
import { Botao } from "./botao";

/**
 * Diálogo de confirmação próprio (não usa window.confirm): fica no mesmo visual, aceita ESC/clique fora,
 * e não bloqueia o restante da página como o modal nativo.
 */
export function Dialogo({
  aberto,
  titulo,
  children,
  confirmar = "Confirmar",
  cancelar = "Cancelar",
  perigo = false,
  carregando = false,
  onConfirmar,
  onCancelar,
}: {
  aberto: boolean;
  titulo: string;
  children?: React.ReactNode;
  confirmar?: string;
  cancelar?: string;
  perigo?: boolean;
  carregando?: boolean;
  onConfirmar: () => void;
  onCancelar: () => void;
}) {
  const ref = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    if (aberto && !el.open) el.showModal();
    if (!aberto && el.open) el.close();
  }, [aberto]);

  return (
    <dialog
      ref={ref}
      onCancel={(e) => {
        e.preventDefault();
        if (!carregando) onCancelar();
      }}
      onClick={(e) => {
        if (e.target === ref.current && !carregando) onCancelar();
      }}
      className="m-auto w-full max-w-md rounded-2xl border border-borda bg-white p-0 text-texto shadow-2xl backdrop:bg-black/40"
    >
      <div className="p-6">
        <h2 className="text-lg font-semibold text-primaria">{titulo}</h2>
        {children && <div className="mt-2 text-sm text-texto-suave">{children}</div>}
        <div className="mt-6 flex justify-end gap-3">
          <Botao type="button" variante="secundaria" tamanho="sm" onClick={onCancelar} disabled={carregando}>
            {cancelar}
          </Botao>
          <Botao type="button" variante={perigo ? "perigo" : "primaria"} tamanho="sm" onClick={onConfirmar} carregando={carregando}>
            {confirmar}
          </Botao>
        </div>
      </div>
    </dialog>
  );
}
