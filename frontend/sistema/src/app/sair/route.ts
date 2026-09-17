import { signOut } from "@/auth";

/**
 * Encerra a sessão quando a API deixa de reconhecer o usuário (ex.: foi desativado com a sessão ainda aberta).
 * Precisa ser route handler: a renderização de página não pode apagar o cookie de sessão.
 */
export async function GET() {
  await signOut({ redirectTo: "/login?sessaoEncerrada=1" });
}
