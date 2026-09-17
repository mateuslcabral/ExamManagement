"use server";

import { revalidatePath } from "next/cache";
import { z } from "zod";
import { api, ApiError, type DiasRevisao, type ExameCatalogo } from "@/lib/api";
import { lerValorMonetario } from "@/lib/formatos";

type CamposExame = { nome?: string; prazo?: string; preco?: string };

/**
 * `alvo` identifica a instância do formulário que gerou o estado, para a tela não exibir erros de uma
 * tentativa anterior depois de trocar de exame ou cancelar. `valores` devolve o que foi digitado, porque o React
 * limpa o formulário após cada envio.
 */
export type EstadoSalvarExame = {
  alvo?: string;
  erro?: string;
  sucesso?: string;
  campos?: CamposExame;
  valores?: Required<CamposExame>;
};

const esquemaExame = z.object({
  nome: z.string().trim().min(2, "Informe o nome do exame.").max(200, "Use no máximo 200 caracteres."),
  prazo: z.coerce
    .number({ error: "Informe o prazo em dias." })
    .int("Use um número inteiro de dias.")
    .min(1, "O prazo deve ser de pelo menos 1 dia."),
  preco: z
    .string()
    .trim()
    .min(1, "Informe o preço de referência.")
    .transform(lerValorMonetario)
    .pipe(z.number({ error: "Informe um valor como 1.250,90." })),
});

export async function salvarExame(_anterior: EstadoSalvarExame, dados: FormData): Promise<EstadoSalvarExame> {
  const id = String(dados.get("id") ?? "");
  const alvo = String(dados.get("alvo") ?? "");
  const valores = {
    nome: String(dados.get("nome") ?? ""),
    prazo: String(dados.get("prazo") ?? ""),
    preco: String(dados.get("preco") ?? ""),
  };

  const parse = esquemaExame.safeParse(valores);
  if (!parse.success) {
    const campos: CamposExame = {};
    for (const issue of parse.error.issues) {
      const c = issue.path[0] as keyof CamposExame;
      campos[c] ??= issue.message;
    }
    return { alvo, campos, valores };
  }

  const corpo = {
    nome: parse.data.nome,
    prazoExecucaoDias: parse.data.prazo,
    precoReferencia: parse.data.preco,
  };

  try {
    const salvo = id
      ? await api<ExameCatalogo>(`/api/catalogo-exames/${id}`, { method: "PUT", body: corpo })
      : await api<ExameCatalogo>("/api/catalogo-exames", { method: "POST", body: corpo });
    revalidatePath("/catalogo");
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

export type EstadoDiasRevisao = { erro?: string; sucesso?: string; valor?: string };

const esquemaDias = z.coerce
  .number({ error: "Informe o número de dias." })
  .int("Use um número inteiro de dias.")
  .min(0, "Não pode ser negativo.");

export async function alterarDiasRevisao(_anterior: EstadoDiasRevisao, dados: FormData): Promise<EstadoDiasRevisao> {
  const valor = String(dados.get("dias") ?? "");
  const parse = esquemaDias.safeParse(valor.trim() === "" ? undefined : valor);
  if (!parse.success) return { erro: parse.error.issues[0]?.message, valor };

  try {
    const r = await api<DiasRevisao>("/api/parametros/dias-revisao", { method: "PUT", body: { dias: parse.data } });
    revalidatePath("/catalogo");
    return { sucesso: `Revisão ajustada para ${r.dias} ${r.dias === 1 ? "dia" : "dias"}.`, valor: String(r.dias) };
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message, valor };
    throw e;
  }
}
