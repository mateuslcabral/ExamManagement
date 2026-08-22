import "server-only";

/**
 * Cliente da API Cligen. Roda APENAS no servidor (BFF): a chave de serviço nunca chega ao navegador.
 * Ver docs/02-arquitetura/autenticacao.md.
 */

const BASE = process.env.CLIGEN_API_URL;
const KEY = process.env.CLIGEN_API_KEY;

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

type Opcoes = Omit<RequestInit, "body"> & { body?: unknown };

export async function api<T = void>(caminho: string, opcoes: Opcoes = {}): Promise<T> {
  if (!BASE || !KEY) throw new Error("CLIGEN_API_URL / CLIGEN_API_KEY não configuradas.");

  const { body, headers, ...resto } = opcoes;
  const res = await fetch(`${BASE}${caminho}`, {
    ...resto,
    headers: {
      "X-Api-Key": KEY,
      ...(body !== undefined ? { "Content-Type": "application/json" } : {}),
      ...headers,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
    cache: "no-store",
  });

  if (!res.ok) {
    let detalhe = res.statusText;
    try {
      const problema = (await res.json()) as { detail?: string; title?: string };
      detalhe = problema.detail ?? problema.title ?? detalhe;
    } catch {
      /* corpo vazio */
    }
    throw new ApiError(res.status, detalhe);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

// ---- Tipos espelhando os DTOs da Aplicação ----

export type TipoLogin = "Google" | "Local";

export interface Usuario {
  id: string;
  nome: string;
  email: string;
  tipoLogin: TipoLogin;
  ativo: boolean;
  senhaDefinida: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

export interface Identidade {
  id: string;
  nome: string;
  email: string;
  tipoLogin: TipoLogin;
}

// ---- Pacientes ----

export type TipoDocumento = "Cpf" | "Passaporte";

export interface ResponsavelLegal {
  nome: string;
  tipoDocumento: TipoDocumento;
  numeroDocumento: string;
  numeroDocumentoFormatado: string;
  parentesco: string | null;
}

export interface PacienteResumo {
  id: string;
  nome: string;
  dataNascimento: string;
  idade: number;
  menorDeIdade: boolean;
  tipoDocumento: TipoDocumento;
  numeroDocumentoFormatado: string;
  email: string;
  telefone: string;
  excluido: boolean;
  criadoEm: string;
}

export interface Paciente extends PacienteResumo {
  numeroDocumento: string;
  responsavelLegal: ResponsavelLegal | null;
  excluidoEm: string | null;
  motivoExclusao: string | null;
  atualizadoEm: string | null;
}

export interface SalvarPacienteRequest {
  nome: string;
  dataNascimento: string;
  tipoDocumento: TipoDocumento;
  numeroDocumento: string;
  email: string;
  telefone: string;
  responsavelLegal: {
    nome: string;
    tipoDocumento: TipoDocumento;
    numeroDocumento: string;
    parentesco?: string | null;
  } | null;
}

/** Envio multipart (upload de anexo). Mesmas regras do `api`, sem JSON. */
export async function apiFormulario<T = void>(caminho: string, form: FormData, method = "POST"): Promise<T> {
  if (!BASE || !KEY) throw new Error("CLIGEN_API_URL / CLIGEN_API_KEY não configuradas.");
  const res = await fetch(`${BASE}${caminho}`, { method, headers: { "X-Api-Key": KEY }, body: form, cache: "no-store" });
  if (!res.ok) {
    let detalhe = res.statusText;
    try {
      const problema = (await res.json()) as { detail?: string; title?: string };
      detalhe = problema.detail ?? problema.title ?? detalhe;
    } catch {
      /* corpo vazio */
    }
    throw new ApiError(res.status, detalhe);
  }
  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

/** Resposta bruta (download de anexo): o chamador repassa o stream ao navegador. */
export async function apiBruta(caminho: string): Promise<Response> {
  if (!BASE || !KEY) throw new Error("CLIGEN_API_URL / CLIGEN_API_KEY não configuradas.");
  return fetch(`${BASE}${caminho}`, { headers: { "X-Api-Key": KEY }, cache: "no-store" });
}

// ---- Catálogo, exames e anexos: tipos/constantes em lib/exames.ts (também usados por client components) ----
export type { Amostra, Anexo, EstadoExame, EtapaLaudo, Exame, ExameCatalogo, ExameResumo, OrigemExame, Parametro, TipoEtapaLaudo, TipoMedicoSolicitante } from "./exames";
