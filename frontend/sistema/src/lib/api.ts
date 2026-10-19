import "server-only";
import { redirect } from "next/navigation";

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

/**
 * Chamada em nome do usuário logado: repassa a identidade da sessão no header X-Usuario-Id, que a API usa
 * para autoria. Se a API recusar o usuário (desativado, por exemplo), a sessão é encerrada.
 */
export async function api<T = void>(caminho: string, opcoes: Opcoes = {}): Promise<T> {
  // Import dinâmico: auth.ts depende deste módulo (apiPublica) para validar credenciais.
  const { auth } = await import("@/auth");
  const sessao = await auth();
  if (!sessao?.user?.id) redirect("/login");

  try {
    return await requisitar<T>(caminho, opcoes, { "X-Usuario-Id": sessao.user.id });
  } catch (e) {
    if (e instanceof ApiError && e.status === 401) redirect("/sair");
    throw e;
  }
}

/** Chamada sem usuário — apenas para os endpoints de /api/auth (login e definição de senha). */
export function apiPublica<T = void>(caminho: string, opcoes: Opcoes = {}): Promise<T> {
  return requisitar<T>(caminho, opcoes);
}

/**
 * Chamada em nome do usuário logado que devolve a Response crua — para upload e download de arquivos, onde o
 * corpo não é JSON e deve trafegar em stream. Usada por route handlers, que tratam 401 por conta própria.
 */
export async function apiArquivo(caminho: string, init: RequestInit & { duplex?: "half" } = {}): Promise<Response> {
  if (!BASE || !KEY) throw new Error("CLIGEN_API_URL / CLIGEN_API_KEY não configuradas.");
  const { auth } = await import("@/auth");
  const sessao = await auth();
  if (!sessao?.user?.id) throw new ApiError(401, "Sessão expirada. Entre novamente.");

  const res = await fetch(`${BASE}${caminho}`, {
    ...init,
    headers: { ...(init.headers as Record<string, string>), "X-Api-Key": KEY, "X-Usuario-Id": sessao.user.id },
    cache: "no-store",
  });
  if (!res.ok) throw await erroDaResposta(res);
  return res;
}

