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

## Endpoints

Todos exigem o header `X-Api-Key`. Fora de `/api/auth`, exigem também `X-Usuario-Id` com o id de um usuário **ativo** (repassado pelo BFF a partir da sessão; 401 caso contrário).

| Método | Rota | Uso |
|---|---|---|
| `POST` | `/api/auth/validar` | BFF valida e-mail/senha → identidade (401 se inválido) |
| `POST` | `/api/auth/definir-senha` | Primeiro acesso / redefinição de conta local (token do e-mail) |
| `GET` | `/api/usuarios` | Listar |
| `POST` | `/api/usuarios` | Criar — tipo decidido pelo e-mail (`@gmail.com` → Google ativo; demais → Local, envia e-mail) |
| `PUT` | `/api/usuarios/{id}` | Renomear |
| `POST` | `/api/usuarios/{id}/desativar` · `/reativar` · `/reenviar-definicao-senha` | Ações |
| `GET` | `/api/catalogo-exames?apenasAtivos=` | Listar catálogo — cada item traz `prazoEntregaDias` (execução + dias de revisão) |
| `POST` · `PUT` | `/api/catalogo-exames` · `/api/catalogo-exames/{id}` | Criar / editar (nome único, prazo ≥ 1 dia, preço ≥ 0 com 2 casas) |
| `POST` | `/api/catalogo-exames/{id}/desativar` · `/reativar` | Sem exclusão: exame aposentado é desativado |
| `GET` | `/api/pacientes?busca=&pagina=&tamanhoPagina=` | Buscar por parte do nome ou início do documento (paginado) |
| `GET` · `POST` · `PUT` | `/api/pacientes/{id}` · `/api/pacientes` | Obter / cadastrar (dispara boas-vindas) / editar. Sem exclusão |
| `GET` | `/api/exames?busca=&pacienteId=&estado=&pagina=&tamanhoPagina=` | Buscar (paciente, documento ou nome do exame). Excluídos não aparecem |
| `GET` · `POST` · `PUT` | `/api/exames/{id}` · `/api/exames` | Obter / cadastrar / editar |
| `POST` | `/api/exames/{id}/excluir` | Exclusão lógica — corpo `{ motivo }` obrigatório |
| `POST` | `/api/exames/{id}/amostra/acolher` | Corpo `{ dataAcolhimento }`. Grava a previsão (acolhimento + execução + revisão) e avança o estado |
| `POST` | `/api/exames/{id}/amostra/rejeitar` | Corpo `{ motivo }`. Zera a previsão; exame volta a aguardar amostra |
| `POST` | `/api/exames/{id}/anexos` | Upload multipart (campo `arquivo`), PDF/JPG/PNG até 50 MB, máx. 3 ativos |
| `GET` · `POST` | `/api/exames/{id}/anexos/{anexoId}` · `…/remover` | Download / remoção lógica |
| `GET` · `PUT` | `/api/parametros/dias-revisao` | Dias de revisão globais (P13), semeados com 3 pela migration |

## Provisório (trocar sem tocar na Aplicação)

- `IEnvioEmail` → `EnvioEmailLog`: **não envia**, só registra no log (provedor pendente — Q37).
- `IEnvioWhatsApp` → `EnvioWhatsAppLog`: **não envia**, só registra no log (textos C7 e templates Meta pendentes).
- `IArmazenamentoArquivos` → `ArmazenamentoArquivosLocal`: anexos em disco, em `ArmazenamentoLocal:Diretorio` (padrão `src/Cligen.Api/App_Data/arquivos`, ignorado pelo git). Sem redundância — faça backup se houver dado real.
- Autenticação BFF→API por chave de serviço estática (`X-Api-Key`) + identidade do usuário em `X-Usuario-Id`.
