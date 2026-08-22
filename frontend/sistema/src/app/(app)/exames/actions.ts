"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { z } from "zod";
import { auth } from "@/auth";
import { api, apiFormulario, ApiError, type Exame } from "@/lib/api";

export type CamposExame = "pacienteId" | "exameCatalogoId" | "origem" | "destino" | "tipoMedico" | "nomeMedico" | "preco";

export type EstadoExame = {
  erro?: string;
  campos?: Partial<Record<CamposExame, string>>;
  valores?: Partial<Record<CamposExame, string>>;
};

const origem = z.enum(["Cligen", "ClinicaParceira", "SiteCligen", "Plataforma"], { message: "Selecione a origem." });
const tipoMedico = z.enum(["Interno", "Externo"], { message: "Selecione o tipo de médico." });

const base = z
  .object({
    origem,
    destino: z.string().trim().max(200, "Destino muito longo."),
    tipoMedico,
    nomeMedico: z.string().trim(),
    preco: z.string().trim(),
  })
  .superRefine((d, ctx) => {
    if (d.tipoMedico === "Externo" && d.nomeMedico.length < 2)
      ctx.addIssue({ code: "custom", path: ["nomeMedico"], message: "Informe o nome do médico externo." });
    if (d.preco && Number.isNaN(lerPreco(d.preco)))
      ctx.addIssue({ code: "custom", path: ["preco"], message: "Preço inválido." });
  });

const esquemaCriar = z
  .object({
    pacienteId: z.string().uuid("Selecione o paciente."),
    exameCatalogoId: z.string().uuid("Selecione o exame do catálogo."),
  })
  .and(base);

function lerPreco(texto: string): number {
  return Number(texto.replace(/\./g, "").replace(",", "."));
}

function lerFormulario(dados: FormData): Record<CamposExame, string> {
  const v = (k: string) => String(dados.get(k) ?? "");
  return {
    pacienteId: v("pacienteId"),
    exameCatalogoId: v("exameCatalogoId"),
    origem: v("origem"),
    destino: v("destino"),
    tipoMedico: v("tipoMedico"),
    nomeMedico: v("nomeMedico"),
    preco: v("preco"),
  };
}

function mapearErros(issues: z.core.$ZodIssue[], valores: Record<CamposExame, string>): EstadoExame {
  const campos: EstadoExame["campos"] = {};
  for (const issue of issues) {
    const c = issue.path[0] as CamposExame;
    campos[c] ??= issue.message;
  }
  return { campos, valores };
}

export async function criarExame(_a: EstadoExame, dados: FormData): Promise<EstadoExame> {
  const valores = lerFormulario(dados);
  const parse = esquemaCriar.safeParse(valores);
  if (!parse.success) return mapearErros(parse.error.issues, valores);
  const d = parse.data;

  let criado: Exame;
  try {
    criado = await api<Exame>("/api/exames", {
      method: "POST",
      body: {
        pacienteId: d.pacienteId,
        exameCatalogoId: d.exameCatalogoId,
        origem: d.origem,
        destino: d.destino || null,
        tipoMedico: d.tipoMedico,
        nomeMedico: d.tipoMedico === "Externo" ? d.nomeMedico : null,
        preco: d.preco ? lerPreco(d.preco) : null,
      },
    });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message, valores };
    throw e;
  }
  revalidatePath("/exames");
  revalidatePath(`/pacientes/${d.pacienteId}`);
  redirect(`/exames/${criado.id}?criado=1`);
}

export async function atualizarExame(id: string, _a: EstadoExame, dados: FormData): Promise<EstadoExame> {
  const valores = lerFormulario(dados);
  const parse = base.safeParse(valores);
  if (!parse.success) return mapearErros(parse.error.issues, valores);
  const d = parse.data;
  if (!d.preco) return { campos: { preco: "Informe o preço." }, valores };

  try {
    await api<Exame>(`/api/exames/${id}`, {
      method: "PUT",
      body: {
        origem: d.origem,
        destino: d.destino || null,
        tipoMedico: d.tipoMedico,
        nomeMedico: d.tipoMedico === "Externo" ? d.nomeMedico : null,
        preco: lerPreco(d.preco),
      },
    });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message, valores };
    throw e;
  }
  revalidatePath("/exames");
  revalidatePath(`/exames/${id}`);
  redirect(`/exames/${id}?salvo=1`);
}

