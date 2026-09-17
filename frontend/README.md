# Frontend — Cligen

React + **Next.js** (App Router), um app por superfície, cada um atuando como **BFF** de autenticação e sessão. Decisão e racional em [`../docs/02-arquitetura/autenticacao.md`](../docs/02-arquitetura/autenticacao.md).

```
frontend/
├── sistema/       # Sistema interno (equipe Cligen) — IMPLEMENTADO: login, shell, Gestão de Usuários, Catálogo de exames, Pacientes
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
| `/pacientes` | Busca (nome, CPF, passaporte) e listagem paginada |
| `/pacientes/novo` · `/pacientes/[id]` | Cadastro e edição; seção de responsável legal obrigatória para menor, opcional para maior |
| `/sair` | Encerra a sessão quando a API deixa de aceitar o usuário (ex.: desativado) |
| `/exames` | Busca e listagem (filtro por paciente com `?pacienteId=`) |
| `/exames/novo` · `/exames/[id]` | Cadastro (com busca de paciente) · detalhe com anexos, edição e exclusão com motivo |
| `/exames/[id]/anexos` · `/exames/[id]/anexos/[anexoId]` | Route handlers de upload e download em stream. Ficam **fora do `proxy.ts`**, que bufferiza e trunca corpos acima de 10 MB; server actions também não servem (limite de 1 MB) |
| `/amostras` | Fila de exames aguardando amostra, com registro do acolhimento na própria linha |
| `/catalogo` | Catálogo de exames — criar, editar, desativar/reativar; mostra a entrega ao paciente (execução + revisão) e edita os dias de revisão |

### Rodar localmente

1. Backend rodando (ver [`../backend/README.md`](../backend/README.md)).
2. `cp .env.example .env.local` e preencha `AUTH_SECRET` e `CLIGEN_API_KEY` (mesma `ChaveDeServico` do backend).
3. `npm install && npm run dev` → `http://localhost:3000`.

### Estrutura

```
src/
├── auth.ts                    # Configuração do Auth.js (BFF): providers, sessão JWT em cookie httpOnly
├── proxy.ts                   # Protege todas as rotas exceto login/definir-senha (Next 16: ex-middleware)
├── lib/api.ts                 # Cliente da API — server-only, injeta X-Api-Key e X-Usuario-Id; tipos dos DTOs
├── lib/formatos.ts            # Exibição de CPF, telefone, datas; idade
├── components/ui/             # Botao, Campo, Selecao, Etiqueta (tokens Cligen)
├── components/layout/         # Sidebar, menu, ícones
└── app/
    ├── globals.css            # @theme com a identidade Cligen (cores, Montserrat, raio pill)
    ├── login/ · definir-senha/
    └── (app)/                 # Route group com o shell autenticado (sidebar + header)
        ├── usuarios/
        ├── catalogo/
        ├── exames/
        ├── amostras/
        └── pacientes/
```

### Identidade visual

Extraída de `cligen.com.br` em 22/08/2026 (ação A8): primária `#234C5A`, teal `#006C7F`, secundária `#738373`, fundos `#FAFAFA`/`#EEEEEE`, Montserrat, botões pill. Logo branco em `public/logo-cligen-branca.png`. Tokens em `src/app/globals.css`.
