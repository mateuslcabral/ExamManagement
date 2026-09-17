import type { EstadoExame, OrigemExame, TipoDocumento, TipoEtapaLaudo } from "./api";

/** Formatação para exibição. Os dados trafegam normalizados pela API (CPF só dígitos, telefone +DDI). */

export function formatarDocumento(tipo: TipoDocumento, numero: string) {
  if (tipo === "Cpf" && /^\d{11}$/.test(numero)) {
    return numero.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4");
  }
  return numero;
}

export function rotuloDocumento(tipo: TipoDocumento) {
  return tipo === "Cpf" ? "CPF" : "Passaporte";
}

export function formatarTelefone(telefone: string) {
  const br = /^\+55(\d{2})(\d{4,5})(\d{4})$/.exec(telefone);
  return br ? `(${br[1]}) ${br[2]}-${br[3]}` : telefone;
}

/** "2015-01-31" → "31/01/2015", sem passar por Date (evita deslocamento de fuso). */
export function formatarData(iso: string) {
  const [ano, mes, dia] = iso.split("-");
  return `${dia}/${mes}/${ano}`;
}

/** Idade em anos completos entre duas datas "yyyy-MM-dd". */
export function idade(nascimento: string, hoje: string) {
  const [an, mn, dn] = nascimento.split("-").map(Number);
  const [ah, mh, dh] = hoje.split("-").map(Number);
  return ah - an - (mh < mn || (mh === mn && dh < dn) ? 1 : 0);
}

/** Soma dias corridos a uma data "yyyy-MM-dd", sem passar por fuso local. */
export function somarDias(iso: string, dias: number) {
  const d = new Date(`${iso}T00:00:00Z`);
  d.setUTCDate(d.getUTCDate() + dias);
  return d.toISOString().slice(0, 10);
}

/** Data civil de Brasília em "yyyy-MM-dd" — a mesma referência que a API usa para menoridade e prazos. */
export function hojeEmBrasilia() {
  return new Intl.DateTimeFormat("en-CA", { timeZone: "America/Sao_Paulo" }).format(new Date());
}

const moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

export function formatarMoeda(valor: number) {
  return moeda.format(valor);
}

/** Valor para preencher campo de edição: 1250.9 → "1250,90". */
export function valorParaCampo(valor: number) {
  return valor.toFixed(2).replace(".", ",");
}

/**
 * Lê valor monetário digitado. Aceita "1.250,90", "1250,90", "1250.90" e "R$ 1.250,90".
 * Devolve null se não for um valor com até 2 casas decimais.
 */
export function lerValorMonetario(texto: string) {
  const limpo = texto.replace(/R\$|\s/g, "");
  const normalizado = limpo.includes(",") ? limpo.replace(/\./g, "").replace(",", ".") : limpo;
  return /^\d+(\.\d{1,2})?$/.test(normalizado) ? Number(normalizado) : null;
}

export function formatarTamanho(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1).replace(".", ",")} MB`;
}

export const ROTULOS_ESTADO: Record<EstadoExame, string> = {
  AguardandoAmostra: "Aguardando amostra",
  AmostraAcolhida: "Amostra acolhida",
  LaudoParceiroPronto: "Laudo parceiro pronto",
  LaudoCligenParaRevisao: "Laudo Cligen para revisão",
  LaudoRevisado: "Laudo revisado",
  Disponibilizado: "Disponibilizado",
};

export const ROTULOS_ORIGEM: Record<OrigemExame, string> = {
  Cligen: "Cligen",
  ClinicaParceira: "Clínica parceira",
  SiteCligen: "Site Cligen",
  Plataforma: "Plataforma",
};

export const ETAPAS_LAUDO: { tipo: TipoEtapaLaudo; numero: number; rotulo: string; estadoAnterior: EstadoExame }[] = [
  { tipo: "LaudoParceiroPronto", numero: 3, rotulo: "Laudo parceiro pronto", estadoAnterior: "AmostraAcolhida" },
  { tipo: "LaudoCligenParaRevisao", numero: 4, rotulo: "Laudo Cligen para revisão", estadoAnterior: "LaudoParceiroPronto" },
  { tipo: "LaudoRevisado", numero: 5, rotulo: "Laudo revisado", estadoAnterior: "LaudoCligenParaRevisao" },
];

/** Data e hora de um instante ISO (UTC) no fuso de Brasília. */
export function formatarDataHora(iso: string) {
  return new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short", timeZone: "America/Sao_Paulo" }).format(new Date(iso));
}
