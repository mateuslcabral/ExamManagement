import Link from "next/link";
import { notFound } from "next/navigation";
import { api, ApiError, type Exame, type ExameCatalogo } from "@/lib/api";
import { formatarData, formatarDocumento, formatarMoeda, hojeEmBrasilia } from "@/lib/formatos";
import { EtiquetaEstado } from "../etiqueta-estado";
import { FormularioExame } from "../formulario-exame";
import { AmostraExame } from "./amostra-exame";
import { AnexosExame } from "./anexos-exame";
import { PainelLaudo } from "./painel-laudo";
import { ExclusaoExame } from "./exclusao-exame";

export const metadata = { title: "Exame — Cligen" };

const mensagensSalvo: Record<string, string> = {
  criado: "Exame cadastrado. Registre o acolhimento quando a amostra chegar e anexe os arquivos da solicitação, se houver.",
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

  const ativos = await api<ExameCatalogo[]>("/api/catalogo-exames?apenasAtivos=true");
  // Se o exame do catálogo foi desativado depois, ele continua aparecendo como opção para este exame.
  const catalogo = ativos.some((c) => c.id === exame.exameCatalogoId)
    ? ativos
    : [await api<ExameCatalogo>(`/api/catalogo-exames/${exame.exameCatalogoId}`), ...ativos];

  return (
    <div className="mx-auto max-w-3xl">
      <Link href="/exames" className="text-sm text-teal hover:underline">
        ← Exames
      </Link>
      <div className="mt-2 flex flex-wrap items-center gap-3">
        <h1 className="text-2xl font-semibold text-primaria">{exame.exameNome}</h1>
        <EtiquetaEstado estado={exame.estado} />
      </div>
      <p className="mt-1 text-sm text-texto-suave">
        <Link href={`/exames?pacienteId=${exame.pacienteId}`} className="text-teal hover:underline">
          {exame.pacienteNome}
        </Link>{" "}
        · {formatarDocumento(exame.pacienteTipoDocumento, exame.pacienteNumeroDocumento)} · entrada em{" "}
        {formatarData(exame.dataEntrada)} · {formatarMoeda(exame.preco)} · {exame.nomeMedico}
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
        <FormularioExame exame={exame} catalogo={catalogo} />
        <ExclusaoExame exameId={exame.id} />
      </div>
    </div>
  );
}
