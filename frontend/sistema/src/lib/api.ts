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
