import Link from "next/link";
import { api, ApiError, type CampoDataExame, type EstadoExame, type ExameResumo, type Paciente, type Pagina } from "@/lib/api";
import { diasEntre, formatarData, formatarDataHora, formatarDocumento, hojeEmBrasilia, ROTULOS_CAMPO_DATA, ROTULOS_ESTADO } from "@/lib/formatos";
import { BotaoRestaurar } from "../exames/botao-restaurar";
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

/** Mesmas larguras em todos os grupos, para as colunas ficarem alinhadas de uma seção para a outra. */
const COLUNAS = "grid grid-cols-[minmax(0,1.6fr)_minmax(0,1.2fr)_9rem_11rem] items-center gap-4 px-4 py-3";
const TAMANHO_PAGINA_EXCLUIDOS = 20;
const LIMITE_POR_GRUPO = 100;

type Parametros = {
  busca?: string;
  pacienteId?: string;
  campo?: string;
  de?: string;
  ate?: string;
  excluidos?: string;
  excluido?: string;
  pagina?: string;
};

/**
 * Gestão completa dos exames (pedido do cliente, 20/10/2026): fila por estado, busca, exames de um paciente,
 * filtro por data e a lista de excluídos com restauração. O cadastro fica no menu Cadastros → Novo exame.
 */
