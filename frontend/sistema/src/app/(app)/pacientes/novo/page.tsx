import Link from "next/link";
import { FormularioPaciente } from "../formulario-paciente";

export const metadata = { title: "Novo paciente — Cligen" };

export default function PaginaNovoPaciente() {
  return (
    <div className="mx-auto max-w-3xl">
      <Link href="/pacientes" className="text-sm text-texto-suave hover:text-texto">
        ← Pacientes
      </Link>
      <h1 className="mt-2 text-2xl font-semibold text-primaria">Novo paciente</h1>
      <p className="mt-1 text-sm text-texto-suave">
        Ao cadastrar, o paciente recebe automaticamente as boas-vindas com as instruções de acesso ao portal.
      </p>

      <section className="mt-6 rounded-2xl border border-borda bg-white p-6 shadow-sm">
        <FormularioPaciente />
      </section>
    </div>
  );
}
