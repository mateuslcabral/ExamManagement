import Link from "next/link";
import { notFound } from "next/navigation";
import { api, ApiError, type Cadastro, type Exame, type ExameCatalogo, type FinanceiroExame as DadosFinanceiros } from "@/lib/api";
import { formatarData, formatarDocumento, formatarMoeda, hojeEmBrasilia } from "@/lib/formatos";
import { EtiquetaEstado } from "../etiqueta-estado";
import { FormularioExame } from "../formulario-exame";
import { AmostraExame } from "./amostra-exame";
import { AnexosExame } from "./anexos-exame";
import { ComentariosExame } from "./comentarios-exame";
import { PainelLaudo } from "./painel-laudo";
import { ExclusaoExame } from "./exclusao-exame";
import { FinanceiroExame } from "./financeiro-exame";

export const metadata = { title: "Exame — Cligen" };

const mensagensSalvo: Record<string, string> = {
  criado: "Exame cadastrado. Anexe o pedido médico e o termo de consentimento e registre o acolhimento quando a amostra chegar.",
  editado: "Alterações salvas.",
  restaurado: "Exame restaurado. Ele volta às listas e ao fluxo normal.",
};

export default async function PaginaExame({
  params,
  searchParams,
}: {
  params: Promise<{ id: string }>;
  searchParams: Promise<{ salvo?: string }>;
}) {
  const [{ id }, { salvo }] = await Promise.all([params, searchParams]);

  let exame: Exame;
  try {
    exame = await api<Exame>(`/api/exames/${encodeURIComponent(id)}`);
  } catch (e) {
    if (e instanceof ApiError && (e.status === 404 || e.status === 400)) notFound();
    throw e;
  }

  // Cadastros completos (inclusive inativos): o formulário mostra o valor atual mesmo se ele foi desativado depois.
  const [ativos, financeiro, origens, destinos, convenios] = await Promise.all([
    api<ExameCatalogo[]>("/api/catalogo-exames?apenasAtivos=true"),
    api<DadosFinanceiros>(`/api/exames/${exame.id}/financeiro`),
    api<Cadastro[]>("/api/cadastros/origens"),
    api<Cadastro[]>("/api/cadastros/destinos"),
    api<Cadastro[]>("/api/cadastros/convenios"),
  ]);
  // Se o exame do catálogo foi desativado depois, ele continua aparecendo como opção para este exame.
  const catalogo = ativos.some((c) => c.id === exame.exameCatalogoId)
    ? ativos
    : [await api<ExameCatalogo>(`/api/catalogo-exames/${exame.exameCatalogoId}`), ...ativos];

  const crm = exame.crmMedico ? ` (CRM ${exame.crmMedico}${exame.ufCrm ? `-${exame.ufCrm}` : ""})` : "";

  return (
    <div className="mx-auto max-w-3xl">
      <Link href="/fluxos" className="text-sm text-teal hover:underline">
        ← Fluxos
      </Link>
      <div className="mt-2 flex flex-wrap items-center gap-3">
        <h1 className="text-2xl font-semibold text-primaria">{exame.exameNome}</h1>
        <span className="rounded-pill bg-fundo px-2.5 py-1 font-mono text-xs font-semibold text-texto-suave" title="Código interno do exame">
          {exame.codigo}
        </span>
        <EtiquetaEstado estado={exame.estado} />
      </div>
      <p className="mt-1 text-sm text-texto-suave">
        <Link href={`/fluxos?pacienteId=${exame.pacienteId}`} className="text-teal hover:underline">
          {exame.pacienteNome}
        </Link>{" "}
        · {formatarDocumento(exame.pacienteTipoDocumento, exame.pacienteNumeroDocumento)} · entrada em {formatarData(exame.dataEntrada)} ·{" "}
        {formatarMoeda(exame.preco)} · {exame.nomeMedico}
        {crm}
      </p>
      <p className="text-xs text-texto-suave">
        Origem: {exame.origemNome}
        {exame.destinoNome && ` · Destino: ${exame.destinoNome}`}
      </p>

      {salvo && mensagensSalvo[salvo] && (
        <p role="status" className="mt-4 rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
          {mensagensSalvo[salvo]}
        </p>
      )}

      <div className="mt-6 space-y-6">
        <AmostraExame exame={exame} hoje={hojeEmBrasilia()} />
        <PainelLaudo exame={exame} />
        <AnexosExame exameId={exame.id} anexos={exame.anexos} />
        <ComentariosExame exameId={exame.id} comentarios={exame.comentarios} />
        <FormularioExame exame={exame} catalogo={catalogo} origens={origens} destinos={destinos} />
        <FinanceiroExame financeiro={financeiro} convenios={convenios.filter((c) => c.ativo)} hoje={hojeEmBrasilia()} />
        <ExclusaoExame exameId={exame.id} />
      </div>
    </div>
  );
}
