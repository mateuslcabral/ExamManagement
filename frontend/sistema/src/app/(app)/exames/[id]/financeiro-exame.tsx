"use client";

import { useActionState, useState, useTransition } from "react";
import type { FinanceiroExame, MetodoPagamento, Pagamento } from "@/lib/api";
import { formatarData, formatarMoeda, ROTULOS_METODO, ROTULOS_STATUS_PAGAMENTO, valorParaCampo } from "@/lib/formatos";
import {
  aplicarDesconto,
  excluirPagamento,
  glosarPagamento,
  lancarPagamento,
  marcarRecebido,
  type EstadoFinanceiro,
} from "../acoes-financeiro";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Etiqueta } from "@/components/ui/etiqueta";
import { Selecao } from "@/components/ui/selecao";

const METODOS = Object.keys(ROTULOS_METODO) as MetodoPagamento[];

/** Financeiro do exame (docs/05-regras-negocio/financeiro.md): desconto, lançamentos e saldo. */
export function FinanceiroExame({ financeiro, hoje }: { financeiro: FinanceiroExame; hoje: string }) {
  const f = financeiro;
  return (
    <section className="rounded-2xl border border-borda bg-white p-5 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="font-semibold text-texto">Financeiro</h2>
          <p className="mt-1 text-xs text-texto-suave">
            Registro interno, sem emissão fiscal. Particular quita no ato; convênio entra no caixa quando o dinheiro chega.
          </p>
        </div>
        <Etiqueta cor={f.saldo === 0 ? "verde" : f.saldo > 0 ? "amarelo" : "vermelho"}>
          {f.saldo === 0 ? "Quitado" : f.saldo > 0 ? `Falta lançar ${formatarMoeda(f.saldo)}` : `Lançado a mais ${formatarMoeda(-f.saldo)}`}
        </Etiqueta>
      </div>

      <dl className="mt-4 grid grid-cols-2 gap-3 text-sm sm:grid-cols-5">
        <Valor rotulo="Preço" valor={f.preco} />
        <Valor rotulo="Desconto" valor={f.desconto} negativo />
        <Valor rotulo="A pagar" valor={f.valorAPagar} destaque />
        <Valor rotulo="Recebido" valor={f.recebido} />
        <Valor rotulo="A receber (convênio)" valor={f.aReceber} />
      </dl>

      <Desconto financeiro={f} />

      {f.pagamentos.length > 0 && (
        <ul className="mt-5 divide-y divide-borda/60 rounded-lg border border-borda">
          {f.pagamentos.map((p) => (
            <Lancamento key={p.id} pagamento={p} exameId={f.exameId} hoje={hoje} />
          ))}
        </ul>
      )}

      <NovoLancamento exameId={f.exameId} hoje={hoje} saldo={f.saldo} />
    </section>
  );
}

function Valor({ rotulo, valor, negativo, destaque }: { rotulo: string; valor: number; negativo?: boolean; destaque?: boolean }) {
  return (
    <div className="rounded-lg bg-fundo px-3 py-2">
      <dt className="text-xs text-texto-suave">{rotulo}</dt>
      <dd className={`font-semibold tabular-nums ${destaque ? "text-primaria" : "text-texto"}`}>
        {negativo && valor > 0 ? "− " : ""}
        {formatarMoeda(valor)}
      </dd>
    </div>
  );
}

