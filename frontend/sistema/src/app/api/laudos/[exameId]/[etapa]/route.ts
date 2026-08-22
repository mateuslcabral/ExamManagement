import { auth } from "@/auth";
import { apiBruta } from "@/lib/api";

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const ETAPAS = new Set(["LaudoParceiroPronto", "LaudoCligenParaRevisao", "LaudoRevisado"]);

/** Download do PDF de uma etapa do laudo via BFF (mesmo padrão de /api/anexos). */
export async function GET(_req: Request, ctx: { params: Promise<{ exameId: string; etapa: string }> }) {
  const sessao = await auth();
  if (!sessao?.user) return new Response("Não autenticado.", { status: 401 });

  const { exameId, etapa } = await ctx.params;
  if (!GUID.test(exameId) || !ETAPAS.has(etapa)) return new Response("Parâmetros inválidos.", { status: 400 });

  const res = await apiBruta(`/api/exames/${exameId}/laudo/${etapa}/arquivo`);
  if (!res.ok) return new Response("Arquivo não encontrado.", { status: res.status === 404 ? 404 : 502 });

  const headers = new Headers();
  for (const h of ["content-type", "content-disposition", "content-length"]) {
    const v = res.headers.get(h);
    if (v) headers.set(h, v);
  }
  headers.set("cache-control", "private, no-store");
  return new Response(res.body, { status: 200, headers });
}
