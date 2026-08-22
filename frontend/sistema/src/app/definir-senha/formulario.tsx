"use client";

import { useActionState } from "react";
import { definirSenha, type EstadoDefinirSenha } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";

export function FormularioDefinirSenha({ token }: { token: string }) {
  const [estado, acao, pendente] = useActionState<EstadoDefinirSenha, FormData>(definirSenha, {});

  return (
    <form action={acao} className="mt-6 space-y-4">
      <input type="hidden" name="token" value={token} />
      <Campo
        label="Nova senha"
        name="senha"
        type="password"
        autoComplete="new-password"
        required
        minLength={8}
        autoFocus
        dica="Mínimo de 8 caracteres."
      />
      <Campo label="Confirmar senha" name="confirmacao" type="password" autoComplete="new-password" required minLength={8} />

      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}

      <Botao type="submit" className="w-full" carregando={pendente}>
        Salvar senha
      </Botao>
    </form>
  );
}
