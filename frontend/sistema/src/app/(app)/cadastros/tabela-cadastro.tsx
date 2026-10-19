"use client";

import { useActionState, useState, useTransition } from "react";
import type { Cadastro, TipoCadastro } from "@/lib/api";
import { desativarCadastro, reativarCadastro, salvarCadastro, type EstadoCadastro } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Etiqueta } from "@/components/ui/etiqueta";

/** Lista + formulário de um cadastro auxiliar (origens, destinos ou convênios). Mesmo padrão do catálogo. */
export function TabelaCadastro({
  tipo,
  itens,
  rotuloSingular,
  descricao,
}: {
  tipo: TipoCadastro;
  itens: Cadastro[];
  rotuloSingular: string;
  descricao: string;
}) {
  const [emEdicao, setEmEdicao] = useState<Cadastro | null>(null);
  const [versao, setVersao] = useState(0);
  const [estado, acao, salvando] = useActionState<EstadoCadastro, FormData>(async (anterior, dados) => {
    const r = await salvarCadastro(anterior, dados);
    if (r.sucesso) {
      setEmEdicao(null);
      setVersao((v) => v + 1);
    }
    return r;
  }, {});
  const [alterando, iniciar] = useTransition();
  const [mensagem, setMensagem] = useState<{ tipo: "ok" | "erro"; texto: string } | null>(null);

  const alvo = `${emEdicao?.id ?? "novo"}#${versao}`;
  const estadoDoAlvo = estado.alvo === alvo ? estado : {};

  function alternar(item: Cadastro) {
    setMensagem(null);
    iniciar(async () => {
      const r = item.ativo ? await desativarCadastro(tipo, item.id) : await reativarCadastro(tipo, item.id);
      setMensagem(r.ok ? { tipo: "ok", texto: `${item.nome} ${item.ativo ? "desativado" : "reativado"}.` } : { tipo: "erro", texto: r.erro ?? "Falha." });
    });
  }

  return (
    <div className="grid gap-6 lg:grid-cols-[320px_1fr]">
      <section className="h-fit rounded-2xl border border-borda bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-texto">{emEdicao ? `Renomear ${rotuloSingular}` : `Nova ${rotuloSingular}`}</h2>
        <p className="mt-1 text-xs text-texto-suave">{descricao}</p>
        <form key={versao} action={acao} className="mt-4 space-y-4">
          <input type="hidden" name="tipo" value={tipo} />
          <input type="hidden" name="alvo" value={alvo} />
          {emEdicao && <input type="hidden" name="id" value={emEdicao.id} />}
          <Campo label="Nome" name="nome" required maxLength={150} autoComplete="off" defaultValue={estadoDoAlvo.valor ?? emEdicao?.nome} erro={estadoDoAlvo.erro} />
          {estado.sucesso && !emEdicao && (
            <p role="status" className="rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
              {estado.sucesso}
            </p>
          )}
          <div className="flex gap-3">
            {emEdicao && (
              <Botao type="button" variante="secundaria" className="flex-1" onClick={() => { setEmEdicao(null); setVersao((v) => v + 1); }}>
                Cancelar
              </Botao>
            )}
            <Botao type="submit" className="flex-1" carregando={salvando}>
              {emEdicao ? "Salvar" : "Adicionar"}
            </Botao>
          </div>
        </form>
      </section>

      <section className="h-fit rounded-2xl border border-borda bg-white shadow-sm">
        {mensagem && (
          <p role={mensagem.tipo === "erro" ? "alert" : "status"} className={`m-4 rounded-lg px-3 py-2 text-sm ${mensagem.tipo === "erro" ? "bg-erro/10 text-erro" : "bg-sucesso/10 text-sucesso"}`}>
            {mensagem.texto}
          </p>
        )}
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
              <th className="px-4 py-3 font-semibold">Nome</th>
              <th className="px-4 py-3 font-semibold">Situação</th>
              <th className="px-4 py-3 text-right font-semibold">Ações</th>
            </tr>
          </thead>
          <tbody>
            {itens.length === 0 && (
              <tr>
                <td colSpan={3} className="px-4 py-8 text-center text-texto-suave">Nada cadastrado.</td>
              </tr>
            )}
            {itens.map((c) => (
              <tr key={c.id} className={`border-b border-borda/60 last:border-0 ${emEdicao?.id === c.id ? "bg-primaria/5" : "hover:bg-fundo"} ${c.ativo ? "" : "text-texto-suave"}`}>
                <td className="px-4 py-3 font-medium">{c.nome}</td>
                <td className="px-4 py-3">{c.ativo ? <Etiqueta cor="verde">Ativo</Etiqueta> : <Etiqueta cor="cinza">Inativo</Etiqueta>}</td>
                <td className="px-4 py-3">
                  <div className="flex justify-end gap-2">
                    <Botao variante="fantasma" tamanho="sm" disabled={alterando || emEdicao?.id === c.id} onClick={() => { setEmEdicao(c); setVersao((v) => v + 1); }}>
                      Renomear
                    </Botao>
                    <Botao variante={c.ativo ? "perigo" : "secundaria"} tamanho="sm" disabled={alterando} onClick={() => alternar(c)}>
                      {c.ativo ? "Desativar" : "Reativar"}
                    </Botao>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}
