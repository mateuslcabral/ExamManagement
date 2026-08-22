# Backend — Cligen API

ASP.NET Core 10 (C#) · Entity Framework Core 10 · SQL Server. **API pura** — chamada exclusivamente pelos BFFs Next.js (`frontend/`), nunca pelo navegador. Arquitetura em camadas DDD com SOLID: ver [`../docs/02-arquitetura/arquitetura-backend.md`](../docs/02-arquitetura/arquitetura-backend.md).

```
backend/
├── Cligen.slnx
├── src/
│   ├── Cligen.Dominio/          # Entidades, enums, exceções de domínio (sem dependências técnicas)
│   ├── Cligen.Aplicacao/        # Interfaces, DTOs, Services (casos de uso)
│   ├── Cligen.Infraestrutura/   # DbContext, Migrations, Repositórios, integrações (e-mail, hash, token)
│   └── Cligen.Api/              # Controllers, middlewares, Program.cs (composition root)
└── tests/
    └── Cligen.Dominio.Tests/    # xUnit — regras da entidade
```

## Pré-requisitos

- .NET SDK 10
- `dotnet tool install -g dotnet-ef`
- SQL Server local (instância padrão, autenticação integrada do Windows) — ou ajuste a connection string

## Primeira execução

1. Copie `src/Cligen.Api/appsettings.Development.example.json` para `appsettings.Development.json` e preencha:
   - `ChaveDeServico` e `TokenDefinicaoSenha:Segredo` — gere com `openssl rand -hex 32`
   - `SeedAdmin` — primeiro usuário (quem vai criar os demais pela Gestão de Usuários)
2. Crie o banco e aplique as migrations:
   ```bash
   dotnet ef database update --project src/Cligen.Infraestrutura --startup-project src/Cligen.Api
   ```
3. Rode a API:
   ```bash
   dotnet run --project src/Cligen.Api
   ```
   Swagger em `http://localhost:5053/swagger` (porta do `launchSettings.json`). O seed cria o admin no primeiro start.

## Nova migration

```bash
dotnet ef migrations add <Nome> --project src/Cligen.Infraestrutura --startup-project src/Cligen.Api --output-dir Persistencia/Migrations
```

## Testes

```bash
dotnet test
```

## Endpoints (todos exigem header `X-Api-Key`)

| Método | Rota | Uso |
|---|---|---|
| `POST` | `/api/auth/validar` | BFF valida e-mail/senha → identidade (401 se inválido) |
| `POST` | `/api/auth/definir-senha` | Primeiro acesso / redefinição de conta local (token do e-mail) |
| `GET` | `/api/usuarios` | Listar |
| `POST` | `/api/usuarios` | Criar — tipo decidido pelo e-mail (`@gmail.com` → Google ativo; demais → Local, envia e-mail) |
| `PUT` | `/api/usuarios/{id}` | Renomear |
| `POST` | `/api/usuarios/{id}/desativar` · `/reativar` · `/reenviar-definicao-senha` | Ações |

## Provisório (trocar sem tocar na Aplicação)

- `IEnvioEmail` → `EnvioEmailLog`: **não envia**, só registra o link no log (provedor pendente — Q37).
- Autenticação BFF→API por chave de serviço estática (`X-Api-Key`).
