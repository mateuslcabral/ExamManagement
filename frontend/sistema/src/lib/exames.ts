/** Tipos e constantes do módulo de exames. Sem "server-only": importável por client components. */

// ---- Catálogo de exames e parâmetros ----

export interface ExameCatalogo {
  id: string;
  nome: string;
  prazoExecucaoDias: number;
  diasRevisao: number;
  prazoTotalDias: number;
  precoReferencia: number;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

export interface Parametro {
  chave: string;
  valor: string;
  descricao: string;
  atualizadoEm: string | null;
}

// ---- Exames ----

export type OrigemExame = "Cligen" | "ClinicaParceira" | "SiteCligen" | "Plataforma";
export type TipoMedicoSolicitante = "Interno" | "Externo";
export type EstadoExame =
  | "AguardandoAmostra"
  | "AmostraAcolhida"
  | "LaudoParceiroPronto"
  | "LaudoCligenParaRevisao"
  | "LaudoRevisado"
  | "DisponibilizadoAoPaciente";

export const ORIGENS: Record<OrigemExame, string> = {
  Cligen: "Cligen",
  ClinicaParceira: "Clínica Parceira",
  SiteCligen: "Site Cligen",
  Plataforma: "Plataforma",
};

export const ESTADOS: Record<EstadoExame, string> = {
  AguardandoAmostra: "Aguardando amostra",
  AmostraAcolhida: "Amostra acolhida",
  LaudoParceiroPronto: "Laudo parceiro pronto",
  LaudoCligenParaRevisao: "Laudo Cligen p/ revisão",
  LaudoRevisado: "Laudo revisado",
  DisponibilizadoAoPaciente: "Disponibilizado ao paciente",
};

export const NOME_MEDICO_INTERNO = "Dr. Arsonval Lamounier Junior";

export interface Anexo {
  id: string;
  nomeOriginal: string;
  tipoConteudo: string;
  tamanhoBytes: number;
  enviadoEm: string;
}

export interface ExameResumo {
  id: string;
  pacienteId: string;
  pacienteNome: string;
  exameNome: string;
  origem: OrigemExame;
  tipoMedico: TipoMedicoSolicitante;
  nomeMedico: string;
  dataEntrada: string;
  preco: number;
  estado: EstadoExame;
  dataLiberacaoPrevista: string | null;
  quantidadeAnexos: number;
  excluido: boolean;
}

export interface Exame extends Omit<ExameResumo, "quantidadeAnexos"> {
  exameCatalogoId: string;
  prazoExecucaoDias: number;
  destino: string | null;
  dataLiberacaoEfetiva: string | null;
  anexos: Anexo[];
  amostraAtiva: Amostra | null;
  amostras: Amostra[];
  etapas: EtapaLaudo[];
  excluidoEm: string | null;
  motivoExclusao: string | null;
  criadoEm: string;
  atualizadoEm: string | null;
}

// ---- Amostra e laudo ----

export type SituacaoAmostra = "Acolhida" | "Rejeitada";
export type TipoEtapaLaudo = "LaudoParceiroPronto" | "LaudoCligenParaRevisao" | "LaudoRevisado";

export const ETAPAS: { tipo: TipoEtapaLaudo; rotulo: string; estadoAnterior: EstadoExame }[] = [
  { tipo: "LaudoParceiroPronto", rotulo: "Laudo parceiro pronto", estadoAnterior: "AmostraAcolhida" },
  { tipo: "LaudoCligenParaRevisao", rotulo: "Laudo Cligen para revisão", estadoAnterior: "LaudoParceiroPronto" },
  { tipo: "LaudoRevisado", rotulo: "Laudo revisado", estadoAnterior: "LaudoCligenParaRevisao" },
];

export interface Amostra {
  id: string;
  dataAcolhimento: string;
  situacao: SituacaoAmostra;
  motivoRejeicao: string | null;
  rejeitadaEm: string | null;
  criadoEm: string;
}

export interface EtapaLaudo {
  tipo: TipoEtapaLaudo;
  data: string;
  nomeOriginal: string;
  tamanhoBytes: number;
  arquivoSubstituidoEm: string | null;
  substituicoes: number;
}
