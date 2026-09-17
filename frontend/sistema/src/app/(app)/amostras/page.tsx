import Link from "next/link";
import { api, type ExameCatalogo, type ExameResumo, type Pagina } from "@/lib/api";
import { formatarData, formatarDocumento, hojeEmBrasilia } from "@/lib/formatos";
import { FormularioAcolhimento } from "../exames/formulario-acolhimento";

export const metadata = { title: "Amostras — Cligen" };

const TAMANHO_PAGINA = 20;

/** Fila de acolhimento: exames aguardando amostra, com registro direto na linha. */
export default async function PaginaAmostras({
  searchParams,
}: {
  searchParams: Promise<{ busca?: string; pagina?: string }>;
}) {
  const { busca = "", pagina: paginaParam } = await searchParams;
  const pagina = Math.max(1, Number(paginaParam) || 1);

  const consulta = new URLSearchParams({
    estado: "AguardandoAmostra",
    pagina: String(pagina),
    tamanhoPagina: String(TAMANHO_PAGINA),
  });
  if (busca.trim()) consulta.set("busca", busca.trim());

  const [resultado, catalogo] = await Promise.all([
    api<Pagina<ExameResumo>>(`/api/exames?${consulta}`),
    api<ExameCatalogo[]>("/api/catalogo-exames"),
  ]);
  const prazoEntrega = new Map(catalogo.map((c) => [c.id, c.prazoEntregaDias]));
  const totalPaginas = Math.max(1, Math.ceil(resultado.total / TAMANHO_PAGINA));
  const hoje = hojeEmBrasilia();

  const linkPagina = (p: number) => {
    const q = new URLSearchParams({ pagina: String(p) });
    if (busca.trim()) q.set("busca", busca.trim());
    return `/amostras?${q}`;
  };

  return (
    <div className="mx-auto max-w-6xl">
      <h1 className="text-2xl font-semibold text-primaria">Amostras</h1>
      <p className="mt-1 text-sm text-texto-suave">
        Exames aguardando amostra. O prazo do paciente começa a contar na data de acolhimento, não no cadastro.
      </p>

      <section className="mt-6 rounded-2xl border border-borda bg-white shadow-sm">
        <form action="/amostras" className="flex flex-wrap items-center gap-3 border-b border-borda p-4">
          <input
            type="search"
            name="busca"
            defaultValue={busca}
            placeholder="Buscar por paciente, documento ou exame"
            aria-label="Buscar na fila de amostras"
            className="min-w-60 flex-1 rounded-lg border border-borda bg-white px-3 py-2.5 text-sm outline-none transition focus:border-primaria focus:ring-2 focus:ring-primaria/40"
          />
          <button
            type="submit"
            className="rounded-pill border border-primaria px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt"
          >
            Buscar
          </button>
          {busca && (
            <Link href="/amostras" className="text-sm text-teal hover:underline">
              Limpar
            </Link>
          )}
          <span className="ml-auto text-xs text-texto-suave">
            {resultado.total} {resultado.total === 1 ? "exame aguardando" : "exames aguardando"}
          </span>
        </form>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
                <th className="px-4 py-3 font-semibold">Exame</th>
                <th className="px-4 py-3 font-semibold">Paciente</th>
                <th className="px-4 py-3 font-semibold">Entrada</th>
                <th className="px-4 py-3 text-right font-semibold">Acolhimento</th>
              </tr>
            </thead>
            <tbody>
              {resultado.itens.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-8 text-center text-texto-suave">
                    {busca ? "Nenhum exame aguardando amostra para essa busca." : "Nenhum exame aguardando amostra."}
                  </td>
                </tr>
              )}
              {resultado.itens.map((e) => (
                <tr key={e.id} className="border-b border-borda/60 align-top last:border-0 hover:bg-fundo">
                  <td className="px-4 py-3">
                    <Link href={`/exames/${e.id}`} className="font-medium text-teal hover:underline">
                      {e.exameNome}
                    </Link>
                  </td>
                  <td className="px-4 py-3">
                    <p className="text-texto">{e.pacienteNome}</p>
                    <p className="text-xs text-texto-suave">
                      {formatarDocumento(e.pacienteTipoDocumento, e.pacienteNumeroDocumento)}
                    </p>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">{formatarData(e.dataEntrada)}</td>
                  <td className="px-4 py-3">
                    <FormularioAcolhimento
                      exameId={e.id}
                      dataEntrada={e.dataEntrada}
                      prazoEntregaDias={prazoEntrega.get(e.exameCatalogoId)}
                      hoje={hoje}
                      compacto
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {totalPaginas > 1 && (
          <nav aria-label="Paginação" className="flex items-center justify-between border-t border-borda px-4 py-3 text-sm">
            {pagina > 1 ? (
              <Link href={linkPagina(pagina - 1)} className="font-semibold text-primaria hover:underline">
                ← Anterior
              </Link>
            ) : (
              <span />
            )}
            <span className="text-xs text-texto-suave">
              Página {pagina} de {totalPaginas}
            </span>
            {pagina < totalPaginas ? (
              <Link href={linkPagina(pagina + 1)} className="font-semibold text-primaria hover:underline">
                Próxima →
              </Link>
            ) : (
              <span />
            )}
          </nav>
        )}
      </section>
    </div>
  );
}
