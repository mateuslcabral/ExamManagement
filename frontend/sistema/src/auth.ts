import NextAuth, { type DefaultSession } from "next-auth";
import Credentials from "next-auth/providers/credentials";
import { api, ApiError, type Identidade, type TipoLogin } from "@/lib/api";

/**
 * Auth.js (NextAuth v5) no papel de BFF. Provider de credenciais delega a validação à API;
 * a sessão é um cookie httpOnly emitido aqui. Provider Google fica para a próxima etapa
 * (decisão registrada em docs/02-arquitetura/autenticacao.md).
 */

declare module "next-auth" {
  interface Session {
    user: { id: string; tipoLogin: TipoLogin } & DefaultSession["user"];
  }
  interface User {
    tipoLogin?: TipoLogin;
  }
}

export const { handlers, auth, signIn, signOut } = NextAuth({
  session: { strategy: "jwt", maxAge: 8 * 60 * 60 }, // 8 h de expediente
  pages: { signIn: "/login" },
  providers: [
    Credentials({
      credentials: { email: {}, senha: {} },
      async authorize(credentials) {
        const email = String(credentials?.email ?? "");
        const senha = String(credentials?.senha ?? "");
        if (!email || !senha) return null;

        try {
          const id = await api<Identidade>("/api/auth/validar", {
            method: "POST",
            body: { email, senha },
          });
          return { id: id.id, name: id.nome, email: id.email, tipoLogin: id.tipoLogin };
        } catch (e) {
          if (e instanceof ApiError && e.status === 401) return null;
          throw e;
        }
      },
    }),
  ],
  callbacks: {
    jwt({ token, user }) {
      if (user) {
        token.id = user.id;
        token.tipoLogin = user.tipoLogin;
      }
      return token;
    },
    session({ session, token }) {
      session.user.id = token.id as string;
      session.user.tipoLogin = token.tipoLogin as TipoLogin;
      return session;
    },
  },
});