export default async function PaginaFluxos({ searchParams }: { searchParams: Promise<Parametros> }) {
  const p = await searchParams;
  const hoje = hojeEmBrasilia();
  const busca = (p.busca ?? "").trim();
  const pacienteId = p.pacienteId || undefined;
  const excluidos = p.excluidos === "1";
  const campo = CAMPOS.find((c) => c === p.campo);
  const periodoValido = (d?: string) => !!d && /^\d{4}-\d{2}-\d{2}$/.test(d);
  const filtrandoData = !!campo && (periodoValido(p.de) || periodoValido(p.ate));

  // Filtros comuns às consultas e aos links (sem paginação).
  const filtros = new URLSearchParams();
  if (busca) filtros.set("busca", busca);
  if (pacienteId) filtros.set("pacienteId", pacienteId);
  if (filtrandoData) {
    filtros.set("campoData", campo!);
    if (periodoValido(p.de)) filtros.set("de", p.de!);
    if (periodoValido(p.ate)) filtros.set("ate", p.ate!);
  }
  const linkFluxos = (extra?: Record<string, string>) => {
    const q = new URLSearchParams();
    if (busca) q.set("busca", busca);
    if (pacienteId) q.set("pacienteId", pacienteId);
    if (filtrandoData) {
      q.set("campo", campo!);
      if (periodoValido(p.de)) q.set("de", p.de!);
      if (periodoValido(p.ate)) q.set("ate", p.ate!);
    }
    for (const [k, v] of Object.entries(extra ?? {})) q.set(k, v);
    const s = q.toString();
    return s ? `/fluxos?${s}` : "/fluxos";
  };

  const paciente = pacienteId ? await api<Paciente>(`/api/pacientes/${encodeURIComponent(pacienteId)}`).catch(ignorarNaoEncontrado) : null;

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">{excluidos ? "Exames excluídos" : "Fluxos"}</h1>
          <p className="mt-1 text-sm text-texto-suave">
            {excluidos ? (
              <>
                Excluídos logicamente, com autor, data e motivo preservados. Podem ser restaurados. ·{" "}
                <Link href={linkFluxos()} className="text-teal hover:underline">
                  voltar à fila
                </Link>
              </>
            ) : paciente ? (
              <>
                Exames de <span className="font-semibold text-texto">{paciente.nome}</span> ·{" "}
                <Link href="/fluxos" className="text-teal hover:underline">
                  ver todos
                </Link>
              </>
            ) : (
              <>
                Todos os exames, do cadastro à disponibilização, agrupados pela próxima ação. ·{" "}
                <Link href={linkFluxos({ excluidos: "1" })} className="text-teal hover:underline">
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

      {p.excluido && (
        <p role="status" className="mt-6 rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
          Exame excluído. O registro permanece guardado com o motivo informado.
        </p>
      )}

      <form action="/fluxos" className="mt-6 flex flex-wrap items-end gap-3 rounded-2xl border border-borda bg-white p-4 shadow-sm">
        {pacienteId && <input type="hidden" name="pacienteId" value={pacienteId} />}
        {excluidos && <input type="hidden" name="excluidos" value="1" />}
        <div className="min-w-64 flex-1">
          <label htmlFor="busca" className="mb-1 block text-sm font-medium text-texto">
            Buscar
          </label>
          <input
            id="busca"
            type="search"
            name="busca"
            defaultValue={busca}
            placeholder="Paciente, documento, exame ou código (EX-000123)"
            className="w-full rounded-lg border border-borda bg-white px-3 py-2 text-sm outline-none transition focus:border-primaria focus:ring-2 focus:ring-primaria/40"
          />
        </div>
        {!excluidos && (
          <>
            <div>
              <label htmlFor="campo" className="mb-1 block text-sm font-medium text-texto">
                Data
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
              <input id="de" type="date" name="de" defaultValue={periodoValido(p.de) ? p.de : ""} className="rounded-lg border border-borda px-3 py-2 text-sm" />
            </div>
            <div>
              <label htmlFor="ate" className="mb-1 block text-sm font-medium text-texto">
                Até
              </label>
              <input id="ate" type="date" name="ate" defaultValue={periodoValido(p.ate) ? p.ate : ""} className="rounded-lg border border-borda px-3 py-2 text-sm" />
            </div>
          </>
        )}
        <button type="submit" className="rounded-pill border border-primaria px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt">
          Aplicar
        </button>
        {(busca || filtrandoData) && (
          <Link href={pacienteId ? `/fluxos?pacienteId=${pacienteId}${excluidos ? "&excluidos=1" : ""}` : excluidos ? "/fluxos?excluidos=1" : "/fluxos"} className="text-sm text-teal hover:underline">
            Limpar
          </Link>
        )}
      </form>

      {excluidos ? (
        <Excluidos filtros={filtros} pagina={Math.max(1, Number(p.pagina) || 1)} linkPagina={(n) => linkFluxos({ excluidos: "1", pagina: String(n) })} busca={busca} />
      ) : (
        <Fila filtros={filtros} hoje={hoje} filtrando={filtrandoData || !!busca || !!pacienteId} campo={campo} />
      )}
    </div>
  );
}

async function Fila({ filtros, hoje, filtrando, campo }: { filtros: URLSearchParams; hoje: string; filtrando: boolean; campo?: CampoDataExame }) {
  const listas = await Promise.all(
    GRUPOS.map((g) => api<Pagina<ExameResumo>>(`/api/exames?estado=${g.estado}&tamanhoPagina=${LIMITE_POR_GRUPO}&${filtros}`)),
  );
  const total = listas.reduce((s, l) => s + l.total, 0);

  return (
    <>
      <p className="mt-4 text-xs text-texto-suave">
        {total} {total === 1 ? "exame" : "exames"}
        {filtrando ? " com os filtros aplicados" : " no total"}
        {campo && filtros.has("campoData") && ` · só exames com ${ROTULOS_CAMPO_DATA[campo].toLowerCase()} no período`}.
      </p>
      <div className={`${COLUNAS} mt-2 py-1 text-[11px] font-semibold uppercase tracking-wider text-texto-suave`}>
        <span>Exame</span>
        <span>Paciente</span>
        <span className="text-center">Prazo</span>
        <span className="text-right">Datas</span>
      </div>
      <div className="space-y-6">
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
                <p className="px-4 py-4 text-sm text-texto-suave">Nenhum exame neste estado{filtrando ? " com os filtros aplicados" : ""}.</p>
              ) : (
                <ul className="divide-y divide-borda/60 text-sm">
                  {lista.itens.map((e) => (
                    <li key={e.id} className={`${COLUNAS} hover:bg-fundo`}>
                      <div className="min-w-0">
                        <Link href={`/exames/${e.id}`} className="font-medium text-teal hover:underline">
                          {e.exameNome}
                        </Link>
                        <p className="truncate text-xs text-texto-suave">
                          {e.codigo} · {e.origemNome}
                          {e.destinoNome && ` → ${e.destinoNome}`}
                        </p>
                      </div>
                      <div className="min-w-0">
                        <Link href={`/fluxos?pacienteId=${e.pacienteId}`} className="block truncate text-texto hover:underline">
                          {e.pacienteNome}
                        </Link>
                        <p className="text-xs text-texto-suave">{formatarDocumento(e.pacienteTipoDocumento, e.pacienteNumeroDocumento)}</p>
                      </div>
                      <Prazo exame={e} hoje={hoje} />
                      <div className="whitespace-nowrap text-right text-xs text-texto-suave">
                        <p>Entrada {formatarData(e.dataEntrada)}</p>
                        {e.dataAcolhimento && <p>Acolhida {formatarData(e.dataAcolhimento)}</p>}
                        {e.dataLiberacaoPrevista && <p>Prevista {formatarData(e.dataLiberacaoPrevista)}</p>}
                        {e.dataLiberacaoEfetiva && <p className="text-sucesso">Entregue {formatarDataHora(e.dataLiberacaoEfetiva)}</p>}
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </section>
          );
        })}
      </div>
    </>
  );
}

