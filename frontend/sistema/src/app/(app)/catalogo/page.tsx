import Link from "next/link";
import { api, type ExameCatalogo } from "@/lib/api";
import { formatarMoeda } from "@/lib/formatos";
import { Etiqueta } from "@/components/ui/etiqueta";

export const metadata = { title: "Catálogo de exames — Cligen" };

/** Visão de consulta: todos os exames ofertados e suas informações. A edição fica em /catalogo/gerenciar. */
export default async function PaginaCatalogo() {
  const exames = await api<ExameCatalogo[]>("/api/catalogo-exames");
  const ativos = exames.filter((e) => e.ativo);
  const inativos = exames.filter((e) => !e.ativo);

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">Catálogo de exames</h1>
          <p className="mt-1 text-sm text-texto-suave">
            {ativos.length} {ativos.length === 1 ? "exame ofertado" : "exames ofertados"}. Prazo em dias corridos a partir do acolhimento da amostra.
          </p>
        </div>
        <Link
          href="/catalogo/gerenciar"
          className="inline-flex items-center rounded-pill border border-primaria bg-white px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt"
        >
          Gerenciar catálogo
        </Link>
      </div>

      {ativos.length === 0 ? (
        <p className="mt-6 rounded-2xl border border-borda bg-white p-8 text-center text-sm text-texto-suave">
          Nenhum exame ativo.{" "}
          <Link href="/catalogo/gerenciar" className="font-semibold text-teal hover:underline">
            Cadastre o primeiro
          </Link>
          .
        </p>
      ) : (
        <div className="mt-6 grid gap-4 md:grid-cols-2">
          {ativos.map((e) => (
            <Cartao key={e.id} exame={e} />
          ))}
        </div>
      )}

      {inativos.length > 0 && (
        <section className="mt-10">
          <h2 className="text-xs font-semibold uppercase tracking-wider text-texto-suave">Inativos ({inativos.length})</h2>
          <div className="mt-3 grid gap-4 opacity-70 md:grid-cols-2">
            {inativos.map((e) => (
              <Cartao key={e.id} exame={e} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}

function Cartao({ exame: e }: { exame: ExameCatalogo }) {
  return (
    <article className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <h2 className="font-semibold text-texto">{e.nome}</h2>
        {!e.ativo && <Etiqueta cor="cinza">Inativo</Etiqueta>}
      </div>
      <dl className="mt-3 grid grid-cols-2 gap-x-4 gap-y-2 text-sm">
        <Item rotulo="Prazo de entrega" valor={`${e.prazoDias} dias corridos`} />
        <Item rotulo="Destino padrão" valor={e.destinoNome ?? "—"} />
        <Item rotulo="Preço de referência" valor={formatarMoeda(e.precoReferencia)} />
        <Item rotulo="Preço limite" valor={formatarMoeda(e.precoLimite)} />
        <Item rotulo="Nº de genes" valor={e.numeroGenes != null ? String(e.numeroGenes) : "—"} />
      </dl>
      {e.genes && (
        <div className="mt-3 text-sm">
          <p className="text-xs text-texto-suave">Genes</p>
          <p className="whitespace-pre-wrap text-texto">{e.genes}</p>
        </div>
      )}
      {e.informacoesTecnicas && (
        <div className="mt-3 text-sm">
          <p className="text-xs text-texto-suave">Informações técnicas</p>
          <p className="whitespace-pre-wrap text-texto">{e.informacoesTecnicas}</p>
        </div>
      )}
    </article>
  );
}

function Item({ rotulo, valor }: { rotulo: string; valor: string }) {
  return (
    <div>
      <dt className="text-xs text-texto-suave">{rotulo}</dt>
      <dd className="font-medium text-texto">{valor}</dd>
    </div>
  );
}
