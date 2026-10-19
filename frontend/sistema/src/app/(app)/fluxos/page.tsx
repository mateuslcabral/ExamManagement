import Link from "next/link";
import { api, type CampoDataExame, type EstadoExame, type ExameResumo, type Pagina } from "@/lib/api";
import { formatarData, formatarDataHora, formatarDocumento, hojeEmBrasilia, ROTULOS_CAMPO_DATA, ROTULOS_ESTADO } from "@/lib/formatos";
import { EtiquetaEstado } from "../exames/etiqueta-estado";

export const metadata = { title: "Fluxos — Cligen" };

/** Fila de trabalho na ordem do fluxo: do cadastro à disponibilização. Cada grupo é um estado e a próxima ação esperada. */
const GRUPOS: { estado: EstadoExame; proximaAcao: string }[] = [
  { estado: "AguardandoAmostra", proximaAcao: "Registrar o acolhimento da amostra" },
  { estado: "AmostraAcolhida", proximaAcao: "Aguardando o laudo do parceiro" },
  { estado: "LaudoParceiroPronto", proximaAcao: "Registrar o laudo Cligen para revisão" },
  { estado: "LaudoCligenParaRevisao", proximaAcao: "Registrar o laudo revisado" },
  { estado: "LaudoRevisado", proximaAcao: "Disponibilizar ao paciente" },
  { estado: "Disponibilizado", proximaAcao: "Concluído" },
];

const CAMPOS = Object.keys(ROTULOS_CAMPO_DATA) as CampoDataExame[];

export default async function PaginaFluxos({ searchParams }: { searchParams: Promise<{ campo?: string; de?: string; ate?: string }> }) {
  const { campo: campoParam, de, ate } = await searchParams;
  const hoje = hojeEmBrasilia();
  const campo = CAMPOS.find((c) => c === campoParam);
  const periodoValido = (d?: string) => !!d && /^\d{4}-\d{2}-\d{2}$/.test(d);
  const filtrando = !!campo && (periodoValido(de) || periodoValido(ate));

  const filtro = new URLSearchParams();
  if (filtrando) {
    filtro.set("campoData", campo!);
    if (periodoValido(de)) filtro.set("de", de!);
    if (periodoValido(ate)) filtro.set("ate", ate!);
  }

  const listas = await Promise.all(
    GRUPOS.map((g) => api<Pagina<ExameResumo>>(`/api/exames?estado=${g.estado}&tamanhoPagina=100&${filtro}`)),
  );
  const total = listas.reduce((s, l) => s + l.total, 0);

  return (
    <div className="mx-auto max-w-6xl">
      <h1 className="text-2xl font-semibold text-primaria">Fluxos</h1>
      <p className="mt-1 text-sm text-texto-suave">
        Todos os exames, do cadastro à disponibilização, agrupados pela próxima ação.{" "}
        {filtrando ? `${total} ${total === 1 ? "exame" : "exames"} no período.` : `${total} ${total === 1 ? "exame" : "exames"} no total.`}
      </p>

      <form action="/fluxos" className="mt-6 flex flex-wrap items-end gap-3 rounded-2xl border border-borda bg-white p-4 shadow-sm">
        <div>
          <label htmlFor="campo" className="mb-1 block text-sm font-medium text-texto">
            Filtrar por
          </label>
          <select id="campo" name="campo" defaultValue={campo ?? "Prevista"} className="rounded-lg border border-borda bg-white px-3 py-2 text-sm">
            {CAMPOS.map((c) => (
              <option key={c} value={c}>
                {ROTULOS_CAMPO_DATA[c]}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label htmlFor="de" className="mb-1 block text-sm font-medium text-texto">
            De
          </label>
          <input id="de" type="date" name="de" defaultValue={periodoValido(de) ? de : ""} className="rounded-lg border border-borda px-3 py-2 text-sm" />
        </div>
        <div>
          <label htmlFor="ate" className="mb-1 block text-sm font-medium text-texto">
            Até
          </label>
          <input id="ate" type="date" name="ate" defaultValue={periodoValido(ate) ? ate : ""} className="rounded-lg border border-borda px-3 py-2 text-sm" />
        </div>
        <button type="submit" className="rounded-pill border border-primaria px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt">
          Aplicar
        </button>
        {filtrando && (
          <Link href="/fluxos" className="text-sm text-teal hover:underline">
            Limpar filtro
          </Link>
        )}
        <span className="ml-auto text-xs text-texto-suave">
          {filtrando ? `Só exames com ${ROTULOS_CAMPO_DATA[campo!].toLowerCase()} no período.` : "Ex.: previstos para esta semana, ou entregues no mês."}
        </span>
      </form>

      <div className="mt-6 space-y-6">
        {GRUPOS.map((g, i) => {
          const lista = listas[i];
          return (
            <section key={g.estado} className="rounded-2xl border border-borda bg-white shadow-sm">
              <div className="flex flex-wrap items-center justify-between gap-3 border-b border-borda px-4 py-3">
                <div className="flex items-center gap-3">
                  <span className="inline-flex size-6 items-center justify-center rounded-full bg-primaria/10 text-xs font-semibold text-primaria">{i + 1}</span>
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
                <p className="px-4 py-4 text-sm text-texto-suave">Nenhum exame neste estado{filtrando ? " no período" : ""}.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <tbody>
                      {lista.itens.map((e) => {
                        const atrasado = e.estado !== "Disponibilizado" && !!e.dataLiberacaoPrevista && e.dataLiberacaoPrevista < hoje;
                        return (
                          <tr key={e.id} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
                            <td className="px-4 py-3">
                              <Link href={`/exames/${e.id}`} className="font-medium text-teal hover:underline">
                                {e.exameNome}
                              </Link>
                              <p className="text-xs text-texto-suave">
                                {e.codigo} · {e.origemNome}
                                {e.destinoNome && ` → ${e.destinoNome}`}
                              </p>
                            </td>
                            <td className="px-4 py-3">
                              <p className="text-texto">{e.pacienteNome}</p>
                              <p className="text-xs text-texto-suave">{formatarDocumento(e.pacienteTipoDocumento, e.pacienteNumeroDocumento)}</p>
                            </td>
                            <td className="px-4 py-3 text-right whitespace-nowrap text-xs text-texto-suave">
                              <p>Entrada {formatarData(e.dataEntrada)}</p>
                              {e.dataAcolhimento && <p>Acolhida {formatarData(e.dataAcolhimento)}</p>}
                              {e.dataLiberacaoPrevista && (
                                <p className={atrasado ? "font-semibold text-erro" : ""}>
                                  Prevista {formatarData(e.dataLiberacaoPrevista)}
                                  {atrasado && " · venceu"}
                                </p>
                              )}
                              {e.dataLiberacaoEfetiva && <p className="text-sucesso">Entregue {formatarDataHora(e.dataLiberacaoEfetiva)}</p>}
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
