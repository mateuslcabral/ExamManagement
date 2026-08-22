import Link from "next/link";
import { api, type ExameCatalogo, type Paciente, type PacienteResumo } from "@/lib/api";
import { FormularioExame } from "../formulario-exame";

export const metadata = { title: "Novo exame — Cligen" };

export default async function PaginaNovoExame({ searchParams }: { searchParams: Promise<{ paciente?: string }> }) {
  const { paciente: pacienteId } = await searchParams;

  const [catalogo, pacientes, pacienteFixo] = await Promise.all([
    api<ExameCatalogo[]>("/api/catalogo-exames?somenteAtivos=true"),
    pacienteId ? Promise.resolve([]) : api<PacienteResumo[]>("/api/pacientes"),
    pacienteId ? api<Paciente>(`/api/pacientes/${pacienteId}`) : Promise.resolve(null),
  ]);

  return (
    <div className="mx-auto max-w-3xl">
      <Link href={pacienteFixo ? `/pacientes/${pacienteFixo.id}` : "/exames"} className="text-sm text-texto-suave hover:text-texto">
        ← {pacienteFixo ? pacienteFixo.nome : "Exames"}
      </Link>
      <h1 className="mt-2 text-2xl font-semibold text-primaria">Novo exame</h1>
      <p className="mt-1 text-sm text-texto-suave">
        O exame nasce aguardando amostra. Prazo e preço vêm do catálogo; o preço pode ser ajustado.
      </p>

      <section className="mt-6 rounded-2xl border border-borda bg-white p-6 shadow-sm">
        {catalogo.length === 0 ? (
          <p className="text-sm text-texto-suave">
            Nenhum exame ativo no catálogo.{" "}
            <Link href="/catalogo" className="text-primaria underline">
              Cadastre um exame no catálogo
            </Link>{" "}
            antes de continuar.
          </p>
        ) : (
          <FormularioExame
            catalogo={catalogo}
            pacientes={pacientes}
            pacienteFixo={pacienteFixo ? { id: pacienteFixo.id, nome: pacienteFixo.nome } : undefined}
          />
        )}
      </section>
    </div>
  );
}
