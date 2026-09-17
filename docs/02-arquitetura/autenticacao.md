# Autenticação

> Fecha a pendência de "autenticação da equipe interna" e "framework de frontend" registradas em [`06-pendencias/pendencias-abertas.md`](../06-pendencias/pendencias-abertas.md). Decisão tomada em 22/08/2026.

Existem **dois modelos de autenticação completamente distintos**, um por superfície — não devem ser confundidos nem compartilhar código de auth:

| Superfície | Público | Modelo |
|---|---|---|
| Sistema interno | Equipe Cligen (4 usuários) | OAuth/OIDC — login próprio + Google |
| Portal do paciente | Pacientes/responsáveis legais | Documento + código de uso único (já especificado, sem senha) — ver [portal-paciente.md](../05-regras-negocio/portal-paciente.md) |

## Padrão BFF (Backend for Frontend)

Cada superfície é servida por seu **próprio app Next.js**, que atua como BFF:

- O **navegador nunca fala diretamente com a API ASP.NET Core**. Ele só fala com o Next.js (mesma origem).
- O Next.js roda no servidor a troca de tokens (OAuth com Google, validação de e-mail/senha, validação do código de uso único do paciente) e, ao final, emite seu **próprio cookie de sessão** (`httpOnly`, `Secure`, `SameSite=Lax` ou `Strict`).
- Nenhum token de acesso (Google, ou da API) chega a existir no JavaScript do navegador. Isso elimina a superfície de roubo de token via XSS que uma SPA pura teria.
- A API ASP.NET Core só aceita chamadas vindas do runtime server-side do BFF correspondente — nunca do navegador diretamente.

**Mecanismo BFF → API (decidido e implementado em 22/08/2026): chave de serviço estática** no header `X-Api-Key`, comparada em tempo constante por um middleware da API (`ChaveDeServicoMiddleware`). Escolhida sobre client-credentials OAuth2 por haver um único cliente (o BFF) e nenhum provedor de identidade na arquitetura; sobre isolamento de rede por não depender de hospedagem ainda indefinida. A chave vive só em variável de ambiente do servidor Next.js (`CLIGEN_API_KEY`) e do backend (`ChaveDeServico`); nunca é exposta ao navegador (`lib/api.ts` é `server-only`). Reavaliar para client-credentials se surgir um segundo consumidor da API.

## Sistema interno — login próprio + Google

### Por que dois métodos

- **Login próprio (e-mail + senha)** — sempre disponível, não depende de infraestrutura externa (Google fora do ar, ou funcionário sem conta corporativa ainda provisionada).
- **Login Google** — conveniência para quem já usa Google Workspace da Cligen no dia a dia; evita mais uma senha para lembrar.

### Implementação

- **Auth.js v5 (NextAuth)** rodando dentro do próprio Next.js do sistema interno (`frontend/sistema/src/auth.ts`), sessão JWT em cookie `httpOnly` com 8 h de validade, com dois providers:
  - **Credentials provider** — **implementado.** Recebe e-mail/senha, chama `POST /api/auth/validar` na API, que verifica o hash da senha do `Usuario` (PBKDF2 via `PasswordHasher` do ASP.NET Core Identity). A resposta é opaca (401 sem distinguir e-mail inexistente, senha errada ou usuário inativo).
  - **Google provider (OIDC)** — **próxima etapa.** Depende de criar o projeto no Google Cloud (Client ID/Secret) e de confirmar Gmail pessoal vs. Workspace (ver "Em aberto"). A entidade `Usuario` e a regra de provisionamento já estão prontas para ele (`google_subject_id`, `VincularGoogle()`).
