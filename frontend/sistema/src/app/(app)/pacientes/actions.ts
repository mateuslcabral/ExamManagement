"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { z } from "zod";
import { auth } from "@/auth";
import { api, ApiError, type Paciente, type SalvarPacienteRequest } from "@/lib/api";
import { ehMenorDeIdade } from "@/lib/idade";

export type CamposPaciente =
  | "nome"
  | "dataNascimento"
  | "tipoDocumento"
  | "numeroDocumento"
  | "email"
  | "telefone"
  | "responsavelNome"
  | "responsavelTipoDocumento"
  | "responsavelNumeroDocumento"
  | "responsavelParentesco";

export type EstadoSalvar = {
  erro?: string;
  campos?: Partial<Record<CamposPaciente, string>>;
  /** Valores devolvidos para repovoar o formulário após erro. */
  valores?: Partial<Record<CamposPaciente, string>>;
};

const tipoDocumento = z.enum(["Cpf", "Passaporte"], { message: "Selecione o tipo de documento." });
const documento = z.string().trim().min(1, "Informe o número do documento.");

// Validações finas (dígitos do CPF, maioridade, unicidade) ficam no domínio — aqui só o mínimo para UX.
const esquema = z
  .object({
    nome: z.string().trim().min(2, "Informe o nome completo."),
    dataNascimento: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, "Informe a data de nascimento."),
    tipoDocumento,
    numeroDocumento: documento,
    email: z.string().trim().toLowerCase().email("Informe um e-mail válido."),
    telefone: z
      .string()
      .trim()
      .refine((t) => t.replace(/\D/g, "").length >= 10, "Informe DDD e número."),
    temResponsavel: z.boolean(),
    responsavelNome: z.string().trim(),
    responsavelTipoDocumento: z.string().trim(),
    responsavelNumeroDocumento: z.string().trim(),
    responsavelParentesco: z.string().trim(),
  })
  .superRefine((d, ctx) => {
    if (!d.temResponsavel) return;
    if (d.responsavelNome.length < 2)
      ctx.addIssue({ code: "custom", path: ["responsavelNome"], message: "Informe o nome do responsável." });
    if (!["Cpf", "Passaporte"].includes(d.responsavelTipoDocumento))
      ctx.addIssue({ code: "custom", path: ["responsavelTipoDocumento"], message: "Selecione o tipo." });
    if (!d.responsavelNumeroDocumento)
      ctx.addIssue({ code: "custom", path: ["responsavelNumeroDocumento"], message: "Informe o documento." });
  });

function lerFormulario(dados: FormData) {
  const v = (k: string) => String(dados.get(k) ?? "");
  const valores = {
    nome: v("nome"),
    dataNascimento: v("dataNascimento"),
    tipoDocumento: v("tipoDocumento"),
    numeroDocumento: v("numeroDocumento"),
    email: v("email"),
    telefone: v("telefone"),
    responsavelNome: v("responsavelNome"),
    responsavelTipoDocumento: v("responsavelTipoDocumento"),
    responsavelNumeroDocumento: v("responsavelNumeroDocumento"),
    responsavelParentesco: v("responsavelParentesco"),
  };
  return { valores, temResponsavel: ehMenorDeIdade(valores.dataNascimento) };
}

function montarRequisicao(dados: FormData): { ok: true; req: SalvarPacienteRequest } | { ok: false; estado: EstadoSalvar } {
  const { valores, temResponsavel } = lerFormulario(dados);
  const parse = esquema.safeParse({ ...valores, temResponsavel });
  if (!parse.success) {
    const campos: EstadoSalvar["campos"] = {};
    for (const issue of parse.error.issues) {
      const c = issue.path[0] as CamposPaciente;
      campos[c] ??= issue.message;
    }
    return { ok: false, estado: { campos, valores } };
  }
  const d = parse.data;
  return {
    ok: true,
    req: {
      nome: d.nome,
      dataNascimento: d.dataNascimento,
      tipoDocumento: d.tipoDocumento,
      numeroDocumento: d.numeroDocumento,
      email: d.email,
      telefone: d.telefone,
      responsavelLegal: temResponsavel
        ? {
            nome: d.responsavelNome,
            tipoDocumento: d.responsavelTipoDocumento as "Cpf" | "Passaporte",
            numeroDocumento: d.responsavelNumeroDocumento,
            parentesco: d.responsavelParentesco || null,
          }
        : null,
    },
  };
}

export async function criarPaciente(_anterior: EstadoSalvar, dados: FormData): Promise<EstadoSalvar> {
  const r = montarRequisicao(dados);
  if (!r.ok) return r.estado;

  let criado: Paciente;
  try {
    criado = await api<Paciente>("/api/pacientes", { method: "POST", body: r.req });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message, valores: lerFormulario(dados).valores };
    throw e;
  }
  revalidatePath("/pacientes");
  redirect(`/pacientes/${criado.id}?criado=1`);
}

export async function atualizarPaciente(id: string, _anterior: EstadoSalvar, dados: FormData): Promise<EstadoSalvar> {
  const r = montarRequisicao(dados);
  if (!r.ok) return r.estado;

  try {
    await api<Paciente>(`/api/pacientes/${id}`, { method: "PUT", body: r.req });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message, valores: lerFormulario(dados).valores };
    throw e;
  }
  revalidatePath("/pacientes");
  revalidatePath(`/pacientes/${id}`);
  redirect(`/pacientes/${id}?salvo=1`);
}

export async function excluirPaciente(id: string, motivo: string) {
  const sessao = await auth();
  const usuarioId = sessao?.user?.id;
  if (!usuarioId) return { ok: false as const, erro: "Sessão expirada. Entre novamente." };
  if (!motivo.trim()) return { ok: false as const, erro: "Informe o motivo da exclusão." };

  try {
    await api(`/api/pacientes/${id}/excluir`, { method: "POST", body: { usuarioId, motivo: motivo.trim() } });
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
  revalidatePath("/pacientes");
  revalidatePath(`/pacientes/${id}`);
  return { ok: true as const };
}

export async function restaurarPaciente(id: string) {
  try {
    await api(`/api/pacientes/${id}/restaurar`, { method: "POST" });
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
  revalidatePath("/pacientes");
  revalidatePath(`/pacientes/${id}`);
  return { ok: true as const };
}