function Desconto({ financeiro }: { financeiro: FinanceiroExame }) {
  const [aberto, setAberto] = useState(false);
  const [estado, acao, salvando] = useActionState<EstadoFinanceiro, FormData>(aplicarDesconto, {});

  if (!aberto)
    return (
      <div className="mt-3 flex flex-wrap items-center gap-3 text-sm">
        {financeiro.desconto > 0 ? (
          <p className="text-texto-suave">
            Desconto de {formatarMoeda(financeiro.desconto)}
            {financeiro.motivoDesconto && ` — ${financeiro.motivoDesconto}`}
          </p>
        ) : (
          <p className="text-texto-suave">Sem desconto.</p>
        )}
        <Botao variante="fantasma" tamanho="sm" onClick={() => setAberto(true)}>
          {financeiro.desconto > 0 ? "Alterar desconto" : "Aplicar desconto"}
        </Botao>
        {estado.sucesso && <span className="text-xs text-sucesso">{estado.sucesso}</span>}
      </div>
    );

  return (
    <form action={acao} className="mt-3 flex flex-wrap items-end gap-3 rounded-lg border border-borda p-3">
      <input type="hidden" name="exameId" value={financeiro.exameId} />
      <Campo
        label="Desconto (R$)"
        name="desconto"
        inputMode="decimal"
        defaultValue={financeiro.desconto > 0 ? valorParaCampo(financeiro.desconto) : ""}
        placeholder="0,00"
        className="w-36"
        erro={estado.campos?.desconto}
      />
      <Campo label="Motivo (opcional)" name="motivo" defaultValue={financeiro.motivoDesconto ?? ""} maxLength={300} className="min-w-60 flex-1" />
      <Botao type="submit" tamanho="sm" carregando={salvando}>
        Salvar
      </Botao>
      <Botao type="button" variante="secundaria" tamanho="sm" onClick={() => setAberto(false)}>
        Cancelar
      </Botao>
      {estado.erro && (
        <p role="alert" className="w-full text-xs text-erro">
          {estado.erro}
        </p>
      )}
    </form>
  );
}

function Lancamento({ pagamento: p, exameId, hoje }: { pagamento: Pagamento; exameId: string; hoje: string }) {
  const [modo, setModo] = useState<"nenhum" | "receber" | "glosar" | "excluir">("nenhum");
  const [texto, setTexto] = useState("");
  const [data, setData] = useState(hoje);
  const [erro, setErro] = useState<string | null>(null);
  const [pendente, iniciar] = useTransition();

  function executar(fn: () => Promise<{ ok: boolean; erro?: string }>) {
    setErro(null);
    iniciar(async () => {
      const r = await fn();
      if (!r.ok) setErro(r.erro ?? "Falha.");
      else {
        setModo("nenhum");
        setTexto("");
      }
    });
  }

  const cor = p.status === "Recebido" ? "verde" : p.status === "AReceber" ? "amarelo" : "cinza";

  return (
    <li className="px-4 py-3">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="text-sm">
          <p className="font-medium text-texto">
            {formatarMoeda(p.valor)} · {ROTULOS_METODO[p.metodo]}
            {p.convenio && ` — ${p.convenio}`}
            {p.numeroGuia && ` (guia ${p.numeroGuia})`}
            {p.parcelas && p.valorParcela && ` · ${p.parcelas}× de ${formatarMoeda(p.valorParcela)}`}
          </p>
          <p className="text-xs text-texto-suave">
            {p.status === "Recebido" && p.dataRecebimento && `Recebido em ${formatarData(p.dataRecebimento)}`}
            {p.status === "AReceber" && "Aguardando o pagamento do convênio"}
            {p.status === "Glosado" && `Glosado — ${p.motivoGlosa}`}
            {p.observacao && ` · ${p.observacao}`}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Etiqueta cor={cor}>{ROTULOS_STATUS_PAGAMENTO[p.status]}</Etiqueta>
          {p.status === "AReceber" && (
            <>
              <Botao variante="secundaria" tamanho="sm" disabled={pendente} onClick={() => setModo(modo === "receber" ? "nenhum" : "receber")}>
                Receber
              </Botao>
              <Botao variante="fantasma" tamanho="sm" disabled={pendente} onClick={() => setModo(modo === "glosar" ? "nenhum" : "glosar")}>
                Glosar
              </Botao>
            </>
          )}
          <Botao variante="fantasma" tamanho="sm" className="text-erro hover:bg-erro/10" disabled={pendente} onClick={() => setModo(modo === "excluir" ? "nenhum" : "excluir")}>
            Excluir
          </Botao>
        </div>
      </div>

      {modo === "receber" && (
        <div className="mt-3 flex flex-wrap items-end gap-3 rounded-lg bg-fundo p-3">
          <div>
            <label className="mb-1 block text-xs font-medium text-texto">Data em que o convênio pagou</label>
            <input type="date" max={hoje} value={data} onChange={(e) => setData(e.target.value)} className="rounded-lg border border-borda bg-white px-3 py-2 text-sm" />
          </div>
          <Botao tamanho="sm" carregando={pendente} onClick={() => executar(() => marcarRecebido(exameId, p.id, data))}>
            Confirmar recebimento
          </Botao>
        </div>
      )}
      {(modo === "glosar" || modo === "excluir") && (
        <div className="mt-3 flex flex-wrap items-end gap-3 rounded-lg bg-fundo p-3">
          <div className="min-w-60 flex-1">
            <label className="mb-1 block text-xs font-medium text-texto">
              {modo === "glosar" ? "Motivo da glosa" : "Motivo da exclusão (o lançamento fica guardado, fora dos totais)"}
            </label>
            <input value={texto} onChange={(e) => setTexto(e.target.value)} maxLength={500} className="w-full rounded-lg border border-borda bg-white px-3 py-2 text-sm" />
          </div>
          <Botao
            tamanho="sm"
            variante="perigo"
            carregando={pendente}
            disabled={!texto.trim()}
            onClick={() => executar(() => (modo === "glosar" ? glosarPagamento(exameId, p.id, texto) : excluirPagamento(exameId, p.id, texto)))}
          >
            {modo === "glosar" ? "Confirmar glosa" : "Confirmar exclusão"}
          </Botao>
        </div>
      )}
      {erro && (
        <p role="alert" className="mt-2 text-xs text-erro">
          {erro}
        </p>
      )}
    </li>
  );
}

