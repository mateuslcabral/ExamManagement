import Link from "next/link";
import { auth } from "@/auth";
import { MENU } from "@/components/layout/menu";
import { Icone } from "@/components/layout/icones";

export default async function PaginaInicial() {
  const sessao = await auth();
  const primeiroNome = sessao?.user?.name?.split(" ")[0] ?? "";

  return (
    <div className="mx-auto max-w-5xl">
      <h1 className="text-2xl font-semibold text-primaria">Olá, {primeiroNome}</h1>
      <p className="mt-1 text-sm text-texto-suave">Plataforma de gestão de exames e pacientes da Cligen.</p>

      <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {MENU.filter((i) => i.icone !== "inicio").map((item) =>
          item.href ? (
            <Link
              key={item.rotulo}
              href={item.href}
              className="group rounded-2xl border border-borda bg-white p-5 shadow-sm transition hover:-translate-y-0.5 hover:border-primaria hover:shadow-md"
            >
              <div className="flex size-11 items-center justify-center rounded-xl bg-primaria/10 text-primaria transition group-hover:bg-primaria group-hover:text-white">
                <Icone nome={item.icone} className="size-6" />
              </div>
              <h2 className="mt-4 font-semibold text-texto">{item.rotulo}</h2>
              <p className="mt-1 text-xs text-texto-suave">Acessar</p>
            </Link>
          ) : (
            <div key={item.rotulo} className="rounded-2xl border border-dashed border-borda bg-white/60 p-5 opacity-70">
              <div className="flex size-11 items-center justify-center rounded-xl bg-fundo-alt text-texto-suave">
                <Icone nome={item.icone} className="size-6" />
              </div>
              <h2 className="mt-4 font-semibold text-texto-suave">{item.rotulo}</h2>
              <p className="mt-1 text-xs text-texto-suave">Em breve</p>
            </div>
          ),
        )}
      </div>
    </div>
  );
}
