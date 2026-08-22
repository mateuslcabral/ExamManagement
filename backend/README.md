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
   - `Armazenamento:DiretorioBase` — pasta dos anexos (provisório em disco, P27; padrão `dados/arquivos`, ignorada pelo git)
2. Crie o banco e aplique as migrations:
   ```bash
   dotnet ef database update --project src/Cligen.Infraestrutura --startup-project src/Cligen.Api
   ```
3. Rode a API:
   ```bash
   dotnet run --project src/Cligen.Api
   ```
   Swagger em `http://localhost:5053/swagger` (porta do `launchSettings.json`). No primeiro start o seed cria o admin e 3 exames de exemplo no catálogo (P24).

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
| `GET` | `/api/pacientes?busca=&incluirExcluidos=` | Listar (busca por nome, documento, e-mail ou telefone; excluídos só se pedido) |
| `GET` | `/api/pacientes/{id}` | Detalhe com responsável legal e dados de exclusão |
| `POST` | `/api/pacientes` | Criar — 409 *"Esse CPF já foi utilizado"* se documento repetido; dispara boas-vindas (log até definir provedores) |
| `PUT` | `/api/pacientes/{id}` | Atualizar (documento editável — P18); responsável exigido para menor de 18 (P19) |
| `POST` | `/api/pacientes/{id}/excluir` | Exclusão lógica com `usuarioId` + `motivo` obrigatórios (P16) |
| `POST` | `/api/pacientes/{id}/restaurar` | Desfaz a exclusão (409 se houver exame ativo — P25) |
| `GET` | `/api/pacientes/{id}/exames?incluirExcluidos=` | Exames do paciente (1 → N) |
| `GET` / `POST` | `/api/catalogo-exames?somenteAtivos=` | Catálogo: listar (com prazo total calculado) / criar |
| `PUT` | `/api/catalogo-exames/{id}` · `POST …/desativar` · `…/reativar` | Catálogo: editar / aposentar / reativar |
| `GET` / `PUT` | `/api/parametros` · `/api/parametros/DiasRevisao` | Parâmetros globais (dias de revisão — P13/P28) |
| `GET` / `POST` | `/api/exames?busca=&incluirExcluidos=` | Exames: listar / criar (nasce em *Aguardando amostra*; preço herdado do catálogo se omitido) |
| `GET` / `PUT` | `/api/exames/{id}` | Detalhe com anexos / editar origem, destino, médico, preço |
| `POST` | `/api/exames/{id}/excluir` · `/restaurar` | Exclusão lógica (`usuarioId` + `motivo`) / restaurar |
| `POST` | `/api/exames/{id}/anexos` (multipart `arquivo`) | Até 3 anexos, PDF/JPG/PNG, 50 MB. Disco local provisório (P27, `Armazenamento:DiretorioBase`) |
| `GET` / `DELETE` | `/api/exames/{id}/anexos/{anexoId}` | Download / remoção física do anexo |
| `POST` | `/api/exames/{id}/amostras` (`usuarioId`, `dataAcolhimento?`) | Acolhimento 1→2: grava a previsão (acolhimento + prazo + dias de revisão) uma única vez (P29/P30) |
| `POST` | `/api/exames/{id}/amostras/rejeitar` (`usuarioId`, `motivo`) | Rejeição 2→1: zera a previsão; recoleta = novo acolhimento (P31) |
| `POST` | `/api/exames/{id}/laudo/{etapa}` (multipart `arquivo` PDF + `usuarioId`) | Registra a etapa (`LaudoParceiroPronto` → `LaudoCligenParaRevisao` → `LaudoRevisado`, sem pular) ou substitui o arquivo dela (P32/P33) |
| `GET` | `/api/exames/{id}/laudo/{etapa}/arquivo` | Download do PDF da etapa |
| `POST` | `/api/exames/{id}/laudo/disponibilizar` | 5→6: grava `dataLiberacaoEfetiva` e dispara e-mail + WhatsApp (log) no modelo interno/externo (P34) |

## Provisório (trocar sem tocar na Aplicação)

- `IEnvioEmail` → `EnvioEmailLog`: **não envia**, só registra o link no log (provedor pendente — Q37).
- Autenticação BFF→API por chave de serviço estática (`X-Api-Key`).
