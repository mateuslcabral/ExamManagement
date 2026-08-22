"use server";

import { revalidatePath } from "next/cache";
import { z } from "zod";
import { api, ApiError, type Usuario } from "@/lib/api";

export type EstadoCriar = { erro?: string; sucesso?: string; campos?: { nome?: string; email?: string } };

const esquemaCriar = z.object({
  nome: z.string().trim().min(2, "Informe o nome completo."),
  email: z.string().trim().toLowerCase().email("Informe um e-mail válido."),
});

export async function criarUsuario(_anterior: EstadoCriar, dados: FormData): Promise<EstadoCriar> {
  const parse = esquemaCriar.safeParse({ nome: dados.get("nome"), email: dados.get("email") });
  if (!parse.success) {
    const campos: EstadoCriar["campos"] = {};
    for (const issue of parse.error.issues) {
      const c = issue.path[0] as "nome" | "email";
      campos[c] ??= issue.message;
    }
    return { campos };
  }

  try {
    const criado = await api<Usuario>("/api/usuarios", { method: "POST", body: parse.data });
    revalidatePath("/usuarios");
    const sucesso =
      criado.tipoLogin === "Google"
        ? `${criado.nome} já pode entrar com a conta Google.`
        : `${criado.nome} recebeu um e-mail para definir a senha.`;
    return { sucesso };
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
}

async function acaoSimples(id: string, sufixo: string) {
  try {
    await api(`/api/usuarios/${id}/${sufixo}`, { method: "POST" });
    revalidatePath("/usuarios");
    return { ok: true as const };
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
}

export async function desativarUsuario(id: string) {
  return acaoSimples(id, "desativar");
}
export async function reativarUsuario(id: string) {
  return acaoSimples(id, "reativar");
}
export async function reenviarDefinicaoSenha(id: string) {
  return acaoSimples(id, "reenviar-definicao-senha");
}
