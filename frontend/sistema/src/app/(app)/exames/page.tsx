import { redirect } from "next/navigation";

/** A lista de exames foi absorvida por Fluxos (20/10/2026). Links antigos continuam funcionando com os mesmos filtros. */
export default async function PaginaExames({ searchParams }: { searchParams: Promise<Record<string, string | undefined>> }) {
  const params = await searchParams;
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) if (v) q.set(k, v);
  const sufixo = q.toString();
  redirect(sufixo ? `/fluxos?${sufixo}` : "/fluxos");
}
