import { apiArquivo, ApiError } from "@/lib/api";

/** 50 MB do arquivo (P1) + folga do envelope multipart — mesmo limite da API. */
const LIMITE_BYTES = 51 * 1024 * 1024;

/**
 * Upload de anexo: repassa o corpo multipart em stream para a API, sem carregar o arquivo em memória.
 * Fora do proxy (ver proxy.ts), por isso a sessão é verificada em apiArquivo.
 */
export async function POST(request: Request, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;

  // Recusa cedo: acima do limite, a API derruba a conexão sem resposta legível.
  const tamanho = Number(request.headers.get("content-length"));
  if (!tamanho) return erro(411, "Envio sem tamanho declarado.");
  if (tamanho > LIMITE_BYTES) return erro(413, "O arquivo excede o limite de 50 MB.");

  const tipo = request.headers.get("content-type") ?? "";
  if (!tipo.startsWith("multipart/form-data")) return erro(415, "Envio inválido.");

  try {
    const res = await apiArquivo(`/api/exames/${encodeURIComponent(id)}/anexos`, {
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

function erro(status: number, detail: string) {
  return Response.json({ detail }, { status });
}
