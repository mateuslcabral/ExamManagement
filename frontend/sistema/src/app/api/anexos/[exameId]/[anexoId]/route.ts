import { auth } from "@/auth";
import { apiBruta } from "@/lib/api";

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/**
 * Download de anexo via BFF: o navegador nunca fala com a API nem vê a chave de serviço.
 * A rota fica sob o proxy de autenticação (proxy.ts), mas a sessão é conferida de novo aqui por segurança.
 */
export async function GET(_req: Request, ctx: { params: Promise<{ exameId: string; anexoId: string }> }) {
  const sessao = await auth();
  if (!sessao?.user) return new Response("Não autenticado.", { status: 401 });

  const { exameId, anexoId } = await ctx.params;
  if (!GUID.test(exameId) || !GUID.test(anexoId)) return new Response("Parâmetros inválidos.", { status: 400 });

  const res = await apiBruta(`/api/exames/${exameId}/anexos/${anexoId}`);
  if (!res.ok) return new Response("Anexo não encontrado.", { status: res.status === 404 ? 404 : 502 });

  const headers = new Headers();
  for (const h of ["content-type", "content-disposition", "content-length"]) {
    const v = res.headers.get(h);
    if (v) headers.set(h, v);
  }
  headers.set("cache-control", "private, no-store");
  return new Response(res.body, { status: 200, headers });
}