- **Link de definição de senha** — token auto-contido `{usuarioId}.{expiraUnix}.{HMAC-SHA256}` (`TokenDefinicaoSenhaHmac`), sem estado no banco, validade configurável (padrão 24 h). Simples e suficiente para 4 usuários; não é revogável individualmente antes de expirar — reavaliar junto com a política de senha.
- **E-mail** — `IEnvioEmail` tem por ora uma implementação que só registra o link no log (`EnvioEmailLog`), até a definição do provedor (Q37).
- Optou-se por Auth.js embutido no BFF em vez de um servidor de identidade dedicado (ex. OpenIddict, Duende IdentityServer) porque **só existem 4 usuários e um único cliente (o próprio BFF)** consumindo essas credenciais — não há caso de uso para um provedor OIDC central compartilhado por múltiplos apps. Reavaliar se, no futuro, mais aplicações precisarem consumir o mesmo login.

### Regra de provisionamento — sem autocadastro

O cadastro de conta nunca é feito pelo próprio usuário — sempre por outro membro da equipe, na tela de **Gestão de Usuários** do sistema interno (mesmo princípio já usado para paciente — D1: nunca há autocadastro).

**O tipo de login é determinado pelo e-mail informado na criação (atualização de 22/08/2026):**

- **E-mail `@gmail.com`** → conta do tipo **Google**. Acesso é concedido **imediatamente na criação** (`ativo = true`) — não há senha e não há e-mail de definição de senha. O usuário simplesmente entra clicando "Entrar com Google"; no primeiro login, o sistema vincula o `google_subject_id` retornado pelo Google ao `Usuario` já cadastrado com aquele e-mail.
- **Qualquer outro e-mail** (não `@gmail.com`) → conta do tipo **local**. Ao criar o registro, o sistema dispara automaticamente um **e-mail de definição de senha** (fluxo de primeiro acesso, equivalente a um "esqueci minha senha") para que o próprio usuário escolha sua senha. **Ninguém mais define a senha por ele** — nem quem cria o cadastro, nem qualquer outro usuário.

Em ambos os casos, **não há autocadastro**: o registro `Usuario` só existe porque outra pessoa da equipe o criou antes; o e-mail recebido (Google) ou a senha escolhida (local) apenas ativam um acesso que já foi provisionado.

> **Nota técnica importante — revisão da decisão anterior.** A versão anterior deste documento assumia restrição por **domínio corporativo do Google Workspace** (ex. `@cligen.com.br`) para o login Google. A descrição de fluxo acima ("usuário `@gmail`") indica contas **pessoais do Gmail**, não um domínio Workspace da Cligen. Isso muda a validação possível: o parâmetro `hd` (hosted domain) do Google **não funciona para contas pessoais `@gmail.com`** — só existe para domínios Google Workspace. Se os 4 logins forem realmente Gmail pessoal, a única barreira de segurança possível no lado do OAuth é: o e-mail devolvido pelo Google precisa bater com um `Usuario` já cadastrado e `ativo` — sem camada adicional de restrição de domínio. **Confirmar com o cliente se é isso mesmo (Gmail pessoal dos 4 funcionários) antes de fechar a implementação**; se no futuro a Cligen adotar Google Workspace corporativo, a restrição por `hd` pode ser adicionada como camada extra.

**Sobre "permissão" concedida na criação.** Como os perfis de acesso internos diferenciados seguem fora do escopo da v1 (Q41, Q42 — ver [escopo](../03-escopo/escopo-v1.md)), "conceder permissão" na criação do usuário significa, por ora, apenas **ativar o acesso** (`ativo = true`) — os 4 usuários continuam com o mesmo nível de acesso entre si. Não introduz papéis/perfis diferenciados; se a intenção for outra, isso reabre Q41/Q42.

### Impacto no modelo de dados (`Usuario`)

A entidade `Usuario` (ver [schema](../04-schema/modelo-de-dados.md)) precisa de:

| Campo | Uso |
|---|---|
| `email` | Identificador único; usado tanto no login próprio quanto para vincular ao login Google |
| `tipo_login` | `google` ou `local`, definido na criação a partir do domínio do e-mail informado |
| `senha_hash` | Só para contas `local` (ASP.NET Core Identity cuida do hashing/verificação) — nulo para contas `google` |
| `google_subject_id` | `sub` do token OIDC do Google — só para contas `google`, vinculado no primeiro login (evita reamarração por e-mail a cada acesso) |
| `ativo` | Concedido na criação (contas `google`) ou após a definição de senha pelo próprio usuário (contas `local`); permite também desativar acesso sem apagar o histórico de autoria (exclusões, uploads, etc.) |

### Fluxo de criação na Gestão de Usuários

1. Um usuário já autenticado abre "Gestão de Usuários" e cadastra nome + e-mail do novo membro da equipe.
2. O sistema classifica automaticamente pelo domínio do e-mail: `@gmail.com` → `tipo_login = google`; qualquer outro domínio → `tipo_login = local`.
3. **Conta `google`:** `ativo = true` de imediato. O novo usuário já consegue entrar via "Entrar com Google" assim que o cadastro é salvo.
4. **Conta `local`:** o sistema envia um e-mail com link de definição de senha (token de uso único, com expiração — mesmo mecanismo de "esqueci minha senha" do ASP.NET Core Identity). `ativo` só vira `true` quando a senha é definida com sucesso.

### Sessão e chamada à API

1. Usuário autentica no Next.js (credenciais ou Google).
2. Auth.js confirma a identidade e verifica que existe `Usuario` ativo correspondente (chamando a API).
3. Next.js emite cookie de sessão próprio no navegador do usuário.
4. Em cada requisição, o Next.js (server-side) lê a sessão e chama a API repassando o id do `Usuario` no header `X-Usuario-Id` (`api()` em `lib/api.ts`; login e definição de senha usam `apiPublica()`, sem usuário).

**Repasse da identidade (decidido e implementado em 16/09/2026).** O header `X-Usuario-Id` não é assinado: ele só é aceito em requisições que já passaram pela chave de serviço, e apenas o BFF conhece a chave e preenche o header a partir da própria sessão. Um header assinado ou token interno não protegeria contra nada que a chave já não proteja — quem tivesse a chave poderia assinar também. O `UsuarioAtualMiddleware` da API exige o header em toda rota fora de `/api/auth`, confere que o usuário existe e está **ativo** (401 caso contrário) e o expõe à Aplicação como `IUsuarioAtual`, usado para autoria (`CriadoPorId`/`AtualizadoPorId`). Consequência: usuário desativado perde o acesso na hora, mesmo com a sessão de 8 h ainda válida — o BFF recebe 401 e encerra a sessão (`/sair`). Reavaliar para token assinado se a API passar a ter mais de um consumidor.

## Portal do paciente — sem OAuth

O portal **não** usa Google nem senha — mantém o fluxo já especificado (documento + código de uso único enviado por e-mail/WhatsApp, ver [portal-paciente.md](../05-regras-negocio/portal-paciente.md)). O app Next.js do portal também atua como BFF (mesmo princípio de cookie de sessão `httpOnly`), só que a "verificação de credencial" é a validação do código de uso único contra `TokenAcesso`, não um provider OAuth.

## Estrutura de repositório decorrente

Dois apps Next.js separados — cada superfície com seu próprio perímetro de segurança, domínio e ciclo de deploy — em vez de um único app com rotas para as duas. Ver [`frontend/README.md`](../../frontend/README.md) para o esqueleto de pastas.

## Em aberto

- **Confirmar se o login Google é mesmo Gmail pessoal** (`@gmail.com`) dos 4 funcionários, ou se a Cligen pretende adotar Google Workspace corporativo no futuro (o que permitiria reforçar com restrição de domínio via `hd`).
- Política de senha (tamanho mínimo, expiração, bloqueio por tentativas) para contas `local`.
- Validade do link de definição de senha enviado na criação de conta `local`.
- 2FA — não decidido; com só 4 usuários e dados sensíveis (prontuário), vale considerar para uma v2.