function NovoLancamento({ exameId, hoje, saldo }: { exameId: string; hoje: string; saldo: number }) {
  const [aberto, setAberto] = useState(false);
  const [metodo, setMetodo] = useState<MetodoPagamento>("Pix");
  const [estado, acao, salvando] = useActionState<EstadoFinanceiro, FormData>(
    async (anterior, dados) => {
      const r = await lancarPagamento(anterior, dados);
      if (r.sucesso) setAberto(false);
      return r;
    },
    {},
  );
  const erro = estado.campos ?? {};

  if (!aberto)
    return (
      <div className="mt-4 flex flex-wrap items-center gap-3">
        <Botao variante="secundaria" onClick={() => setAberto(true)}>
          Novo lançamento
        </Botao>
        {estado.sucesso && <span className="text-xs text-sucesso">{estado.sucesso}</span>}
      </div>
    );

  return (
    <form action={acao} className="mt-4 space-y-3 rounded-lg border border-primaria/30 p-4">
      <input type="hidden" name="exameId" value={exameId} />
      <div className="grid gap-3 sm:grid-cols-3">
        <Selecao label="Método" name="metodo" value={metodo} onChange={(e) => setMetodo(e.target.value as MetodoPagamento)} erro={erro.metodo}>
          {METODOS.map((m) => (
            <option key={m} value={m}>
              {ROTULOS_METODO[m]}
            </option>
          ))}
        </Selecao>
        <Campo
          label="Valor (R$)"
          name="valor"
          inputMode="decimal"
          required
          defaultValue={saldo > 0 ? valorParaCampo(saldo) : ""}
          placeholder="0,00"
          erro={erro.valor}
        />
        {metodo === "Convenio" ? (
          <Campo label="Convênio" name="convenio" required placeholder="Ex.: Unimed" maxLength={150} erro={erro.convenio} dica="Entra como a receber." />
        ) : (
          <Campo label="Data do recebimento" name="dataRecebimento" type="date" max={hoje} defaultValue={hoje} erro={erro.dataRecebimento} />
        )}
        {metodo === "Convenio" && <Campo label="Guia / autorização (opcional)" name="numeroGuia" maxLength={50} />}
        {metodo === "CartaoCredito" && (
          <Campo label="Parcelas (opcional)" name="parcelas" type="number" min={2} max={24} placeholder="à vista" erro={erro.parcelas} dica="Só informação: o valor entra integral na data." />
        )}
        <Campo label="Observação (opcional)" name="observacao" maxLength={300} className={metodo === "Convenio" || metodo === "CartaoCredito" ? "" : "sm:col-span-1"} />
      </div>
      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}
      <div className="flex gap-3">
        <Botao type="button" variante="secundaria" tamanho="sm" onClick={() => setAberto(false)}>
          Cancelar
        </Botao>
        <Botao type="submit" tamanho="sm" carregando={salvando}>
          Registrar lançamento
        </Botao>
      </div>
    </form>
  );
}
