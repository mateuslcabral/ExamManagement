"use server";

import { revalidatePath } from "next/cache";
import { z } from "zod";
import { api, ApiError, type ExameCatalogo } from "@/lib/api";
import { lerValorMonetario } from "@/lib/formatos";

export type CampoCatalogo = "nome" | "prazo" | "precoReferencia" | "precoLimite" | "numeroGenes" | "genes" | "destinoId" | "informacoesTecnicas";

/**
 * `alvo` identifica a instância do formulário que gerou o estado, para a tela não exibir erros de uma
 * tentativa anterior depois de trocar de exame ou cancelar. `valores` devolve o que foi digitado, porque o React
 * limpa o formulário após cada envio.
 */
export type EstadoSalvarExame = {
  alvo?: string;
  erro?: string;
  sucesso?: string;
  campos?: Partial<Record<CampoCatalogo, string>>;
  valores?: Record<CampoCatalogo, string>;
};

const valorMonetario = (mensagem: string) =>
  z.string().trim().min(1, mensagem).transform(lerValorMonetario).pipe(z.number({ error: "Informe um valor como 1.250,90." }));

const esquemaExame = z
  .object({
    nome: z.string().trim().min(2, "Informe o nome do exame.").max(200, "Use no máximo 200 caracteres."),
    prazo: z.coerce.number({ error: "Informe o prazo em dias." }).int("Use um número inteiro de dias.").min(1, "O prazo deve ser de pelo menos 1 dia."),
    precoReferencia: valorMonetario("Informe o preço de referência."),
    precoLimite: z.string().trim().default("").transform((v) => (v ? lerValorMonetario(v) : 0)).pipe(z.number({ error: "Informe um valor como 1.250,90." })),
    numeroGenes: z.string().trim().default("").transform((v) => (v ? Number(v) : null)).pipe(z.number().int("Use um número inteiro.").min(0, "Não pode ser negativo.").nullable()),
    genes: z.string().trim().max(4000, "Use no máximo 4000 caracteres.").default(""),
    destinoId: z.string().trim().default(""),
    informacoesTecnicas: z.string().trim().max(4000, "Use no máximo 4000 caracteres.").default(""),
  })
  .superRefine((d, ctx) => {
    if (d.precoLimite > d.precoReferencia)
      ctx.addIssue({ code: "custom", path: ["precoLimite"], message: "O preço limite não pode ser maior que o de referência." });
  });

export async function salvarExame(_anterior: EstadoSalvarExame, dados: FormData): Promise<EstadoSalvarExame> {
  const id = String(dados.get("id") ?? "");
  const alvo = String(dados.get("alvo") ?? "");
  const valores = Object.fromEntries(
    (["nome", "prazo", "precoReferencia", "precoLimite", "numeroGenes", "genes", "destinoId", "informacoesTecnicas"] as CampoCatalogo[])
      .map((c) => [c, String(dados.get(c) ?? "")]),
  ) as Record<CampoCatalogo, string>;

  const parse = esquemaExame.safeParse(valores);
  if (!parse.success) {
    const campos: EstadoSalvarExame["campos"] = {};
    for (const issue of parse.error.issues) campos[issue.path[0] as CampoCatalogo] ??= issue.message;
    return { alvo, campos, valores };
  }

  const d = parse.data;
  const corpo = {
    nome: d.nome,
    prazoDias: d.prazo,
    precoReferencia: d.precoReferencia,
    precoLimite: d.precoLimite,
    numeroGenes: d.numeroGenes,
    genes: d.genes || null,
    destinoId: d.destinoId || null,
    informacoesTecnicas: d.informacoesTecnicas || null,
  };

  try {
    const salvo = id
      ? await api<ExameCatalogo>(`/api/catalogo-exames/${id}`, { method: "PUT", body: corpo })
      : await api<ExameCatalogo>("/api/catalogo-exames", { method: "POST", body: corpo });
    revalidatePath("/catalogo");
    revalidatePath("/catalogo/gerenciar");
    return { alvo, sucesso: id ? `${salvo.nome} atualizado.` : `${salvo.nome} adicionado ao catálogo.` };
  } catch (e) {
    if (e instanceof ApiError) return { alvo, erro: e.message, valores };
    throw e;
  }
}

async function acaoSimples(id: string, sufixo: string) {
  try {
    await api(`/api/catalogo-exames/${id}/${sufixo}`, { method: "POST" });
    revalidatePath("/catalogo");
    revalidatePath("/catalogo/gerenciar");
    return { ok: true as const };
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
}

export async function desativarExame(id: string) {
  return acaoSimples(id, "desativar");
}
export async function reativarExame(id: string) {
  return acaoSimples(id, "reativar");
}
