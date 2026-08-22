"use server";

import { revalidatePath } from "next/cache";
import { z } from "zod";
import { api, ApiError, type ExameCatalogo } from "@/lib/api";

export type EstadoCatalogo = {
  erro?: string;
  sucesso?: string;
  campos?: { nome?: string; prazoExecucaoDias?: string; precoReferencia?: string };
};

const esquema = z.object({
  nome: z.string().trim().min(2, "Informe o nome do exame."),
  prazoExecucaoDias: z.coerce.number().int("Use dias inteiros.").min(1, "Prazo deve ser maior que zero."),
  precoReferencia: z.coerce.number().min(0, "Preço não pode ser negativo."),
});

function ler(dados: FormData) {
  const preco = String(dados.get("precoReferencia") ?? "").replace(/\./g, "").replace(",", ".");
  return esquema.safeParse({
    nome: dados.get("nome"),
    prazoExecucaoDias: dados.get("prazoExecucaoDias"),
    precoReferencia: preco,
  });
}

function erros(parse: z.ZodSafeParseError<unknown>): EstadoCatalogo {
  const campos: EstadoCatalogo["campos"] = {};
  for (const issue of parse.error.issues) {
    const c = issue.path[0] as keyof NonNullable<EstadoCatalogo["campos"]>;
    campos[c] ??= issue.message;
  }
  return { campos };
}

export async function criarItemCatalogo(_a: EstadoCatalogo, dados: FormData): Promise<EstadoCatalogo> {
  const parse = ler(dados);
  if (!parse.success) return erros(parse);
  try {
    const criado = await api<ExameCatalogo>("/api/catalogo-exames", { method: "POST", body: parse.data });
    revalidatePath("/catalogo");
    return { sucesso: `"${criado.nome}" adicionado ao catálogo.` };
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
}

export async function atualizarItemCatalogo(id: string, _a: EstadoCatalogo, dados: FormData): Promise<EstadoCatalogo> {
  const parse = ler(dados);
  if (!parse.success) return erros(parse);
  try {
    await api<ExameCatalogo>(`/api/catalogo-exames/${id}`, { method: "PUT", body: parse.data });
    revalidatePath("/catalogo");
    return { sucesso: "Alterações salvas." };
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
}

async function acaoSimples(id: string, sufixo: string) {
  try {
    await api(`/api/catalogo-exames/${id}/${sufixo}`, { method: "POST" });
    revalidatePath("/catalogo");
    return { ok: true as const };
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
}
export async function desativarItemCatalogo(id: string) {
  return acaoSimples(id, "desativar");
}
export async function reativarItemCatalogo(id: string) {
  return acaoSimples(id, "reativar");
}

export type EstadoParametro = { erro?: string; sucesso?: string };

export async function atualizarDiasRevisao(_a: EstadoParametro, dados: FormData): Promise<EstadoParametro> {
  const valor = String(dados.get("valor") ?? "").trim();
  if (!/^\d+$/.test(valor)) return { erro: "Informe um número inteiro de dias." };
  try {
    await api("/api/parametros/DiasRevisao", { method: "PUT", body: { valor } });
    revalidatePath("/catalogo");
    return { sucesso: `Dias de revisão atualizados para ${valor}.` };
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
}
