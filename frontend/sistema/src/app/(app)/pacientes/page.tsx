import Link from "next/link";
import { api, type Pagina, type Paciente } from "@/lib/api";
import { formatarData, formatarDocumento, formatarTelefone, rotuloDocumento } from "@/lib/formatos";
import { Etiqueta } from "@/components/ui/etiqueta";

export const metadata = { title: "Pacientes — Cligen" };

const TAMANHO_PAGINA = 20;

const mensagensSalvo: Record<string, string> = {
  criado: "Paciente cadastrado. A mensagem de boas-vindas foi disparada para o e-mail e o WhatsApp informados.",
  editado: "Cadastro atualizado.",
};

export default async function PaginaPacientes({
  searchParams,
}: {
  searchParams: Promise<{ busca?: string; pagina?: string; salvo?: string }>;
}) {
  const { busca = "", pagina: paginaParam, salvo } = await searchParams;
  const pagina = Math.max(1, Number(paginaParam) || 1);

  const consulta = new URLSearchParams({ pagina: String(pagina), tamanhoPagina: String(TAMANHO_PAGINA) });
  if (busca.trim()) consulta.set("busca", busca.trim());
  const resultado = await api<Pagina<Paciente>>(`/api/pacientes?${consulta}`);
  const totalPaginas = Math.max(1, Math.ceil(resultado.total / TAMANHO_PAGINA));

  const linkPagina = (p: number) => {
    const q = new URLSearchParams({ pagina: String(p) });
    if (busca.trim()) q.set("busca", busca.trim());
    return `/pacientes?${q}`;
  };

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">Pacientes</h1>
          <p className="mt-1 text-sm text-texto-suave">
            Cadastro único: exames futuros se vinculam ao mesmo paciente, sem recadastro.
          </p>
        </div>
        <Link
          href="/pacientes/novo"
          className="inline-flex items-center rounded-pill bg-primaria px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-primaria-clara"
        >
          Novo paciente
        </Link>
      </div>

      {salvo && mensagensSalvo[salvo] && (
        <p role="status" className="mt-6 rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
          {mensagensSalvo[salvo]}
        </p>
      )}

      <section className="mt-6 rounded-2xl border border-borda bg-white shadow-sm">
        <form action="/pacientes" className="flex flex-wrap items-center gap-3 border-b border-borda p-4">
          <input
            type="search"
            name="busca"
            defaultValue={busca}
            placeholder="Buscar por nome, CPF ou passaporte"
            aria-label="Buscar pacientes"
            className="min-w-60 flex-1 rounded-lg border border-borda bg-white px-3 py-2.5 text-sm outline-none transition focus:border-primaria focus:ring-2 focus:ring-primaria/40"
          />
          <button
            type="submit"
            className="rounded-pill border border-primaria px-5 py-2.5 text-sm font-semibold text-primaria transition hover:bg-fundo-alt"
          >
            Buscar
          </button>
          {busca && (
            <Link href="/pacientes" className="text-sm text-teal hover:underline">
              Limpar
            </Link>
          )}
          <span className="ml-auto text-xs text-texto-suave">
            {resultado.total} {resultado.total === 1 ? "paciente" : "pacientes"}
          </span>
        </form>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
                <th className="px-4 py-3 font-semibold">Paciente</th>
                <th className="px-4 py-3 font-semibold">Documento</th>
                <th className="px-4 py-3 font-semibold">Contato</th>
                <th className="px-4 py-3 text-right font-semibold">Ações</th>
              </tr>
            </thead>
            <tbody>
              {resultado.itens.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-8 text-center text-texto-suave">
                    {busca ? "Nenhum paciente encontrado para essa busca." : "Nenhum paciente cadastrado."}
                  </td>
                </tr>
              )}
              {resultado.itens.map((p) => (
                <tr key={p.id} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
                  <td className="px-4 py-3">
                    <p className="font-medium text-texto">
                      {p.nome}
                      {p.menorDeIdade && (
                        <span className="ml-2 align-middle">
                          <Etiqueta cor="amarelo">Menor</Etiqueta>
                        </span>
                      )}
                    </p>
                    <p className="text-xs text-texto-suave">Nascimento: {formatarData(p.dataNascimento)}</p>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap">
                    <p>{formatarDocumento(p.tipoDocumento, p.numeroDocumento)}</p>
                    <p className="text-xs text-texto-suave">{rotuloDocumento(p.tipoDocumento)}</p>
                  </td>
                  <td className="px-4 py-3">
                    <p>{p.email}</p>
                    <p className="text-xs text-texto-suave">
                      {formatarTelefone(p.telefone)}
                      {p.responsavelLegal && ` · de ${p.responsavelLegal.nome}`}
                    </p>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-1 whitespace-nowrap">
                      {[
                        { href: `/exames?pacienteId=${p.id}`, rotulo: "Exames" },
                        { href: `/exames/novo?pacienteId=${p.id}`, rotulo: "Novo exame" },
                        { href: `/pacientes/${p.id}`, rotulo: "Editar" },
                      ].map((l) => (
                        <Link
                          key={l.rotulo}
                          href={l.href}
                          className="rounded-pill px-3 py-1.5 text-xs font-semibold text-primaria transition hover:bg-primaria/10"
                        >
                          {l.rotulo}
                        </Link>
                      ))}
                    </div>
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
