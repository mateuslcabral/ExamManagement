"use server";

import { redirect } from "next/navigation";
import { apiPublica, ApiError } from "@/lib/api";

export type EstadoDefinirSenha = { erro?: string };

export async function definirSenha(_anterior: EstadoDefinirSenha, dados: FormData): Promise<EstadoDefinirSenha> {
  const token = String(dados.get("token") ?? "");
  const senha = String(dados.get("senha") ?? "");
  const confirmacao = String(dados.get("confirmacao") ?? "");

  if (senha.length < 8) return { erro: "A senha deve ter pelo menos 8 caracteres." };
  if (senha !== confirmacao) return { erro: "As senhas não conferem." };

  try {
    await apiPublica("/api/auth/definir-senha", { method: "POST", body: { token, senha } });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }

  redirect("/login?senhaDefinida=1");
}
