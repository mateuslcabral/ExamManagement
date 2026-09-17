import Link from "next/link";
import { notFound } from "next/navigation";
import { api, ApiError, type Paciente } from "@/lib/api";
import { formatarDocumento, hojeEmBrasilia, rotuloDocumento } from "@/lib/formatos";
import { FormularioPaciente } from "../formulario-paciente";

export const metadata = { title: "Editar paciente — Cligen" };

export default async function PaginaEditarPaciente({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;

  let paciente: Paciente;
  try {
    paciente = await api<Paciente>(`/api/pacientes/${encodeURIComponent(id)}`);
  } catch (e) {
    if (e instanceof ApiError && (e.status === 404 || e.status === 400)) notFound();
    throw e;
  }

  return (
    <div className="mx-auto max-w-3xl">
      <Link href="/pacientes" className="text-sm text-teal hover:underline">
        ← Pacientes
      </Link>
      <h1 className="mt-2 text-2xl font-semibold text-primaria">{paciente.nome}</h1>
      <p className="mt-1 mb-6 text-sm text-texto-suave">
        {rotuloDocumento(paciente.tipoDocumento)} {formatarDocumento(paciente.tipoDocumento, paciente.numeroDocumento)}.
        Alterar o documento muda o login do paciente no portal.
      </p>
      <FormularioPaciente paciente={paciente} hoje={hojeEmBrasilia()} />
    </div>
  );
}
