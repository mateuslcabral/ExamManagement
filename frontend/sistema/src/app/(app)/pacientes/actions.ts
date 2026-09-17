"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { z } from "zod";
import { api, ApiError, type Paciente } from "@/lib/api";

export type CampoPaciente =
  | "nome"
  | "dataNascimento"
  | "numeroDocumento"
  | "email"
  | "telefone"
  | "responsavelNome"
  | "responsavelNumeroDocumento";

export type EstadoSalvarPaciente = { erro?: string; campos?: Partial<Record<CampoPaciente, string>> };

const tipoDocumento = z.enum(["Cpf", "Passaporte"]);
const obrigatorio = (mensagem: string) => z.string().trim().min(1, mensagem);

// Formato de CPF, telefone e regra de menoridade são validados pelo domínio na API; aqui só o preenchimento.
const esquema = z
  .object({
    nome: obrigatorio("Informe o nome completo.").max(200, "Use no máximo 200 caracteres."),
    dataNascimento: z.iso.date("Informe a data de nascimento."),
    tipoDocumento,
    numeroDocumento: obrigatorio("Informe o número do documento."),
    email: z.string().trim().email("Informe um e-mail válido."),
    telefone: obrigatorio("Informe o telefone com DDD."),
    temResponsavel: z.enum(["sim", "nao"]),
    // Ausentes quando a seção do responsável está oculta.
    responsavelNome: z.string().trim().default(""),
    responsavelTipoDocumento: tipoDocumento.default("Cpf"),
    responsavelNumeroDocumento: z.string().trim().default(""),
    responsavelParentesco: z.string().trim().default(""),
  })
  .superRefine((d, ctx) => {
    if (d.temResponsavel !== "sim") return;
    if (!d.responsavelNome)
      ctx.addIssue({ code: "custom", path: ["responsavelNome"], message: "Informe o nome do responsável." });
    if (!d.responsavelNumeroDocumento)
      ctx.addIssue({ code: "custom", path: ["responsavelNumeroDocumento"], message: "Informe o documento do responsável." });
  });

export async function salvarPaciente(_anterior: EstadoSalvarPaciente, dados: FormData): Promise<EstadoSalvarPaciente> {
  const id = String(dados.get("id") ?? "");
  const parse = esquema.safeParse(Object.fromEntries(dados));
  if (!parse.success) {
    const campos: EstadoSalvarPaciente["campos"] = {};
    for (const issue of parse.error.issues) campos[issue.path[0] as CampoPaciente] ??= issue.message;
    return { campos };
  }

  const d = parse.data;
  const corpo = {
    nome: d.nome,
    dataNascimento: d.dataNascimento,
    tipoDocumento: d.tipoDocumento,
    numeroDocumento: d.numeroDocumento,
    email: d.email,
    telefone: d.telefone,
    responsavelLegal:
      d.temResponsavel === "sim"
        ? {
            nome: d.responsavelNome,
            tipoDocumento: d.responsavelTipoDocumento,
            numeroDocumento: d.responsavelNumeroDocumento,
            parentesco: d.responsavelParentesco || null,
          }
        : null,
  };

  let salvo: Paciente;
  try {
    salvo = id
      ? await api<Paciente>(`/api/pacientes/${id}`, { method: "PUT", body: corpo })
      : await api<Paciente>("/api/pacientes", { method: "POST", body: corpo });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }

  revalidatePath("/pacientes");
  // Volta para a lista filtrada pelo paciente salvo, com a confirmação.
  redirect(`/pacientes?busca=${encodeURIComponent(salvo.numeroDocumento)}&salvo=${id ? "editado" : "criado"}`);
}
