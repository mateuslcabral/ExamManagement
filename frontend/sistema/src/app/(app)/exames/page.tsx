import Link from "next/link";
import { api, ApiError, type ExameResumo, type Pagina, type Paciente } from "@/lib/api";
import { formatarData, formatarDataHora, formatarDocumento, ROTULOS_ORIGEM } from "@/lib/formatos";
import { BotaoRestaurar } from "./botao-restaurar";
import { EtiquetaEstado } from "./etiqueta-estado";

export const metadata = { title: "Exames — Cligen" };

const TAMANHO_PAGINA = 20;

export default async function PaginaExames({
  searchParams,
}: {
  searchParams: Promise<{ busca?: string; pacienteId?: string; pagina?: string; excluido?: string; excluidos?: string }>;
}) {
  const { busca = "", pacienteId, pagina: paginaParam, excluido, excluidos: excluidosParam } = await searchParams;
  const pagina = Math.max(1, Number(paginaParam) || 1);
  const excluidos = excluidosParam === "1";

  const filtros = new URLSearchParams();
  if (excluidos) filtros.set("excluidos", "true");
  if (busca.trim()) filtros.set("busca", busca.trim());
  if (pacienteId) filtros.set("pacienteId", pacienteId);

  const consulta = new URLSearchParams(filtros);
  consulta.set("pagina", String(pagina));
  consulta.set("tamanhoPagina", String(TAMANHO_PAGINA));

  const [resultado, paciente] = await Promise.all([
    api<Pagina<ExameResumo>>(`/api/exames?${consulta}`),
    pacienteId ? api<Paciente>(`/api/pacientes/${encodeURIComponent(pacienteId)}`).catch(ignorarNaoEncontrado) : null,
  ]);
  const totalPaginas = Math.max(1, Math.ceil(resultado.total / TAMANHO_PAGINA));

  const linkPagina = (p: number) => {
    const q = new URLSearchParams(filtros);
    if (excluidos) {
      q.delete("excluidos");
      q.set("excluidos", "1");
    }
    q.set("pagina", String(p));
    return `/exames?${q}`;
  };
  const linkBase = excluidos ? "/exames?excluidos=1" : "/exames";

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">{excluidos ? "Exames excluídos" : "Exames"}</h1>
          <p className="mt-1 text-sm text-texto-suave">
            {excluidos ? (
              <>
                Excluídos logicamente, com autor, data e motivo preservados. Podem ser restaurados. ·{" "}
                <Link href="/exames" className="text-teal hover:underline">
                  voltar aos ativos
                </Link>
              </>
            ) : paciente ? (
              <>
                Exames de <span className="font-semibold text-texto">{paciente.nome}</span> ·{" "}
                <Link href="/exames" className="text-teal hover:underline">
                  ver todos
                </Link>
              </>
            ) : (
              <>
                Exames solicitados, do cadastro à disponibilização do laudo. ·{" "}
                <Link href="/exames?excluidos=1" className="text-teal hover:underline">
                  ver excluídos
                </Link>
              </>
            )}
          </p>
        </div>
        {!excluidos && (
          <Link
            href={paciente ? `/exames/novo?pacienteId=${paciente.id}` : "/exames/novo"}
            className="inline-flex items-center rounded-pill bg-primaria px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-primaria-clara"
          >
            Novo exame
          </Link>
        )}
      </div>

      {excluido && (
        <p role="status" className="mt-6 rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
          Exame excluído. O registro permanece guardado com o motivo informado.
        </p>
      )}

      <section className="mt-6 rounded-2xl border border-borda bg-white shadow-sm">
        <form action="/exames" className="flex flex-wrap items-center gap-3 border-b border-borda p-4">
          {pacienteId && <input type="hidden" name="pacienteId" value={pacienteId} />}
          {excluidos && <input type="hidden" name="excluidos" value="1" />}
          <input
            type="search"
            name="busca"
            defaultValue={busca}
            placeholder="Buscar por paciente, documento ou exame"
            aria-label="Buscar exames"
            className="min-w-60 flex-1 rounded-lg border border-borda bg-white px-3 py-2.5 text-sm outline-none transition focus:border-primaria focus:ring-2 focus:ring-primaria/40"
          />
          <button
            type="submit"
            className="rounded-pill border border-primaria px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt"
          >
            Buscar
          </button>
          {busca && (
            <Link href={pacienteId ? `${linkBase}${excluidos ? "&" : "?"}pacienteId=${pacienteId}` : linkBase} className="text-sm text-teal hover:underline">
              Limpar
            </Link>
          )}
          <span className="ml-auto text-xs text-texto-suave">
            {resultado.total} {resultado.total === 1 ? "exame" : "exames"}
          </span>
        </form>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
                <th className="px-4 py-3 font-semibold">Exame</th>
                <th className="px-4 py-3 font-semibold">Paciente</th>
                <th className="px-4 py-3 font-semibold">Entrada</th>
                <th className="px-4 py-3 font-semibold">{excluidos ? "Exclusão" : "Situação"}</th>
                {excluidos && <th className="px-4 py-3" />}
              </tr>
            </thead>
            <tbody>
              {resultado.itens.length === 0 && (
                <tr>
                  <td colSpan={excluidos ? 5 : 4} className="px-4 py-8 text-center text-texto-suave">
                    {busca ? "Nenhum exame encontrado para essa busca." : excluidos ? "Nenhum exame excluído." : "Nenhum exame cadastrado."}
                  </td>
                </tr>
              )}
              {resultado.itens.map((e) => (
                <tr key={e.id} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
                  <td className="px-4 py-3">
                    {excluidos ? (
                      <span className="font-medium text-texto">{e.exameNome}</span>
                    ) : (
                      <Link href={`/exames/${e.id}`} className="font-medium text-teal hover:underline">
                        {e.exameNome}
                      </Link>
                    )}
                    <p className="text-xs text-texto-suave">{ROTULOS_ORIGEM[e.origem]}</p>
                  </td>
                  <td className="px-4 py-3">
                    <p className="text-texto">{e.pacienteNome}</p>
                    <p className="text-xs text-texto-suave">
                      {formatarDocumento(e.pacienteTipoDocumento, e.pacienteNumeroDocumento)}
                    </p>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">{formatarData(e.dataEntrada)}</td>
                  <td className="px-4 py-3">
                    {excluidos ? (
                      <>
                        <p className="text-xs text-texto-suave">{e.excluidoEm && formatarDataHora(e.excluidoEm)}</p>
                        <p className="text-texto">{e.motivoExclusao}</p>
                      </>
                    ) : (
                      <>
                        <EtiquetaEstado estado={e.estado} />
                        {e.dataLiberacaoPrevista && (
                          <p className="mt-1 text-xs text-texto-suave">Previsto: {formatarData(e.dataLiberacaoPrevista)}</p>
                        )}
                      </>
                    )}
                  </td>
                  {excluidos && (
                    <td className="px-4 py-3">
                      <BotaoRestaurar exameId={e.id} nome={e.exameNome} />
                    </td>
                  )}
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

function ignorarNaoEncontrado(e: unknown): null {
  if (e instanceof ApiError && (e.status === 404 || e.status === 400)) return null;
  throw e;
}
