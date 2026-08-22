import Image from "next/image";
import Link from "next/link";
import { FormularioDefinirSenha } from "./formulario";

export const metadata = { title: "Definir senha — Cligen" };

export default async function PaginaDefinirSenha({ searchParams }: { searchParams: Promise<{ token?: string }> }) {
  const { token } = await searchParams;

  return (
    <main className="fundo-hero flex min-h-screen items-center justify-center p-6">
      <div className="w-full max-w-md">
        <div className="mb-8 flex justify-center">
          <Image src="/logo-cligen-branca.png" alt="Cligen" width={216} height={78} priority />
        </div>

        <section className="rounded-2xl bg-white p-8 shadow-2xl shadow-black/40">
          <h1 className="text-2xl font-semibold text-primaria">Defina sua senha</h1>
          <p className="mt-1 text-sm text-texto-suave">
            Escolha a senha que você usará para entrar no sistema interno.
          </p>

          {token ? (
            <FormularioDefinirSenha token={token} />
          ) : (
            <p role="alert" className="mt-6 rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
              Link inválido. Use o link recebido por e-mail ou peça um novo à equipe.
            </p>
          )}

          <p className="mt-6 text-center text-xs text-texto-suave">
            <Link href="/login" className="font-medium text-teal hover:underline">
              Voltar para o login
            </Link>
          </p>
        </section>
      </div>
    </main>
  );
}
