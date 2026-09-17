export { auth as proxy } from "@/auth";

/**
 * Protege tudo exceto a tela de login, definição de senha, rotas do Auth.js e estáticos.
 * Next 16 renomeou middleware.ts -> proxy.ts.
 *
 * Anexos e laudos de exame também ficam fora: o proxy bufferiza o corpo em memória e o trunca acima de 10 MB
 * (proxyClientMaxBodySize), o que corromperia uploads de até 50 MB. Esses route handlers verificam a sessão sozinhos.
 */
export const config = {
  matcher: [
    "/((?!login|definir-senha|api/auth|exames/[^/]+/anexos|exames/[^/]+/laudo|_next/static|_next/image|favicon.ico|logo-cligen-branca.png).*)",
  ],
};
