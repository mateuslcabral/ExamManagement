import Link from "next/link";
import { api, type PacienteResumo } from "@/lib/api";
import { formatarData, formatarTelefone } from "@/lib/idade";
import { Etiqueta } from "@/components/ui/etiqueta";

export const metadata = { title: "Pacientes — Cligen" };

type Busca = { busca?: string; excluidos?: string };

export default async function PaginaPacientes({ searchParams }: { searchParams: Promise<Busca> }) {
  const { busca = "", excluidos } = await searchParams;
  const incluirExcluidos = excluidos === "1";

  const qs = new URLSearchParams();
  if (busca) qs.set("busca", busca);
  if (incluirExcluidos) qs.set("incluirExcluidos", "true");
  const pacientes = await api<PacienteResumo[]>(`/api/pacientes?${qs}`);

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">Pacientes</h1>
          <p className="mt-1 text-sm text-texto-suave">
            Cadastro único por paciente: os exames se vinculam ao mesmo registro, sem recadastro.
          </p>
        </div>
        <Link
          href="/pacientes/novo"
          className="inline-flex items-center gap-2 rounded-pill bg-primaria px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-primaria-clara"
        >
          + Novo paciente
        </Link>
      </div>

      <form className="mt-6 flex flex-wrap items-center gap-3" role="search">
        <input
          type="search"
          name="busca"
          defaultValue={busca}
          placeholder="Buscar por nome, documento, e-mail ou telefone"
          aria-label="Buscar pacientes"
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
          {pacientes.length} {pacientes.length === 1 ? "paciente" : "pacientes"}
        </span>
      </form>

      <section className="mt-4 overflow-x-auto rounded-2xl border border-borda bg-white shadow-sm">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
              <th className="px-4 py-3 font-semibold">Paciente</th>
              <th className="px-4 py-3 font-semibold">Documento</th>
              <th className="px-4 py-3 font-semibold">Nascimento</th>
              <th className="px-4 py-3 font-semibold">Contato</th>
              <th className="px-4 py-3 font-semibold">Situação</th>
            </tr>
          </thead>
          <tbody>
            {pacientes.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-10 text-center text-texto-suave">
                  {busca ? "Nenhum paciente encontrado para essa busca." : "Nenhum paciente cadastrado ainda."}
                </td>
              </tr>
            )}
            {pacientes.map((p) => (
              <tr key={p.id} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
                <td className="px-4 py-3">
                  <Link href={`/pacientes/${p.id}`} className="font-medium text-primaria hover:underline">
                    {p.nome}
                  </Link>
                </td>
                <td className="px-4 py-3 text-texto">
                  <span className="text-xs text-texto-suave">{p.tipoDocumento === "Cpf" ? "CPF" : "Passaporte"}</span>{" "}
                  {p.numeroDocumentoFormatado}
                </td>
                <td className="px-4 py-3 text-texto">
                  {formatarData(p.dataNascimento)}
                  <span className="ml-1 text-xs text-texto-suave">({p.idade} anos)</span>
                  {p.menorDeIdade && (
                    <Etiqueta cor="amarelo" className="ml-2">
                      Menor
                    </Etiqueta>
                  )}
                </td>
                <td className="px-4 py-3">
                  <p className="text-texto">{p.email}</p>
                  <p className="text-xs text-texto-suave">{formatarTelefone(p.telefone)}</p>
                </td>
                <td className="px-4 py-3">
                  {p.excluido ? <Etiqueta cor="vermelho">Excluído</Etiqueta> : <Etiqueta cor="verde">Ativo</Etiqueta>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}
