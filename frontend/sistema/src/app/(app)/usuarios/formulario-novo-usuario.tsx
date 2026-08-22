"use client";

import { useActionState, useEffect, useRef } from "react";
import { criarUsuario, type EstadoCriar } from "./actions";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";

export function FormularioNovoUsuario() {
  const [estado, acao, pendente] = useActionState<EstadoCriar, FormData>(criarUsuario, {});
  const formRef = useRef<HTMLFormElement>(null);

  useEffect(() => {
    if (estado.sucesso) formRef.current?.reset();
  }, [estado]);

  return (
    <form ref={formRef} action={acao} className="mt-4 space-y-4">
      <Campo label="Nome completo" name="nome" required autoComplete="off" erro={estado.campos?.nome} />
      <Campo
        label="E-mail"
        name="email"
        type="email"
        required
        autoComplete="off"
        placeholder="nome@cligen.com.br"
        erro={estado.campos?.email}
      />

      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}
      {estado.sucesso && (
        <p role="status" className="rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
          {estado.sucesso}
        </p>
      )}

      <Botao type="submit" className="w-full" carregando={pendente}>
        Cadastrar
      </Botao>
    </form>
  );
}
