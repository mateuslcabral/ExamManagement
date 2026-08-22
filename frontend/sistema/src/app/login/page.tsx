import Image from "next/image";
import { redirect } from "next/navigation";
import { auth } from "@/auth";
import { FormularioLogin } from "./formulario-login";

export const metadata = { title: "Entrar — Cligen" };

export default async function PaginaLogin({
  searchParams,
}: {
  searchParams: Promise<{ callbackUrl?: string; error?: string }>;
}) {
  const sessao = await auth();
  if (sessao?.user) redirect("/");

  const { callbackUrl, error } = await searchParams;

  return (
    <main className="fundo-hero flex min-h-screen items-center justify-center p-6">
      <div className="w-full max-w-md">
        <div className="mb-8 flex justify-center">
          <Image src="/logo-cligen-branca.png" alt="Cligen" width={216} height={78} priority />
        </div>

        <section className="rounded-2xl bg-white p-8 shadow-2xl shadow-black/40">
          <h1 className="text-2xl font-semibold text-primaria">Sistema interno</h1>
          <p className="mt-1 text-sm text-texto-suave">Acesso restrito à equipe Cligen.</p>

          <FormularioLogin callbackUrl={callbackUrl} erroInicial={error} />
        </section>

        <p className="mt-6 text-center text-xs text-white/60">
          Sua avaliação genética completa, onde você estiver.
        </p>
      </div>
    </main>
  );
}
