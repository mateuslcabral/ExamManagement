import Link from "next/link";
import { hojeEmBrasilia } from "@/lib/formatos";
import { FormularioPaciente } from "../formulario-paciente";

export const metadata = { title: "Novo paciente — Cligen" };

export default function PaginaNovoPaciente() {
  return (
    <div className="mx-auto max-w-3xl">
      <Link href="/pacientes" className="text-sm text-teal hover:underline">
        ← Pacientes
      </Link>
      <h1 className="mt-2 text-2xl font-semibold text-primaria">Novo paciente</h1>
      <p className="mt-1 mb-6 text-sm text-texto-suave">
        Ao cadastrar, o sistema envia a mensagem de boas-vindas com as instruções de acesso ao portal.
      </p>
      <FormularioPaciente hoje={hojeEmBrasilia()} />
    </div>
  );
}
