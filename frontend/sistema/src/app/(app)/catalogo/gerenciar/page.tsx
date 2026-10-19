import Link from "next/link";
import { api, type Cadastro, type ExameCatalogo } from "@/lib/api";
import { CatalogoExames } from "../catalogo-exames";

export const metadata = { title: "Gerenciar catálogo — Cligen" };

export default async function PaginaGerenciarCatalogo() {
  const [exames, destinos] = await Promise.all([api<ExameCatalogo[]>("/api/catalogo-exames"), api<Cadastro[]>("/api/cadastros/destinos")]);

  return (
    <div className="mx-auto max-w-6xl">
      <Link href="/catalogo" className="text-sm text-teal hover:underline">
        ← Catálogo
      </Link>
      <div className="mt-2 flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-primaria">Gerenciar catálogo</h1>
          <p className="mt-1 text-sm text-texto-suave">
            Exame aposentado é desativado, nunca apagado, para preservar o histórico. Destinos vêm dos{" "}
            <Link href="/cadastros?aba=destinos" className="text-teal hover:underline">
              cadastros auxiliares
            </Link>
            .
          </p>
        </div>
      </div>
      <CatalogoExames exames={exames} destinos={destinos} />
    </div>
  );
}
