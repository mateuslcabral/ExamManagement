import Link from "next/link";
import { api, type LinhaFinanceira, type RelatorioFinanceiro } from "@/lib/api";
import { formatarData, formatarDocumento, formatarMoeda, hojeEmBrasilia, ROTULOS_METODO } from "@/lib/formatos";

export const metadata = { title: "Financeiro — Cligen" };

/** Relatório por regime de caixa (Q29): só o que foi efetivamente recebido, pela data de recebimento. */
export default async function PaginaFinanceiro({ searchParams }: { searchParams: Promise<{ de?: string; ate?: string }> }) {
  const { de, ate } = await searchParams;
  const hoje = hojeEmBrasilia();
  const q = new URLSearchParams();
  if (de && /^\d{4}-\d{2}-\d{2}$/.test(de)) q.set("de", de);
  if (ate && /^\d{4}-\d{2}-\d{2}$/.test(ate)) q.set("ate", ate);

  const r = await api<RelatorioFinanceiro>(`/api/financeiro/relatorio?${q}`);

  return (
    <div className="mx-auto max-w-6xl">
      <h1 className="text-2xl font-semibold text-primaria">Financeiro</h1>
      <p className="mt-1 text-sm text-texto-suave">
        Regime de caixa: entra o que foi recebido, pela data do recebimento. Convênio só conta quando o dinheiro chega.
        Lançamentos são feitos na tela de cada exame.
      </p>

      <form action="/financeiro" className="mt-6 flex flex-wrap items-end gap-3 rounded-2xl border border-borda bg-white p-4 shadow-sm">
        <div>
          <label htmlFor="de" className="mb-1 block text-sm font-medium text-texto">
            De
          </label>
          <input id="de" type="date" name="de" defaultValue={r.de} max={hoje} className="rounded-lg border border-borda px-3 py-2 text-sm" />
        </div>
        <div>
          <label htmlFor="ate" className="mb-1 block text-sm font-medium text-texto">
            Até
          </label>
          <input id="ate" type="date" name="ate" defaultValue={r.ate} max={hoje} className="rounded-lg border border-borda px-3 py-2 text-sm" />
        </div>
        <button type="submit" className="rounded-pill border border-primaria px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt">
          Aplicar período
        </button>
        <span className="ml-auto text-xs text-texto-suave">
          Período: {formatarData(r.de)} a {formatarData(r.ate)}
        </span>
      </form>

      <div className="mt-6 grid gap-6 lg:grid-cols-[1fr_320px]">
        <section className="rounded-2xl border border-borda bg-white shadow-sm">
          <div className="flex flex-wrap items-center justify-between gap-3 border-b border-borda px-4 py-3">
            <h2 className="font-semibold text-texto">Recebido no período</h2>
            <p className="text-lg font-semibold text-primaria tabular-nums">{formatarMoeda(r.totalRecebido)}</p>
          </div>
          <Tabela linhas={r.recebidos} vazio="Nenhum recebimento no período." mostrarData />
        </section>

        <aside className="space-y-6">
          <Totais titulo="Por método" itens={r.recebidoPorMetodo.map((t) => ({ ...t, chave: ROTULOS_METODO[t.chave as keyof typeof ROTULOS_METODO] ?? t.chave }))} />
        </aside>
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-[1fr_320px]">
        <section className="rounded-2xl border border-borda bg-white shadow-sm">
          <div className="flex flex-wrap items-center justify-between gap-3 border-b border-borda px-4 py-3">
            <div>
              <h2 className="font-semibold text-texto">A receber de convênio</h2>
              <p className="text-xs text-texto-suave">Tudo que está pendente, sem corte de data. Glosas não entram.</p>
            </div>
            <p className="text-lg font-semibold text-alerta tabular-nums">{formatarMoeda(r.totalAReceber)}</p>
          </div>
          <Tabela linhas={r.aReceber} vazio="Nada a receber de convênio." />
        </section>
        <aside>
          <Totais titulo="Por convênio" itens={r.aReceberPorConvenio} />
        </aside>
      </div>
    </div>
  );
}

function Tabela({ linhas, vazio, mostrarData }: { linhas: LinhaFinanceira[]; vazio: string; mostrarData?: boolean }) {
  if (linhas.length === 0) return <p className="px-4 py-6 text-sm text-texto-suave">{vazio}</p>;
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
            {mostrarData ? <th className="px-4 py-2 font-semibold">Recebido em</th> : <th className="px-4 py-2 font-semibold">Entrada</th>}
            <th className="px-4 py-2 font-semibold">Paciente</th>
            <th className="px-4 py-2 font-semibold">Exame</th>
            <th className="px-4 py-2 font-semibold">{mostrarData ? "Método" : "Convênio"}</th>
            <th className="px-4 py-2 text-right font-semibold">Valor</th>
          </tr>
        </thead>
        <tbody>
          {linhas.map((l) => (
            <tr key={l.pagamentoId} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
              <td className="px-4 py-2 whitespace-nowrap">{formatarData(mostrarData ? (l.dataRecebimento ?? l.dataEntradaExame) : l.dataEntradaExame)}</td>
              <td className="px-4 py-2">
                <p className="text-texto">{l.pacienteNome}</p>
                <p className="text-xs text-texto-suave">{formatarDocumento(l.pacienteTipoDocumento, l.pacienteNumeroDocumento)}</p>
              </td>
              <td className="px-4 py-2">
                <Link href={`/exames/${l.exameId}`} className="text-teal hover:underline">
                  {l.exameNome}
                </Link>
              </td>
              <td className="px-4 py-2">
                {mostrarData ? ROTULOS_METODO[l.metodo] : l.convenio}
                {l.convenio && mostrarData && ` — ${l.convenio}`}
                {l.numeroGuia && <span className="text-xs text-texto-suave"> (guia {l.numeroGuia})</span>}
              </td>
              <td className="px-4 py-2 text-right tabular-nums">{formatarMoeda(l.valor)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function Totais({ titulo, itens }: { titulo: string; itens: { chave: string; total: number; quantidade: number }[] }) {
  return (
    <section className="rounded-2xl border border-borda bg-white p-4 shadow-sm">
      <h3 className="text-xs font-semibold uppercase tracking-wider text-texto-suave">{titulo}</h3>
      {itens.length === 0 ? (
        <p className="mt-2 text-sm text-texto-suave">—</p>
      ) : (
        <ul className="mt-2 space-y-1.5 text-sm">
          {itens.map((t) => (
            <li key={t.chave} className="flex justify-between gap-3">
              <span className="text-texto">
                {t.chave} <span className="text-xs text-texto-suave">({t.quantidade})</span>
              </span>
              <span className="font-semibold tabular-nums">{formatarMoeda(t.total)}</span>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