async function Excluidos({
  filtros,
  pagina,
  linkPagina,
  busca,
}: {
  filtros: URLSearchParams;
  pagina: number;
  linkPagina: (n: number) => string;
  busca: string;
}) {
  const consulta = new URLSearchParams(filtros);
  consulta.set("excluidos", "true");
  consulta.set("pagina", String(pagina));
  consulta.set("tamanhoPagina", String(TAMANHO_PAGINA_EXCLUIDOS));
  const resultado = await api<Pagina<ExameResumo>>(`/api/exames?${consulta}`);
  const totalPaginas = Math.max(1, Math.ceil(resultado.total / TAMANHO_PAGINA_EXCLUIDOS));

  return (
    <section className="mt-6 rounded-2xl border border-borda bg-white shadow-sm">
      <div className="flex items-center justify-between border-b border-borda px-4 py-3">
        <h2 className="font-semibold text-texto">Excluídos</h2>
        <span className="text-xs text-texto-suave">
          {resultado.total} {resultado.total === 1 ? "exame" : "exames"}
        </span>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
              <th className="px-4 py-3 font-semibold">Exame</th>
              <th className="px-4 py-3 font-semibold">Paciente</th>
              <th className="px-4 py-3 font-semibold">Entrada</th>
              <th className="px-4 py-3 font-semibold">Exclusão</th>
              <th className="px-4 py-3" />
            </tr>
          </thead>
          <tbody>
            {resultado.itens.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-texto-suave">
                  {busca ? "Nenhum exame excluído encontrado para essa busca." : "Nenhum exame excluído."}
                </td>
              </tr>
            )}
            {resultado.itens.map((e) => (
              <tr key={e.id} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
                <td className="px-4 py-3">
                  <span className="font-medium text-texto">{e.exameNome}</span>
                  <p className="text-xs text-texto-suave">
                    {e.codigo} · {e.origemNome}
                  </p>
                </td>
                <td className="px-4 py-3">
                  <p className="text-texto">{e.pacienteNome}</p>
                  <p className="text-xs text-texto-suave">{formatarDocumento(e.pacienteTipoDocumento, e.pacienteNumeroDocumento)}</p>
                </td>
                <td className="px-4 py-3 whitespace-nowrap">{formatarData(e.dataEntrada)}</td>
                <td className="px-4 py-3">
                  <p className="text-xs text-texto-suave">{e.excluidoEm && formatarDataHora(e.excluidoEm)}</p>
                  <p className="text-texto">{e.motivoExclusao}</p>
                </td>
                <td className="px-4 py-3">
                  <BotaoRestaurar exameId={e.id} nome={e.exameNome} />
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
  );
}

/** Quantos dias faltam para a data prevista; vermelho quando já passou. Entregue e sem amostra não têm contagem. */
function Prazo({ exame: e, hoje }: { exame: ExameResumo; hoje: string }) {
  if (e.estado === "Disponibilizado") {
    const antecedencia = e.dataLiberacaoEfetiva && e.dataLiberacaoPrevista ? diasEntre(e.dataLiberacaoEfetiva.slice(0, 10), e.dataLiberacaoPrevista) : null;
    return (
      <div className="text-center">
        <p className="text-sm font-semibold text-sucesso">Entregue</p>
        {antecedencia !== null && (
          <p className="text-xs text-texto-suave">{antecedencia >= 0 ? `${antecedencia} ${antecedencia === 1 ? "dia" : "dias"} antes` : `${-antecedencia} ${antecedencia === -1 ? "dia" : "dias"} depois`}</p>
        )}
      </div>
    );
  }
  if (!e.dataLiberacaoPrevista)
    return (
      <div className="text-center">
        <p className="text-sm text-texto-suave">—</p>
        <p className="text-xs text-texto-suave">sem previsão</p>
      </div>
    );
  const dias = diasEntre(hoje, e.dataLiberacaoPrevista);
  if (dias < 0)
    return (
      <div className="text-center">
        <p className="text-lg font-semibold leading-tight text-erro">{-dias}</p>
        <p className="text-xs font-semibold text-erro">{-dias === 1 ? "dia de atraso" : "dias de atraso"}</p>
      </div>
    );
  if (dias === 0)
    return (
      <div className="text-center">
        <p className="text-lg font-semibold leading-tight text-alerta">Hoje</p>
        <p className="text-xs text-alerta">vence hoje</p>
      </div>
    );
  return (
    <div className="text-center">
      <p className={`text-lg font-semibold leading-tight ${dias <= 3 ? "text-alerta" : "text-texto"}`}>{dias}</p>
      <p className="text-xs text-texto-suave">{dias === 1 ? "dia para entrega" : "dias para entrega"}</p>
    </div>
  );
}

function ignorarNaoEncontrado(e: unknown): null {
  if (e instanceof ApiError && (e.status === 404 || e.status === 400)) return null;
  throw e;
}
