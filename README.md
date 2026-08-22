# Cligen

Plataforma web para a Cligen gerenciar o ciclo completo de exames genéticos: cadastro de pacientes, registro de exames, controle do fluxo de laudo em etapas, registro financeiro e disponibilização do resultado ao paciente por portal próprio, com notificação automática por e-mail e WhatsApp.

## Estrutura do repositório

```
.
├── backend/    # API ASP.NET Core (C#) + Entity Framework Core + SQL Server
├── frontend/   # Aplicação(ões) de frontend — framework a definir, ver docs/06-pendencias
└── docs/       # Documentação funcional e técnica do projeto
```

## Documentação

Toda a documentação do projeto — stack, arquitetura, escopo, schema, regras de negócio e pendências — vive em [`docs/`](docs/README.md). Comece por lá.

## Stack

| Camada | Escolha |
|---|---|
| Backend | ASP.NET Core (C#) |
| ORM | Entity Framework Core |
| Banco de dados | SQL Server |
| Armazenamento de arquivos | Object storage externo ao banco |
| Frontend | React + Next.js (BFF), um app por superfície |
| Autenticação (sistema interno) | OAuth/OIDC — login próprio + Google |

Detalhes em [`docs/01-stack/stack-tecnologica.md`](docs/01-stack/stack-tecnologica.md).
