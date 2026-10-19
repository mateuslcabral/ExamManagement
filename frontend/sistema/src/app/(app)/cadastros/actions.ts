"use server";

import { revalidatePath } from "next/cache";
import { z } from "zod";
import { api, ApiError, type Cadastro, type TipoCadastro } from "@/lib/api";

export type EstadoCadastro = { alvo?: string; erro?: string; sucesso?: string; valor?: string };

const tipos = z.enum(["origens", "destinos", "convenios"]);
const esquema = z.object({
  tipo: tipos,
  id: z.string().default(""),
  nome: z.string().trim().min(2, "Informe o nome.").max(150, "Use no máximo 150 caracteres."),
});

function revalidar() {
  // Os cadastros alimentam listas suspensas em várias telas.
  for (const p of ["/cadastros", "/catalogo", "/catalogo/gerenciar", "/exames", "/financeiro"]) revalidatePath(p);
}

export async function salvarCadastro(_anterior: EstadoCadastro, dados: FormData): Promise<EstadoCadastro> {
  const alvo = String(dados.get("alvo") ?? "");
  const parse = esquema.safeParse(Object.fromEntries(dados));
  if (!parse.success) return { alvo, erro: parse.error.issues[0]?.message, valor: String(dados.get("nome") ?? "") };
  const { tipo, id, nome } = parse.data;

  try {
    const salvo = id
      ? await api<Cadastro>(`/api/cadastros/${tipo}/${id}`, { method: "PUT", body: { nome } })
      : await api<Cadastro>(`/api/cadastros/${tipo}`, { method: "POST", body: { nome } });
    revalidar();
    return { alvo, sucesso: id ? `${salvo.nome} renomeado.` : `${salvo.nome} adicionado.` };
  } catch (e) {
    if (e instanceof ApiError) return { alvo, erro: e.message, valor: nome };
    throw e;
  }
}

async function acaoSimples(tipo: TipoCadastro, id: string, sufixo: string) {
  try {
    await api(`/api/cadastros/${tipo}/${id}/${sufixo}`, { method: "POST" });
    revalidar();
    return { ok: true as const };
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
}

export async function desativarCadastro(tipo: TipoCadastro, id: string) {
  return acaoSimples(tipo, id, "desativar");
}
export async function reativarCadastro(tipo: TipoCadastro, id: string) {
  return acaoSimples(tipo, id, "reativar");
}
