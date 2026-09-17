"use client";

import { useActionState } from "react";
import { entrar, type EstadoLogin } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";

export function FormularioLogin({ callbackUrl, erroInicial }: { callbackUrl?: string; erroInicial?: string }) {
  const [estado, acao, pendente] = useActionState<EstadoLogin, FormData>(entrar, {
    erro: erroInicial || undefined,
  });

  return (
    <form action={acao} className="mt-6 space-y-4">
      <input type="hidden" name="callbackUrl" value={callbackUrl ?? "/"} />

      <Campo
        label="E-mail"
        name="email"
        type="email"
        autoComplete="username"
        required
        autoFocus
        placeholder="voce@cligen.com.br"
      />
      <Campo
        label="Senha"
        name="senha"
        type="password"
        autoComplete="current-password"
        required
        placeholder="••••••••"
      />

      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}

      <Botao type="submit" className="w-full" carregando={pendente}>
        Entrar
      </Botao>

      <p className="text-center text-xs text-texto-suave">
        Não tem acesso? Peça a um membro da equipe para cadastrar você na Gestão de Usuários.
      </p>
    </form>
  );
}
