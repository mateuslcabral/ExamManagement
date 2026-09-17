import Link from "next/link";
import { api, ApiError, type ExameCatalogo, type Paciente } from "@/lib/api";
import type { PacienteEncontrado } from "../actions";
import { FormularioExame } from "../formulario-exame";

export const metadata = { title: "Novo exame — Cligen" };

export default async function PaginaNovoExame({ searchParams }: { searchParams: Promise<{ pacienteId?: string }> }) {
  const { pacienteId } = await searchParams;

  const [catalogo, paciente] = await Promise.all([
    api<ExameCatalogo[]>("/api/catalogo-exames?apenasAtivos=true"),
    pacienteId
      ? api<Paciente>(`/api/pacientes/${encodeURIComponent(pacienteId)}`).catch((e) => {
          if (e instanceof ApiError && (e.status === 404 || e.status === 400)) return null;
          throw e;
        })
      : null,
  ]);

  const pacienteInicial: PacienteEncontrado | undefined = paciente
    ? {
        id: paciente.id,
        nome: paciente.nome,
        tipoDocumento: paciente.tipoDocumento,
        numeroDocumento: paciente.numeroDocumento,
        dataNascimento: paciente.dataNascimento,
      }
    : undefined;

  return (
    <div className="mx-auto max-w-3xl">
      <Link href="/exames" className="text-sm text-teal hover:underline">
        ← Exames
      </Link>
      <h1 className="mt-2 text-2xl font-semibold text-primaria">Novo exame</h1>
      <p className="mt-1 mb-6 text-sm text-texto-suave">
        O exame nasce aguardando amostra. Os anexos são enviados na tela seguinte.
      </p>

      {catalogo.length === 0 ? (
        <p className="rounded-lg bg-alerta/10 px-4 py-3 text-sm text-alerta">
          Nenhum exame ativo no catálogo.{" "}
          <Link href="/catalogo" className="font-semibold underline">
            Cadastre um exame no catálogo
          </Link>{" "}
          antes de registrar a solicitação.
        </p>
      ) : (
        <FormularioExame catalogo={catalogo} pacienteInicial={pacienteInicial} />
      )}
    </div>
  );
}
