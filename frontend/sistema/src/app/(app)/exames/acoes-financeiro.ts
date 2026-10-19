"use server";

import { revalidatePath } from "next/cache";
import { z } from "zod";
import { api, ApiError, type FinanceiroExame } from "@/lib/api";
import { lerValorMonetario } from "@/lib/formatos";

export type EstadoFinanceiro = { erro?: string; sucesso?: string; campos?: Record<string, string> };

function revalidar(exameId: string) {
  revalidatePath(`/exames/${exameId}`);
  revalidatePath("/financeiro");
}

const valorMonetario = (mensagem: string) =>
  z.string().trim().min(1, mensagem).transform(lerValorMonetario).pipe(z.number({ error: "Informe um valor como 1.250,90." }));

const esquemaLancamento = z
  .object({
    exameId: z.uuid(),
    metodo: z.enum(["Pix", "CartaoCredito", "CartaoDebito", "Dinheiro", "Boleto", "Transferencia", "Convenio"], "Selecione o método."),
    valor: valorMonetario("Informe o valor."),
    dataRecebimento: z.string().trim().default(""),
    convenioId: z.string().trim().default(""),
    numeroGuia: z.string().trim().default(""),
    parcelas: z.string().trim().default(""),
    observacao: z.string().trim().max(300, "Use no máximo 300 caracteres.").default(""),
  })
  .superRefine((d, ctx) => {
    if (d.metodo === "Convenio" && !d.convenioId)
      ctx.addIssue({ code: "custom", path: ["convenioId"], message: "Selecione o convênio." });
    if (d.dataRecebimento && !/^\d{4}-\d{2}-\d{2}$/.test(d.dataRecebimento))
      ctx.addIssue({ code: "custom", path: ["dataRecebimento"], message: "Data inválida." });
    if (d.parcelas && !/^\d{1,2}$/.test(d.parcelas))
      ctx.addIssue({ code: "custom", path: ["parcelas"], message: "Informe o número de parcelas." });
  });

export async function lancarPagamento(_anterior: EstadoFinanceiro, dados: FormData): Promise<EstadoFinanceiro> {
  const parse = esquemaLancamento.safeParse(Object.fromEntries(dados));
  if (!parse.success) {
    const campos: Record<string, string> = {};
    for (const issue of parse.error.issues) campos[String(issue.path[0])] ??= issue.message;
    return { campos };
  }
  const d = parse.data;
  try {
    await api<FinanceiroExame>(`/api/exames/${d.exameId}/pagamentos`, {
      method: "POST",
      body: {
        metodo: d.metodo,
        valor: d.valor,
        dataRecebimento: d.metodo !== "Convenio" && d.dataRecebimento ? d.dataRecebimento : null,
        convenioId: d.metodo === "Convenio" ? d.convenioId : null,
        numeroGuia: d.metodo === "Convenio" ? d.numeroGuia || null : null,
        parcelas: d.metodo === "CartaoCredito" && d.parcelas ? Number(d.parcelas) : null,
        valorParcela: null,
        observacao: d.observacao || null,
      },
    });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
  revalidar(d.exameId);
  return { sucesso: "Lançamento registrado." };
}

export async function aplicarDesconto(_anterior: EstadoFinanceiro, dados: FormData): Promise<EstadoFinanceiro> {
  const exameId = String(dados.get("exameId") ?? "");
  const desconto = lerValorMonetario(String(dados.get("desconto") ?? "0") || "0");
  if (desconto === null) return { campos: { desconto: "Informe um valor como 150,00." } };
  const motivo = String(dados.get("motivo") ?? "").trim();
  try {
    await api<FinanceiroExame>(`/api/exames/${exameId}/desconto`, { method: "PUT", body: { desconto, motivo: motivo || null } });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
  revalidar(exameId);
  return { sucesso: desconto > 0 ? "Desconto aplicado." : "Desconto removido." };
}

async function acaoPagamento(exameId: string, caminho: string, corpo: unknown) {
  try {
    await api<FinanceiroExame>(caminho, { method: "POST", body: corpo });
    revalidar(exameId);
    return { ok: true as const };
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
}

export async function marcarRecebido(exameId: string, pagamentoId: string, dataRecebimento: string) {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(dataRecebimento)) return { ok: false as const, erro: "Informe a data de recebimento." };
  return acaoPagamento(exameId, `/api/pagamentos/${pagamentoId}/receber`, { dataRecebimento });
}

export async function glosarPagamento(exameId: string, pagamentoId: string, motivo: string) {
  if (!motivo.trim()) return { ok: false as const, erro: "Informe o motivo da glosa." };
  return acaoPagamento(exameId, `/api/pagamentos/${pagamentoId}/glosar`, { motivo: motivo.trim() });
}

export async function excluirPagamento(exameId: string, pagamentoId: string, motivo: string) {
  if (!motivo.trim()) return { ok: false as const, erro: "Informe o motivo da exclusão." };
  return acaoPagamento(exameId, `/api/pagamentos/${pagamentoId}/excluir`, { motivo: motivo.trim() });
}
