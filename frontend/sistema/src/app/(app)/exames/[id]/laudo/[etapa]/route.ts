import { apiArquivo, ApiError } from "@/lib/api";

const ETAPAS = new Set(["LaudoParceiroPronto", "LaudoCligenParaRevisao", "LaudoRevisado"]);
/** 50 MB do arquivo (P1) + folga do envelope multipart — mesmo limite da API. */
const LIMITE_BYTES = 51 * 1024 * 1024;

/**
 * Upload (registro ou substituição) e download do PDF de uma etapa do laudo, em stream, fora do proxy
 * (ver proxy.ts). A sessão é verificada em apiArquivo.
 */
export async function POST(request: Request, { params }: { params: Promise<{ id: string; etapa: string }> }) {
  const { id, etapa } = await params;
  if (!ETAPAS.has(etapa)) return erro(400, "Etapa inválida.");

  const tamanho = Number(request.headers.get("content-length"));
  if (!tamanho) return erro(411, "Envio sem tamanho declarado.");
  if (tamanho > LIMITE_BYTES) return erro(413, "O arquivo excede o limite de 50 MB.");
  const tipo = request.headers.get("content-type") ?? "";
  if (!tipo.startsWith("multipart/form-data")) return erro(415, "Envio inválido.");

  try {
    const res = await apiArquivo(`/api/exames/${encodeURIComponent(id)}/laudo/${etapa}`, {
      method: "POST",
      headers: { "Content-Type": tipo },
      body: request.body,
      duplex: "half",
    });
    return new Response(res.body, { status: 200, headers: { "Content-Type": "application/json" } });
  } catch (e) {
    if (e instanceof ApiError) return erro(e.status, e.message);
    throw e;
  }
}

export async function GET(_request: Request, { params }: { params: Promise<{ id: string; etapa: string }> }) {
  const { id, etapa } = await params;
  if (!ETAPAS.has(etapa)) return new Response("Etapa inválida.", { status: 400 });

  try {
    const res = await apiArquivo(`/api/exames/${encodeURIComponent(id)}/laudo/${etapa}/arquivo`);
    const headers = new Headers({ "Cache-Control": "private, no-store" });
    for (const h of ["content-type", "content-length", "content-disposition"]) {
      const valor = res.headers.get(h);
      if (valor) headers.set(h, valor);
    }
    return new Response(res.body, { status: 200, headers });
  } catch (e) {
    if (e instanceof ApiError) {
      return new Response(e.status === 401 ? "Sessão expirada. Entre novamente." : "Arquivo não encontrado.", {
        status: e.status === 401 ? 401 : 404,
      });
    }
    throw e;
  }
}

function erro(status: number, detail: string) {
  return Response.json({ detail }, { status });
}
