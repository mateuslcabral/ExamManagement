import Link from "next/link";
import { api, type Cadastro, type TipoCadastro } from "@/lib/api";
import { TabelaCadastro } from "./tabela-cadastro";

export const metadata = { title: "Cadastros auxiliares — Cligen" };

const ABAS: { tipo: TipoCadastro; rotulo: string; singular: string; descricao: string }[] = [
  { tipo: "origens", rotulo: "Origens", singular: "origem", descricao: "Por onde o exame entrou (Cligen, clínica parceira, site, plataforma…). Aparece como lista no exame." },
  { tipo: "destinos", rotulo: "Destinos", singular: "destino", descricao: "Laboratório ou unidade que executa o exame. Pode ser o padrão de um item do catálogo." },
  { tipo: "convenios", rotulo: "Convênios", singular: "operadora", descricao: "Operadoras de convênio. O relatório financeiro soma o que cada uma tem a pagar." },
];

export default async function PaginaCadastros({ searchParams }: { searchParams: Promise<{ aba?: string }> }) {
  const { aba } = await searchParams;
  const atual = ABAS.find((a) => a.tipo === aba) ?? ABAS[0];
  const itens = await api<Cadastro[]>(`/api/cadastros/${atual.tipo}`);

  return (
    <div className="mx-auto max-w-6xl">
      <h1 className="text-2xl font-semibold text-primaria">Cadastros auxiliares</h1>
      <p className="mt-1 text-sm text-texto-suave">
        Listas usadas nos formulários. Item em uso nunca é apagado: desativar o tira das listas e preserva o histórico.
      </p>

      <nav className="mt-6 flex gap-2 border-b border-borda" aria-label="Cadastros">
        {ABAS.map((a) => (
          <Link
            key={a.tipo}
            href={`/cadastros?aba=${a.tipo}`}
            aria-current={a.tipo === atual.tipo ? "page" : undefined}
            className={`-mb-px border-b-2 px-4 py-2 text-sm font-semibold transition ${
              a.tipo === atual.tipo ? "border-primaria text-primaria" : "border-transparent text-texto-suave hover:text-texto"
            }`}
          >
            {a.rotulo}
          </Link>
        ))}
      </nav>

      <div className="mt-6">
        <TabelaCadastro key={atual.tipo} tipo={atual.tipo} itens={itens} rotuloSingular={atual.singular} descricao={atual.descricao} />
      </div>
    </div>
  );
}
