import Image from "next/image";
import Link from "next/link";
import { redirect } from "next/navigation";
import { auth, signOut } from "@/auth";
import { Sidebar } from "@/components/layout/sidebar";

export default async function LayoutApp({ children }: { children: React.ReactNode }) {
  const sessao = await auth();
  if (!sessao?.user) redirect("/login");

  const iniciais = (sessao.user.name ?? "?")
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase())
    .join("");

  return (
    <div className="flex min-h-screen">
      <aside className="hidden w-64 shrink-0 flex-col bg-primaria text-white md:flex">
        <div className="flex h-16 items-center border-b border-white/10 px-5">
          <Link href="/" aria-label="Início">
            <Image src="/logo-cligen-branca.png" alt="Cligen" width={120} height={43} priority />
          </Link>
        </div>
        <div className="flex-1 overflow-y-auto">
          <Sidebar />
        </div>
        <div className="border-t border-white/10 p-4 text-[11px] text-white/40">Cligen · Sistema interno</div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex h-16 items-center justify-between border-b border-borda bg-white px-6">
          <div className="flex items-center gap-3 md:hidden">
            <Image src="/logo-cligen-branca.png" alt="Cligen" width={100} height={36} className="rounded bg-primaria p-1" />
          </div>
          <div className="hidden md:block" />

          <div className="flex items-center gap-4">
            <div className="text-right">
              <p className="text-sm font-semibold text-texto">{sessao.user.name}</p>
              <p className="text-xs text-texto-suave">{sessao.user.email}</p>
            </div>
            <div
              className="flex size-10 items-center justify-center rounded-full bg-primaria text-sm font-bold text-white"
              aria-hidden
            >
              {iniciais}
            </div>
            <form
              action={async () => {
                "use server";
                await signOut({ redirectTo: "/login" });
              }}
            >
              <button
                type="submit"
                className="rounded-pill border border-borda px-3 py-1.5 text-xs font-semibold text-primaria transition hover:bg-fundo-alt"
              >
                Sair
              </button>
            </form>
          </div>
        </header>

        <main className="fundo-ondas flex-1 p-6 md:p-8">{children}</main>
      </div>
    </div>
  );
}
