import { auth } from "@/auth";
import { api, type Usuario } from "@/lib/api";
import { FormularioNovoUsuario } from "./formulario-novo-usuario";
import { TabelaUsuarios } from "./tabela-usuarios";

export const metadata = { title: "Gestão de Usuários — Cligen" };

export default async function PaginaUsuarios() {
  const [sessao, usuarios] = await Promise.all([auth(), api<Usuario[]>("/api/usuarios")]);

  return (
    <div className="mx-auto max-w-6xl">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">Gestão de Usuários</h1>
          <p className="mt-1 text-sm text-texto-suave">
            Equipe com acesso ao sistema interno. Não há autocadastro: toda conta nasce aqui.
          </p>
        </div>
        <p className="text-xs text-texto-suave">
          {usuarios.length} {usuarios.length === 1 ? "usuário" : "usuários"}
        </p>
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-[360px_1fr]">
        <section className="h-fit rounded-2xl border border-borda bg-white p-5 shadow-sm">
          <h2 className="font-semibold text-texto">Novo usuário</h2>
          <p className="mt-1 text-xs text-texto-suave">
            E-mail <span className="font-medium">@gmail.com</span> entra com Google e já fica ativo. Qualquer outro
            e-mail recebe um link para definir a própria senha.
          </p>
          <FormularioNovoUsuario />
        </section>

        <section className="rounded-2xl border border-borda bg-white shadow-sm">
          <TabelaUsuarios usuarios={usuarios} meuId={sessao?.user?.id} />
        </section>
      </div>
    </div>
  );
}
