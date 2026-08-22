"use client";

import { useState, useTransition } from "react";
import type { Usuario } from "@/lib/api";
import { desativarUsuario, reativarUsuario, reenviarDefinicaoSenha } from "./actions";
import { Botao } from "@/components/ui/botao";

export function TabelaUsuarios({ usuarios, meuId }: { usuarios: Usuario[]; meuId?: string }) {
  const [pendente, iniciar] = useTransition();
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
          className={`m-4 rounded-lg px-3 py-2 text-sm ${
            mensagem.tipo === "erro" ? "bg-erro/10 text-erro" : "bg-sucesso/10 text-sucesso"
          }`}
        >
          {mensagem.texto}
        </p>
      )}

      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
              <th className="px-4 py-3 font-semibold">Usuário</th>
              <th className="px-4 py-3 font-semibold">Login</th>
              <th className="px-4 py-3 font-semibold">Situação</th>
              <th className="px-4 py-3 text-right font-semibold">Ações</th>
            </tr>
          </thead>
          <tbody>
            {usuarios.length === 0 && (
              <tr>
                <td colSpan={4} className="px-4 py-8 text-center text-texto-suave">
                  Nenhum usuário cadastrado.
                </td>
              </tr>
            )}
            {usuarios.map((u) => {
              const souEu = u.id === meuId;
              return (
                <tr key={u.id} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
                  <td className="px-4 py-3">
                    <p className="font-medium text-texto">
                      {u.nome}
                      {souEu && <span className="ml-2 text-xs font-normal text-texto-suave">(você)</span>}
                    </p>
                    <p className="text-xs text-texto-suave">{u.email}</p>
                  </td>
                  <td className="px-4 py-3">
                    <Etiqueta cor={u.tipoLogin === "Google" ? "teal" : "cinza"}>
                      {u.tipoLogin === "Google" ? "Google" : "Senha"}
                    </Etiqueta>
                  </td>
                  <td className="px-4 py-3">
                    {!u.ativo && u.tipoLogin === "Local" && !u.senhaDefinida ? (
                      <Etiqueta cor="amarelo">Aguardando senha</Etiqueta>
                    ) : u.ativo ? (
                      <Etiqueta cor="verde">Ativo</Etiqueta>
                    ) : (
                      <Etiqueta cor="vermelho">Inativo</Etiqueta>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      {u.tipoLogin === "Local" && (
                        <Botao
                          variante="fantasma"
                          tamanho="sm"
                          disabled={pendente}
                          onClick={() =>
                            executar(() => reenviarDefinicaoSenha(u.id), `Link de senha reenviado para ${u.email}.`)
                          }
                        >
                          Reenviar senha
                        </Botao>
                      )}
                      {u.ativo ? (
                        <Botao
                          variante="perigo"
                          tamanho="sm"
                          disabled={pendente || souEu}
                          title={souEu ? "Você não pode desativar a si mesmo" : undefined}
                          onClick={() => executar(() => desativarUsuario(u.id), `${u.nome} desativado.`)}
                        >
                          Desativar
                        </Botao>
                      ) : (
                        <Botao
                          variante="secundaria"
                          tamanho="sm"
                          disabled={pendente || (u.tipoLogin === "Local" && !u.senhaDefinida)}
                          title={
                            u.tipoLogin === "Local" && !u.senhaDefinida
                              ? "Precisa definir a senha primeiro — use Reenviar senha"
                              : undefined
                          }
                          onClick={() => executar(() => reativarUsuario(u.id), `${u.nome} reativado.`)}
                        >
                          Reativar
                        </Botao>
                      )}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function Etiqueta({ cor, children }: { cor: "teal" | "cinza" | "verde" | "amarelo" | "vermelho"; children: React.ReactNode }) {
  const cores = {
    teal: "bg-teal/10 text-teal",
    cinza: "bg-fundo-alt text-texto-suave",
    verde: "bg-sucesso/10 text-sucesso",
    amarelo: "bg-alerta/10 text-alerta",
    vermelho: "bg-erro/10 text-erro",
  };
  return <span className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold ${cores[cor]}`}>{children}</span>;
}
