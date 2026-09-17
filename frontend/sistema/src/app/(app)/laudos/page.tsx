import Link from "next/link";
import { api, type EstadoExame, type ExameResumo, type Pagina } from "@/lib/api";
import { formatarData, formatarDocumento, hojeEmBrasilia, ROTULOS_ESTADO } from "@/lib/formatos";
import { EtiquetaEstado } from "../exames/etiqueta-estado";

export const metadata = { title: "Laudos — Cligen" };

/** Fila de trabalho do laudo: cada grupo é um estado, com a próxima ação esperada. */
const GRUPOS: { estado: EstadoExame; proximaAcao: string }[] = [
  { estado: "LaudoRevisado", proximaAcao: "Disponibilizar ao paciente" },
  { estado: "LaudoCligenParaRevisao", proximaAcao: "Registrar o laudo revisado" },
  { estado: "LaudoParceiroPronto", proximaAcao: "Registrar o laudo Cligen para revisão" },
  { estado: "AmostraAcolhida", proximaAcao: "Aguardando o laudo do parceiro" },
];

export default async function PaginaLaudos() {
  const hoje = hojeEmBrasilia();
  const listas = await Promise.all(
    GRUPOS.map((g) => api<Pagina<ExameResumo>>(`/api/exames?estado=${g.estado}&tamanhoPagina=100`)),
  );
  const total = listas.reduce((s, l) => s + l.total, 0);

  return (
    <div className="mx-auto max-w-6xl">
      <h1 className="text-2xl font-semibold text-primaria">Laudos</h1>
      <p className="mt-1 text-sm text-texto-suave">
        Exames em execução ou com laudo em andamento, agrupados pela próxima ação. {total}{" "}
        {total === 1 ? "exame" : "exames"} no fluxo.
      </p>

      <div className="mt-6 space-y-6">
        {GRUPOS.map((g, i) => {
          const lista = listas[i];
          return (
            <section key={g.estado} className="rounded-2xl border border-borda bg-white shadow-sm">
              <div className="flex flex-wrap items-center justify-between gap-3 border-b border-borda px-4 py-3">
                <div className="flex items-center gap-3">
                  <EtiquetaEstado estado={g.estado} />
                  <p className="text-sm text-texto">
                    {ROTULOS_ESTADO[g.estado]} · <span className="text-texto-suave">{g.proximaAcao}</span>
                  </p>
                </div>
                <span className="text-xs text-texto-suave">
                  {lista.total} {lista.total === 1 ? "exame" : "exames"}
                  {lista.total > lista.itens.length && ` (mostrando ${lista.itens.length})`}
                </span>
              </div>
              {lista.itens.length === 0 ? (
                <p className="px-4 py-5 text-sm text-texto-suave">Nenhum exame neste estado.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <tbody>
                      {lista.itens.map((e) => {
                        const atrasado = !!e.dataLiberacaoPrevista && e.dataLiberacaoPrevista < hoje;
                        return (
                          <tr key={e.id} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
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
                            <td className="px-4 py-3 text-right whitespace-nowrap">
                              {e.dataLiberacaoPrevista && (
                                <>
                                  <p className={`text-xs ${atrasado ? "font-semibold text-erro" : "text-texto-suave"}`}>
                                    Previsto {formatarData(e.dataLiberacaoPrevista)}
                                  </p>
                                  {atrasado && <p className="text-xs text-erro">Data prevista já passou</p>}
                                </>
                              )}
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              )}
            </section>
          );
        })}
      </div>
    </div>
  );
}
