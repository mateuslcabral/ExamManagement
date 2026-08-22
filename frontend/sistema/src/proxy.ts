export { auth as proxy } from "@/auth";

/**
 * Protege tudo exceto a tela de login, definição de senha, rotas do Auth.js e estáticos.
 * Next 16 renomeou middleware.ts -> proxy.ts.
 */
export const config = {
  matcher: ["/((?!login|definir-senha|api/auth|_next/static|_next/image|favicon.ico|logo-cligen-branca.png).*)"],
};
