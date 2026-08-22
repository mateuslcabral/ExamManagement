import Link from "next/link";
import { api, type ExameResumo } from "@/lib/api";
import { TabelaExames } from "./tabela-exames";

export const metadata = { title: "Exames — Cligen" };

type Busca = { busca?: string; excluidos?: string };

export default async function PaginaExames({ searchParams }: { searchParams: Promise<Busca> }) {
  const { busca = "", excluidos } = await searchParams;
  const incluirExcluidos = excluidos === "1";

  const qs = new URLSearchParams();
  if (busca) qs.set("busca", busca);
  if (incluirExcluidos) qs.set("incluirExcluidos", "true");
  const exames = await api<ExameResumo[]>(`/api/exames?${qs}`);

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">Exames</h1>
          <p className="mt-1 text-sm text-texto-suave">Exames solicitados, vinculados ao cadastro único de cada paciente.</p>
        </div>
        <Link
          href="/exames/novo"
          className="inline-flex items-center gap-2 rounded-pill bg-primaria px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-primaria-clara"
        >
          + Novo exame
        </Link>
      </div>

      <form className="mt-6 flex flex-wrap items-center gap-3" role="search">
        <input
          type="search"
          name="busca"
          defaultValue={busca}
          placeholder="Buscar por paciente, exame, médico ou documento"
          aria-label="Buscar exames"
          className="min-w-64 flex-1 rounded-lg border border-borda bg-white px-3 py-2.5 text-sm text-texto outline-none focus:border-primaria focus:ring-2 focus:ring-primaria/40"
        />
        <label className="flex items-center gap-2 text-sm text-texto-suave">
          <input type="checkbox" name="excluidos" value="1" defaultChecked={incluirExcluidos} className="accent-primaria" />
          Mostrar excluídos
        </label>
        <button type="submit" className="rounded-pill border border-primaria bg-white px-4 py-2 text-sm font-semibold text-primaria hover:bg-fundo-alt">
          Buscar
        </button>
        <span className="ml-auto text-xs text-texto-suave">
          {exames.length} {exames.length === 1 ? "exame" : "exames"}
        </span>
      </form>

      <section className="mt-4 rounded-2xl border border-borda bg-white shadow-sm">
        <TabelaExames exames={exames} vazio={busca ? "Nenhum exame encontrado para essa busca." : "Nenhum exame cadastrado ainda."} />
      </section>
    </div>
  );
}
