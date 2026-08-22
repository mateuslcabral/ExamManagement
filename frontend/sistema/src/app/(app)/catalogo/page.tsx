import { api, type ExameCatalogo, type Parametro } from "@/lib/api";
import { FormularioCatalogo } from "./formulario-catalogo";
import { FormularioDiasRevisao } from "./formulario-dias-revisao";
import { TabelaCatalogo } from "./tabela-catalogo";

export const metadata = { title: "Catálogo de exames — Cligen" };

export default async function PaginaCatalogo() {
  const [itens, parametros] = await Promise.all([
    api<ExameCatalogo[]>("/api/catalogo-exames"),
    api<Parametro[]>("/api/parametros"),
  ]);
  const diasRevisao = parametros.find((p) => p.chave === "DiasRevisao")?.valor ?? "3";

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">Catálogo de exames</h1>
          <p className="mt-1 text-sm text-texto-suave">
            Prazo fixo por exame, em dias corridos. A previsão ao paciente soma os dias de revisão da Cligen.
          </p>
        </div>
        <p className="text-xs text-texto-suave">
          {itens.length} {itens.length === 1 ? "exame" : "exames"}
        </p>
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-[360px_1fr]">
        <div className="space-y-6">
          <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
            <h2 className="font-semibold text-texto">Novo exame</h2>
            <FormularioCatalogo diasRevisao={Number(diasRevisao)} />
          </section>

          <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
            <h2 className="font-semibold text-texto">Dias de revisão</h2>
            <p className="mt-1 text-xs text-texto-suave">
              Parâmetro global somado ao prazo de execução de todo exame. Vale para todos, inclusive os executados internamente.
            </p>
            <FormularioDiasRevisao valorAtual={diasRevisao} />
          </section>
        </div>

        <section className="rounded-2xl border border-borda bg-white shadow-sm">
          <TabelaCatalogo itens={itens} />
        </section>
      </div>
    </div>
  );
}
