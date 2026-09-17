import { api, type DiasRevisao, type ExameCatalogo } from "@/lib/api";
import { CatalogoExames } from "./catalogo-exames";
import { FormularioDiasRevisao } from "./formulario-dias-revisao";

export const metadata = { title: "Catálogo de exames — Cligen" };

export default async function PaginaCatalogo() {
  const [exames, revisao] = await Promise.all([
    api<ExameCatalogo[]>("/api/catalogo-exames"),
    api<DiasRevisao>("/api/parametros/dias-revisao"),
  ]);
  const ativos = exames.filter((e) => e.ativo).length;

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">Catálogo de exames</h1>
          <p className="mt-1 text-sm text-texto-suave">
            Exames oferecidos pela Cligen. Exame aposentado é desativado, nunca apagado, para preservar o histórico.
          </p>
        </div>
        <p className="text-xs text-texto-suave">
          {ativos} {ativos === 1 ? "exame ativo" : "exames ativos"}
          {exames.length > ativos && ` · ${exames.length - ativos} inativo${exames.length - ativos === 1 ? "" : "s"}`}
        </p>
      </div>

      <CatalogoExames
        exames={exames}
        diasRevisao={revisao.dias}
        painelLateral={<FormularioDiasRevisao dias={revisao.dias} />}
      />
    </div>
  );
}
