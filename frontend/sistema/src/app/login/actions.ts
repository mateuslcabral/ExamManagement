"use server";

import { AuthError } from "next-auth";
import { signIn } from "@/auth";

export type EstadoLogin = { erro?: string };

export async function entrar(_anterior: EstadoLogin, dados: FormData): Promise<EstadoLogin> {
  const callbackUrl = String(dados.get("callbackUrl") || "/");
  try {
    await signIn("credentials", {
      email: dados.get("email"),
      senha: dados.get("senha"),
      redirectTo: callbackUrl.startsWith("/") ? callbackUrl : "/",
    });
    return {};
  } catch (e) {
    if (e instanceof AuthError) {
      return { erro: "E-mail ou senha inválidos, ou usuário sem acesso." };
    }
    throw e; // NEXT_REDIRECT e erros inesperados seguem o fluxo normal
  }
}