async function requisitar<T>(caminho: string, opcoes: Opcoes, headersExtras: Record<string, string> = {}): Promise<T> {
  if (!BASE || !KEY) throw new Error("CLIGEN_API_URL / CLIGEN_API_KEY não configuradas.");

  const { body, headers, ...resto } = opcoes;
  const res = await fetch(`${BASE}${caminho}`, {
    ...resto,
    headers: {
      "X-Api-Key": KEY,
      ...headersExtras,
      ...(body !== undefined ? { "Content-Type": "application/json" } : {}),
      ...headers,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
    cache: "no-store",
  });

  if (!res.ok) throw await erroDaResposta(res);

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

async function erroDaResposta(res: Response) {
  let detalhe = res.statusText;
  try {
    const problema = (await res.json()) as { detail?: string; title?: string };
    detalhe = problema.detail ?? problema.title ?? detalhe;
  } catch {
    /* corpo vazio */
  }
  return new ApiError(res.status, detalhe);
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

export interface Pagina<T> {
  itens: T[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

/** Origem, destino ou convênio — mesma forma para os três. */
export interface Cadastro {
  id: string;
  nome: string;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

export type TipoCadastro = "origens" | "destinos" | "convenios";

export interface ExameCatalogo {
  id: string;
  nome: string;
  /** Prazo de entrega em dias corridos a partir do acolhimento; já inclui a revisão. */
  prazoDias: number;
  precoReferencia: number;
  /** Piso: o valor a pagar de um exame não fica abaixo dele. */
  precoLimite: number;
  numeroGenes: number | null;
  genes: string | null;
  destinoId: string | null;
  destinoNome: string | null;
  informacoesTecnicas: string | null;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

export type TipoDocumento = "Cpf" | "Passaporte";

export interface ResponsavelLegal {
  nome: string;
  tipoDocumento: TipoDocumento;
  numeroDocumento: string;
  parentesco: string | null;
}

export interface Paciente {
  id: string;
  nome: string;
  /** yyyy-MM-dd */
  dataNascimento: string;
  menorDeIdade: boolean;
  tipoDocumento: TipoDocumento;
  /** Normalizado: CPF só dígitos, passaporte em maiúsculas. */
  numeroDocumento: string;
  email: string;
  /** Formato internacional, ex. +5531999998888. */
  telefone: string;
  cidade: string | null;
  responsavelLegal: ResponsavelLegal | null;
  criadoEm: string;
  atualizadoEm: string | null;
}

export type EstadoExame =
  | "AguardandoAmostra"
  | "AmostraAcolhida"
  | "LaudoParceiroPronto"
  | "LaudoCligenParaRevisao"
  | "LaudoRevisado"
  | "Disponibilizado";

export type TipoMedico = "Interno" | "Externo";
export type TipoAnexo = "PedidoMedico" | "TermoConsentimento" | "Outro";
export type CampoDataExame = "Acolhimento" | "Prevista" | "Disponibilizado";

export interface Anexo {
  id: string;
  tipo: TipoAnexo;
  nomeOriginal: string;
  tipoConteudo: string;
  tamanhoBytes: number;
  hashSha256: string;
  enviadoEm: string;
}

export interface Comentario {
  id: string;
  texto: string;
  autorId: string;
  autorNome: string;
  criadoEm: string;
}

export interface ExameResumo {
  id: string;
  /** EX-000123 */
  codigo: string;
  pacienteId: string;
  pacienteNome: string;
  pacienteTipoDocumento: TipoDocumento;
  pacienteNumeroDocumento: string;
  exameCatalogoId: string;
  exameNome: string;
  origemId: string;
  origemNome: string;
  destinoId: string | null;
  destinoNome: string | null;
  dataEntrada: string;
  estado: EstadoExame;
  dataAcolhimento: string | null;
  /** yyyy-MM-dd. Nula até o acolhimento da amostra. */
  dataLiberacaoPrevista: string | null;
  dataLiberacaoEfetiva: string | null;
  /** Preenchidos só na listagem de excluídos (?excluidos=true). */
  excluidoEm: string | null;
  motivoExclusao: string | null;
}

export type TipoEtapaLaudo = "LaudoParceiroPronto" | "LaudoCligenParaRevisao" | "LaudoRevisado";

export interface EtapaLaudo {
  id: string;
  tipo: TipoEtapaLaudo;
  /** Do primeiro upload; não muda na substituição. */
  data: string;
  nomeOriginal: string;
  tamanhoBytes: number;
  hashSha256: string;
  substituicoes: number;
  substituidoEm: string | null;
}

export interface Amostra {
  id: string;
  dataAcolhimento: string;
  prazoDias: number;
  dataLiberacaoPrevista: string;
  recoleta: boolean;
  registradoEm: string;
  rejeitadaEm: string | null;
  motivoRejeicao: string | null;
}

export interface Exame {
  id: string;
  codigo: string;
  pacienteId: string;
  pacienteNome: string;
  pacienteTipoDocumento: TipoDocumento;
  pacienteNumeroDocumento: string;
  exameCatalogoId: string;
  exameNome: string;
  prazoDias: number;
  precoLimite: number;
  origemId: string;
  origemNome: string;
  destinoId: string | null;
  destinoNome: string | null;
  tipoMedico: TipoMedico;
  /** Já resolvido: nome do médico interno ou o digitado para o externo. */
  nomeMedico: string;
  crmMedico: string | null;
  ufCrm: string | null;
  preco: number;
  /** Em reais (C5). */
  desconto: number;
  motivoDesconto: string | null;
  /** preco − desconto. */
  valorAPagar: number;
  dataEntrada: string;
  estado: EstadoExame;
  dataAcolhimento: string | null;
  /** Fixa desde o acolhimento (P14); nula enquanto aguarda amostra. */
  dataLiberacaoPrevista: string | null;
  /** Gravada ao disponibilizar ao paciente. */
  dataLiberacaoEfetiva: string | null;
  /** Histórico, da mais recente para a mais antiga; inclui rejeitadas. */
  amostras: Amostra[];
  /** Na ordem do fluxo (3, 4, 5); só as já registradas. */
  etapas: EtapaLaudo[];
  anexos: Anexo[];
  /** Mais recentes primeiro. */
  comentarios: Comentario[];
  criadoEm: string;
  atualizadoEm: string | null;
}

export type MetodoPagamento = "Pix" | "CartaoCredito" | "CartaoDebito" | "Dinheiro" | "Boleto" | "Transferencia" | "Convenio";
export type StatusPagamento = "AReceber" | "Recebido" | "Glosado";

export interface Pagamento {
  id: string;
  exameId: string;
  metodo: MetodoPagamento;
  valor: number;
  status: StatusPagamento;
  dataRecebimento: string | null;
  convenioId: string | null;
  convenioNome: string | null;
  numeroGuia: string | null;
  parcelas: number | null;
  valorParcela: number | null;
  observacao: string | null;
  motivoGlosa: string | null;
  criadoEm: string;
}

export interface FinanceiroExame {
  exameId: string;
  preco: number;
  desconto: number;
  motivoDesconto: string | null;
  precoLimite: number;
  valorAPagar: number;
  recebido: number;
  aReceber: number;
  /** valorAPagar − recebido − aReceber. */
  saldo: number;
  pagamentos: Pagamento[];
}

export interface LinhaFinanceira {
  pagamentoId: string;
  exameId: string;
  exameCodigo: string;
  exameNome: string;
  pacienteNome: string;
  pacienteTipoDocumento: TipoDocumento;
  pacienteNumeroDocumento: string;
  origemId: string;
  origemNome: string;
  metodo: MetodoPagamento;
  convenioId: string | null;
  convenioNome: string | null;
  numeroGuia: string | null;
  valor: number;
  dataRecebimento: string | null;
  dataEntradaExame: string;
}

export interface TotalPorChave {
  chave: string;
  total: number;
  quantidade: number;
}

export interface RelatorioFinanceiro {
  de: string;
  ate: string;
  convenioId: string | null;
  origemId: string | null;
  totalRecebido: number;
  recebidoPorMetodo: TotalPorChave[];
  recebidoPorOrigem: TotalPorChave[];
  recebidos: LinhaFinanceira[];
  totalAReceber: number;
  aReceberPorConvenio: TotalPorChave[];
  aReceber: LinhaFinanceira[];
}
