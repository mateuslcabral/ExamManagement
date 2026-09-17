import { apiArquivo, ApiError } from "@/lib/api";

/** Download de anexo em stream. Anexos nunca são visíveis ao paciente (Q23) — esta rota é só do sistema interno. */
export async function GET(_request: Request, { params }: { params: Promise<{ id: string; anexoId: string }> }) {
  const { id, anexoId } = await params;

  try {
    const res = await apiArquivo(`/api/exames/${encodeURIComponent(id)}/anexos/${encodeURIComponent(anexoId)}`);
    const headers = new Headers({ "Cache-Control": "private, no-store" });
    for (const h of ["content-type", "content-length", "content-disposition"]) {
      const valor = res.headers.get(h);
      if (valor) headers.set(h, valor);
    }
    return new Response(res.body, { status: 200, headers });
  } catch (e) {
    if (e instanceof ApiError) {
      const mensagem = e.status === 401 ? "Sessão expirada. Entre novamente." : "Anexo não encontrado.";
      return new Response(mensagem, { status: e.status === 401 ? 401 : 404 });
    }
    throw e;
  }
}