export async function excluirExame(id: string, pacienteId: string, motivo: string) {
  const sessao = await auth();
  const usuarioId = sessao?.user?.id;
  if (!usuarioId) return { ok: false as const, erro: "Sessão expirada. Entre novamente." };
  if (!motivo.trim()) return { ok: false as const, erro: "Informe o motivo da exclusão." };
  try {
    await api(`/api/exames/${id}/excluir`, { method: "POST", body: { usuarioId, motivo: motivo.trim() } });
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
  revalidar(id, pacienteId);
  return { ok: true as const };
}

export async function restaurarExame(id: string, pacienteId: string) {
  try {
    await api(`/api/exames/${id}/restaurar`, { method: "POST" });
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
  revalidar(id, pacienteId);
  return { ok: true as const };
}

export type EstadoAnexo = { erro?: string; sucesso?: string };

export async function enviarAnexo(exameId: string, _a: EstadoAnexo, dados: FormData): Promise<EstadoAnexo> {
  const arquivo = dados.get("arquivo");
  if (!(arquivo instanceof File) || arquivo.size === 0) return { erro: "Selecione um arquivo." };
  if (arquivo.size > 50 * 1024 * 1024) return { erro: "O arquivo excede o limite de 50 MB." };

  const form = new FormData();
  form.append("arquivo", arquivo, arquivo.name);
  try {
    await apiFormulario(`/api/exames/${exameId}/anexos`, form);
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
  revalidatePath(`/exames/${exameId}`);
  return { sucesso: `"${arquivo.name}" anexado.` };
}

export async function removerAnexo(exameId: string, anexoId: string) {
  try {
    await api(`/api/exames/${exameId}/anexos/${anexoId}`, { method: "DELETE" });
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
  revalidatePath(`/exames/${exameId}`);
  return { ok: true as const };
}

function revalidar(id: string, pacienteId: string) {
  revalidatePath("/exames");
  revalidatePath(`/exames/${id}`);
  revalidatePath(`/pacientes/${pacienteId}`);
  revalidatePath("/pacientes");
}

// ---- Amostra e laudo ----

export type EstadoAcao = { erro?: string; sucesso?: string };

async function usuarioLogado() {
  const sessao = await auth();
  return sessao?.user?.id ?? null;
}

export async function acolherAmostra(exameId: string, _a: EstadoAcao, dados: FormData): Promise<EstadoAcao> {
  const usuarioId = await usuarioLogado();
  if (!usuarioId) return { erro: "Sessão expirada. Entre novamente." };
  const data = String(dados.get("dataAcolhimento") ?? "").trim();
  if (!/^\d{4}-\d{2}-\d{2}$/.test(data)) return { erro: "Informe a data de acolhimento." };
  try {
    await api(`/api/exames/${exameId}/amostras`, { method: "POST", body: { usuarioId, dataAcolhimento: data } });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
  revalidatePath(`/exames/${exameId}`);
  revalidatePath("/exames");
  return { sucesso: "Amostra acolhida. Previsão de liberação calculada." };
}

export async function rejeitarAmostra(exameId: string, motivo: string) {
  const usuarioId = await usuarioLogado();
  if (!usuarioId) return { ok: false as const, erro: "Sessão expirada. Entre novamente." };
  if (!motivo.trim()) return { ok: false as const, erro: "Informe o motivo da rejeição." };
  try {
    await api(`/api/exames/${exameId}/amostras/rejeitar`, { method: "POST", body: { usuarioId, motivo: motivo.trim() } });
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
  revalidatePath(`/exames/${exameId}`);
  revalidatePath("/exames");
  return { ok: true as const };
}

export async function enviarEtapaLaudo(exameId: string, etapa: string, _a: EstadoAcao, dados: FormData): Promise<EstadoAcao> {
  const usuarioId = await usuarioLogado();
  if (!usuarioId) return { erro: "Sessão expirada. Entre novamente." };
  const arquivo = dados.get("arquivo");
  if (!(arquivo instanceof File) || arquivo.size === 0) return { erro: "Selecione o PDF." };
  if (arquivo.size > 50 * 1024 * 1024) return { erro: "O arquivo excede o limite de 50 MB." };

  const form = new FormData();
  form.append("arquivo", arquivo, arquivo.name);
  form.append("usuarioId", usuarioId);
  try {
    await apiFormulario(`/api/exames/${exameId}/laudo/${etapa}`, form);
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
  revalidatePath(`/exames/${exameId}`);
  revalidatePath("/exames");
  return { sucesso: `"${arquivo.name}" registrado.` };
}

export async function disponibilizarLaudo(exameId: string) {
  try {
    await api(`/api/exames/${exameId}/laudo/disponibilizar`, { method: "POST" });
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
  revalidatePath(`/exames/${exameId}`);
  revalidatePath("/exames");
  return { ok: true as const };
}
