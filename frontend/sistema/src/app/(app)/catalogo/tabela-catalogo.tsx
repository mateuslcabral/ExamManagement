"use client";

import { useState, useTransition } from "react";
import type { ExameCatalogo } from "@/lib/api";
import { formatarMoeda } from "@/lib/idade";
import { Botao } from "@/components/ui/botao";
import { Etiqueta } from "@/components/ui/etiqueta";
import { desativarItemCatalogo, reativarItemCatalogo } from "./actions";
import { FormularioCatalogo } from "./formulario-catalogo";

export function TabelaCatalogo({ itens }: { itens: ExameCatalogo[] }) {
  const [pendente, iniciar] = useTransition();
  const [editando, setEditando] = useState<string | null>(null);
  const [mensagem, setMensagem] = useState<{ tipo: "ok" | "erro"; texto: string } | null>(null);

  function executar(fn: () => Promise<{ ok: boolean; erro?: string }>, okTexto: string) {
    setMensagem(null);
    iniciar(async () => {
      const r = await fn();
      setMensagem(r.ok ? { tipo: "ok", texto: okTexto } : { tipo: "erro", texto: r.erro ?? "Falha." });
    });
  }

  return (
    <div>
      {mensagem && (
        <p
          role={mensagem.tipo === "erro" ? "alert" : "status"}
          className={`m-4 rounded-lg px-3 py-2 text-sm ${mensagem.tipo === "erro" ? "bg-erro/10 text-erro" : "bg-sucesso/10 text-sucesso"}`}
        >
          {mensagem.texto}
        </p>
      )}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
              <th className="px-4 py-3 font-semibold">Exame</th>
              <th className="px-4 py-3 font-semibold">Execução</th>
              <th className="px-4 py-3 font-semibold">Entrega ao paciente</th>
              <th className="px-4 py-3 font-semibold">Preço ref.</th>
              <th className="px-4 py-3 font-semibold">Situação</th>
              <th className="px-4 py-3 text-right font-semibold">Ações</th>
            </tr>
          </thead>
          <tbody>
            {itens.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center text-texto-suave">
                  Catálogo vazio.
                </td>
              </tr>
            )}
            {itens.map((i) =>
              editando === i.id ? (
                <tr key={i.id} className="border-b border-borda/60 bg-fundo">
                  <td colSpan={6} className="px-4 py-3">
                    <FormularioCatalogo item={i} diasRevisao={i.diasRevisao} aoConcluir={() => setEditando(null)} />
                  </td>
                </tr>
              ) : (
                <tr key={i.id} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
                  <td className="px-4 py-3 font-medium text-texto">{i.nome}</td>
                  <td className="px-4 py-3 text-texto">{i.prazoExecucaoDias} dias</td>
                  <td className="px-4 py-3 text-texto">
                    {i.prazoTotalDias} dias <span className="text-xs text-texto-suave">(+{i.diasRevisao} revisão)</span>
                  </td>
                  <td className="px-4 py-3 text-texto">{formatarMoeda(i.precoReferencia)}</td>
                  <td className="px-4 py-3">
                    {i.ativo ? <Etiqueta cor="verde">Ativo</Etiqueta> : <Etiqueta cor="cinza">Inativo</Etiqueta>}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <Botao variante="fantasma" tamanho="sm" disabled={pendente} onClick={() => setEditando(i.id)}>
                        Editar
                      </Botao>
                      {i.ativo ? (
                        <Botao
                          variante="perigo"
                          tamanho="sm"
                          disabled={pendente}
                          onClick={() => executar(() => desativarItemCatalogo(i.id), `"${i.nome}" desativado.`)}
                        >
                          Desativar
                        </Botao>
                      ) : (
                        <Botao
                          variante="secundaria"
                          tamanho="sm"
                          disabled={pendente}
                          onClick={() => executar(() => reativarItemCatalogo(i.id), `"${i.nome}" reativado.`)}
                        >
                          Reativar
                        </Botao>
                      )}
                    </div>
                  </td>
                </tr>
              ),
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
