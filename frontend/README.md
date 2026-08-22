# Frontend — Cligen

React + **Next.js** (App Router), um app por superfície, cada um atuando como **BFF** de autenticação e sessão. Decisão e racional em [`../docs/02-arquitetura/autenticacao.md`](../docs/02-arquitetura/autenticacao.md).

```
frontend/
├── sistema/       # Sistema interno (equipe Cligen) — IMPLEMENTADO: login, shell, Gestão de Usuários
└── portal/        # Portal do paciente (resultados.cligen.com.br) — ainda não iniciado
```

## `sistema/`

Next.js 16 · React 19 · TypeScript · Tailwind 4 · Auth.js v5 (`next-auth@beta`) · zod.

| Rota | Descrição |
|---|---|
| `/login` | E-mail + senha (provider de credenciais do Auth.js, validado pela API). Botão Google fica para a próxima etapa. |
| `/definir-senha?token=…` | Destino do link enviado a contas locais na criação (primeiro acesso / redefinição) |
| `/` | Início — cards dos módulos; os sem escopo fechado aparecem como "em breve" |
| `/usuarios` | Gestão de Usuários — listar, criar, desativar/reativar, reenviar link de senha |

### Rodar localmente

1. Backend rodando (ver [`../backend/README.md`](../backend/README.md)).
2. `cp .env.example .env.local` e preencha `AUTH_SECRET` e `CLIGEN_API_KEY` (mesma `ChaveDeServico` do backend).
3. `npm install && npm run dev` → `http://localhost:3000`.

### Estrutura

```
src/
├── auth.ts                    # Configuração do Auth.js (BFF): providers, sessão JWT em cookie httpOnly
├── proxy.ts                   # Protege todas as rotas exceto login/definir-senha (Next 16: ex-middleware)
├── lib/api.ts                 # Cliente da API — server-only, injeta X-Api-Key; tipos espelhando os DTOs
├── components/ui/             # Botao, Campo (tokens Cligen)
├── components/layout/         # Sidebar, menu, ícones
└── app/
    ├── globals.css            # @theme com a identidade Cligen (cores, Montserrat, raio pill)
    ├── login/ · definir-senha/
    └── (app)/                 # Route group com o shell autenticado (sidebar + header)
        └── usuarios/
```

### Identidade visual

Extraída de `cligen.com.br` em 22/08/2026 (ação A8): primária `#234C5A`, teal `#006C7F`, secundária `#738373`, fundos `#FAFAFA`/`#EEEEEE`, Montserrat, botões pill. Logo branco em `public/logo-cligen-branca.png`. Tokens em `src/app/globals.css`.
