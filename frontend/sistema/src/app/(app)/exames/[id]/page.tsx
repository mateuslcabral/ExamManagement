import Link from "next/link";
import { notFound } from "next/navigation";
import { api, ApiError, type Exame } from "@/lib/api";
import { ORIGENS } from "@/lib/exames";
import { formatarData, formatarMoeda } from "@/lib/idade";
import { Etiqueta } from "@/components/ui/etiqueta";
import { FormularioExame } from "../formulario-exame";
import { EtiquetaEstado } from "../tabela-exames";
import { AcoesExame } from "./acoes-exame";
import { Anexos } from "./anexos";
import { PainelAmostra } from "./amostra";
import { PainelLaudo } from "./laudo";

export const metadata = { title: "Exame — Cligen" };

type Busca = { editar?: string; criado?: string; salvo?: string };

export default async function PaginaExame({
  params,
  searchParams,
}: {
  params: Promise<{ id: string }>;
  searchParams: Promise<Busca>;
}) {
  const [{ id }, { editar, criado, salvo }] = await Promise.all([params, searchParams]);

  let exame: Exame;
  try {
    exame = await api<Exame>(`/api/exames/${id}`);
  } catch (e) {
    if (e instanceof ApiError && e.status === 404) notFound();
    throw e;
  }

  const emEdicao = editar === "1" && !exame.excluido;
  const aviso = criado ? "Exame cadastrado." : salvo ? "Alterações salvas." : null;

  return (
    <div className="mx-auto max-w-5xl">
      <Link href="/exames" className="text-sm text-texto-suave hover:text-texto">
        ← Exames
      </Link>

      <div className="mt-2 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">{exame.exameNome}</h1>
          <p className="mt-1 flex flex-wrap items-center gap-2 text-sm text-texto-suave">
            <Link href={`/pacientes/${exame.pacienteId}`} className="font-medium text-texto hover:underline">
              {exame.pacienteNome}
            </Link>
            <span aria-hidden>·</span>
            <span>entrada em {formatarData(exame.dataEntrada)}</span>
            {exame.excluido ? <Etiqueta cor="vermelho">Excluído</Etiqueta> : <EtiquetaEstado estado={exame.estado} />}
          </p>
        </div>
        {!emEdicao && <AcoesExame exame={exame} />}
      </div>

      {aviso && (
        <p role="status" className="mt-4 rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
          {aviso}
        </p>
      )}

      {exame.excluido && (
        <div className="mt-4 rounded-xl border border-erro/30 bg-erro/5 p-4 text-sm">
          <p className="font-semibold text-erro">Exame excluído em {formatarData(exame.excluidoEm ?? "")}</p>
          <p className="mt-1 text-texto">Motivo: {exame.motivoExclusao}</p>
          <p className="mt-1 text-xs text-texto-suave">Registro preservado (prontuário). Restaure para voltar a editá-lo.</p>
        </div>
      )}

      {emEdicao ? (
        <section className="mt-6 rounded-2xl border border-borda bg-white p-6 shadow-sm">
          <h2 className="mb-4 font-semibold text-texto">Editar exame</h2>
          <FormularioExame exame={exame} />
        </section>
      ) : (
        <div className="mt-6 grid gap-6 lg:grid-cols-2">
          <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
            <h2 className="font-semibold text-texto">Solicitação</h2>
            <dl className="mt-3 space-y-2 text-sm">
              <Linha rotulo="Origem" valor={ORIGENS[exame.origem]} />
              <Linha rotulo="Destino" valor={exame.destino ?? "—"} />
              <Linha rotulo="Médico solicitante" valor={`${exame.nomeMedico} (${exame.tipoMedico === "Interno" ? "interno" : "externo"})`} />
              <Linha rotulo="Preço" valor={formatarMoeda(exame.preco)} />
            </dl>
          </section>

          <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
            <PainelAmostra exame={exame} />
          </section>

          <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm lg:col-span-2">
            <PainelLaudo exame={exame} />
          </section>

          <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm lg:col-span-2">
            <Anexos exame={exame} />
          </section>

          <p className="text-xs text-texto-suave lg:col-span-2">
            Cadastrado em {formatarData(exame.criadoEm)}
            {exame.atualizadoEm && ` · atualizado em ${formatarData(exame.atualizadoEm)}`}
          </p>
        </div>
      )}
    </div>
  );
}

function Linha({ rotulo, valor }: { rotulo: string; valor: string }) {
  return (
    <div className="flex gap-3">
      <dt className="w-40 shrink-0 text-texto-suave">{rotulo}</dt>
      <dd className="text-texto">{valor}</dd>
    </div>
  );
}
