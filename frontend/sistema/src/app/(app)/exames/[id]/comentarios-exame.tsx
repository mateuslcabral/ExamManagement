"use client";

import { useActionState, useState } from "react";
import type { Comentario } from "@/lib/api";
import { formatarDataHora } from "@/lib/formatos";
import { adicionarComentario, type EstadoComentario } from "../actions";
import { Botao } from "@/components/ui/botao";

/** Anotações da equipe sobre o exame em andamento. Só inserção: não se edita nem se apaga (prontuário). */
export function ComentariosExame({ exameId, comentarios }: { exameId: string; comentarios: Comentario[] }) {
  const [texto, setTexto] = useState("");
  const [estado, acao, salvando] = useActionState<EstadoComentario, FormData>(async (anterior, dados) => {
    const r = await adicionarComentario(anterior, dados);
    if (r.sucesso) setTexto("");
    return r;
  }, {});

  return (
    <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="font-semibold text-texto">Comentários</h2>
          <p className="mt-1 text-xs text-texto-suave">Anotações internas sobre o andamento. Ficam registradas com autor e hora; não são editadas nem apagadas.</p>
        </div>
        <span className="text-xs text-texto-suave">{comentarios.length}</span>
      </div>

      <form action={acao} className="mt-4 space-y-2">
        <input type="hidden" name="exameId" value={exameId} />
        <textarea
          name="texto"
          rows={2}
          maxLength={2000}
          required
          value={texto}
          onChange={(e) => setTexto(e.target.value)}
          placeholder="Ex.: paciente ligou perguntando do prazo; laboratório avisou atraso de 2 dias."
          className="w-full rounded-lg border border-borda bg-white px-3 py-2.5 text-sm outline-none transition focus:border-primaria focus:ring-2 focus:ring-primaria/40"
        />
        <div className="flex items-center gap-3">
          <Botao type="submit" tamanho="sm" variante="secundaria" carregando={salvando} disabled={!texto.trim()}>
            Registrar comentário
          </Botao>
          {estado.erro && (
            <span role="alert" className="text-xs text-erro">
              {estado.erro}
            </span>
          )}
        </div>
      </form>

      {comentarios.length > 0 && (
        <ol className="mt-4 space-y-3 border-t border-borda/60 pt-4">
          {comentarios.map((c) => (
            <li key={c.id} className="text-sm">
              <p className="whitespace-pre-wrap text-texto">{c.texto}</p>
              <p className="mt-0.5 text-xs text-texto-suave">
                {c.autorNome} · {formatarDataHora(c.criadoEm)}
              </p>
            </li>
          ))}
        </ol>
      )}
    </section>
  );
}
