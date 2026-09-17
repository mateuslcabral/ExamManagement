"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { z } from "zod";
import { api, ApiError, type Exame, type Pagina, type Paciente } from "@/lib/api";
import { lerValorMonetario } from "@/lib/formatos";

export type CampoExame = "pacienteId" | "exameCatalogoId" | "origem" | "nomeMedicoExterno" | "preco" | "destino";
export type EstadoSalvarExame = { erro?: string; campos?: Partial<Record<CampoExame, string>> };

const esquema = z
  .object({
    pacienteId: z.uuid("Selecione o paciente."),
    exameCatalogoId: z.uuid("Selecione o exame."),
    origem: z.enum(["Cligen", "ClinicaParceira", "SiteCligen", "Plataforma"], "Selecione a origem."),
    destino: z.string().trim().max(200, "Use no máximo 200 caracteres.").default(""),
    tipoMedico: z.enum(["Interno", "Externo"]),
    nomeMedicoExterno: z.string().trim().default(""),
    preco: z.string().trim().min(1, "Informe o preço.").transform(lerValorMonetario).pipe(z.number({ error: "Informe um valor como 1.250,90." })),
  })
  .superRefine((d, ctx) => {
    if (d.tipoMedico === "Externo" && !d.nomeMedicoExterno)
      ctx.addIssue({ code: "custom", path: ["nomeMedicoExterno"], message: "Informe o nome do médico." });
  });

export async function salvarExame(_anterior: EstadoSalvarExame, dados: FormData): Promise<EstadoSalvarExame> {
  const id = String(dados.get("id") ?? "");
  const parse = esquema.safeParse(Object.fromEntries(dados));
  if (!parse.success) {
    const campos: EstadoSalvarExame["campos"] = {};
    for (const issue of parse.error.issues) campos[issue.path[0] as CampoExame] ??= issue.message;
    return { campos };
  }

  const d = parse.data;
  const corpo = {
    pacienteId: d.pacienteId,
    exameCatalogoId: d.exameCatalogoId,
    origem: d.origem,
    destino: d.destino || null,
    tipoMedico: d.tipoMedico,
    nomeMedicoExterno: d.tipoMedico === "Externo" ? d.nomeMedicoExterno : null,
    preco: d.preco,
  };

  let salvo: Exame;
  try {
    salvo = id
      ? await api<Exame>(`/api/exames/${id}`, { method: "PUT", body: corpo })
      : await api<Exame>("/api/exames", { method: "POST", body: corpo });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }

  revalidatePath("/exames");
  // Após cadastrar, vai para o detalhe: o próximo passo natural é anexar os arquivos.
  redirect(`/exames/${salvo.id}?salvo=${id ? "editado" : "criado"}`);
}

export type PacienteEncontrado = Pick<Paciente, "id" | "nome" | "tipoDocumento" | "numeroDocumento" | "dataNascimento">;

export async function buscarPacientes(termo: string): Promise<PacienteEncontrado[]> {
  const busca = termo.trim();
  if (busca.length < 2) return [];
  const r = await api<Pagina<Paciente>>(`/api/pacientes?busca=${encodeURIComponent(busca)}&tamanhoPagina=8`);
  return r.itens.map(({ id, nome, tipoDocumento, numeroDocumento, dataNascimento }) => ({
    id, nome, tipoDocumento, numeroDocumento, dataNascimento,
  }));
}

export type EstadoExcluirExame = { erro?: string };

export async function excluirExame(_anterior: EstadoExcluirExame, dados: FormData): Promise<EstadoExcluirExame> {
  const id = String(dados.get("id") ?? "");
  const motivo = String(dados.get("motivo") ?? "").trim();
  if (!motivo) return { erro: "Informe o motivo da exclusão." };

  try {
    await api(`/api/exames/${id}/excluir`, { method: "POST", body: { motivo } });
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }

  revalidatePath("/exames");
  redirect("/exames?excluido=1");
}

export type EstadoAmostra = { erro?: string; sucesso?: string };

function revalidarAmostra(exameId: string) {
  revalidatePath(`/exames/${exameId}`);
  revalidatePath("/exames");
  revalidatePath("/amostras");
}

export async function acolherAmostra(_anterior: EstadoAmostra, dados: FormData): Promise<EstadoAmostra> {
  const exameId = String(dados.get("exameId") ?? "");
  const parse = z.iso.date().safeParse(dados.get("dataAcolhimento"));
  if (!parse.success) return { erro: "Informe a data de acolhimento." };

  try {
    const exame = await api<Exame>(`/api/exames/${exameId}/amostra/acolher`, {
      method: "POST",
      body: { dataAcolhimento: parse.data },
    });
    revalidarAmostra(exameId);
    const [ano, mes, dia] = (exame.dataLiberacaoPrevista ?? "").split("-");
    return { sucesso: `Amostra acolhida. Liberação prevista para ${dia}/${mes}/${ano}.` };
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
}

export async function rejeitarAmostra(_anterior: EstadoAmostra, dados: FormData): Promise<EstadoAmostra> {
  const exameId = String(dados.get("exameId") ?? "");
  const motivo = String(dados.get("motivo") ?? "").trim();
  if (!motivo) return { erro: "Informe o motivo da rejeição." };

  try {
    await api(`/api/exames/${exameId}/amostra/rejeitar`, { method: "POST", body: { motivo } });
    revalidarAmostra(exameId);
    return { sucesso: "Amostra rejeitada. O exame voltou a aguardar amostra." };
  } catch (e) {
    if (e instanceof ApiError) return { erro: e.message };
    throw e;
  }
}

/** 5→6 (D10): grava a data efetiva e dispara e-mail e WhatsApp (em log até haver provedores). */
export async function disponibilizarLaudo(exameId: string) {
  try {
    await api(`/api/exames/${exameId}/laudo/disponibilizar`, { method: "POST" });
    revalidarAmostra(exameId);
    revalidatePath("/laudos");
    return { ok: true as const };
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
}

export async function restaurarExame(exameId: string) {
  try {
    await api(`/api/exames/${exameId}/restaurar`, { method: "POST" });
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
  revalidatePath("/exames");
  redirect(`/exames/${exameId}?salvo=restaurado`);
}

export async function removerAnexo(exameId: string, anexoId: string) {
  try {
    await api(`/api/exames/${exameId}/anexos/${anexoId}/remover`, { method: "POST" });
    revalidatePath(`/exames/${exameId}`);
    return { ok: true as const };
  } catch (e) {
    if (e instanceof ApiError) return { ok: false as const, erro: e.message };
    throw e;
  }
}
