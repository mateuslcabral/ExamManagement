# Arquitetura do backend — camadas (DDD) e SOLID

> Fecha a pendência "organização da solução backend" registrada em [`visao-geral.md`](visao-geral.md) e [`06-pendencias/pendencias-abertas.md`](../06-pendencias/pendencias-abertas.md). Decisão tomada em 22/08/2026.

## Camadas

Quatro projetos .NET, com uma **regra de dependência de mão única**: cada camada só pode referenciar a(s) camada(s) abaixo dela. Nada nunca referencia a `Api` de volta, e o `Domínio` não referencia nada.

```
Cligen.Api             (Controllers, autenticação servidor-a-servidor, Program.cs)
        │  depende de
        ▼
Cligen.Aplicacao       (Interfaces, DTOs, Services — casos de uso)
        │  depende de
        ▼
Cligen.Dominio         (Entidades, regras de negócio, exceções de domínio)

Cligen.Infraestrutura  (Repositórios, Migrations, integrações externas)
   implementa as interfaces definidas em Cligen.Aplicacao e Cligen.Dominio
```

> **Nota sobre a 4ª camada.** Você listou API / Aplicação / Banco de dados. Para que isso seja **DDD** de fato — e não apenas "camadas técnicas" — falta uma peça: um projeto **`Cligen.Dominio`** próprio, sem dependência de EF Core, ASP.NET ou qualquer coisa técnica, onde vivem as entidades com suas regras de negócio reais (ex.: o cálculo de `previsão de liberação`, a regra de `valor pago = exame − desconto`, as transições de estado do exame). Sem essa camada isolada, essas regras acabam vazando para dentro dos Services da Aplicação ou, pior, para dentro dos Repositórios — e aí deixa de ser DDD, vira só um CRUD em camadas. Também estendi "Banco de dados" para **Infraestrutura**, porque ali entram não só EF Core/Migrations, mas as integrações externas já decididas (object storage, e-mail, WhatsApp) — são o mesmo tipo de preocupação técnica que o Domínio e a Aplicação não deveriam conhecer.

| Camada | Papel | Conhece |
|---|---|---|
| **Api** | Controllers (HTTP, roteamento, status code, model binding), autenticação/autorização, `Program.cs` (composition root) | Aplicação |
| **Aplicação** | Casos de uso (Services), Interfaces de tudo que vem de fora (`IExameRepositorio`, `IArmazenamentoArquivos`, `IEnvioEmail`, `IEnvioWhatsApp`), DTOs de entrada/saída | Domínio |
| **Domínio** | Entidades, Value Objects, regras de negócio, exceções de domínio | Nada (nem EF Core, nem ASP.NET) |
| **Infraestrutura** | `DbContext`, Migrations, implementação dos repositórios, clientes de object storage/e-mail/WhatsApp | Aplicação (implementa as interfaces) e Domínio |

O `Program.cs` da `Api` é o único lugar que conhece a `Infraestrutura` — é lá que a injeção de dependência liga `IExameRepositorio` (interface, em Aplicação) à `ExameRepositorio` (implementação EF Core, em Infraestrutura).

## Estrutura de solution proposta

```
backend/
├── Cligen.sln
├── src/
│   ├── Cligen.Api/
│   │   ├── Controllers/
│   │   │   ├── PacientesController.cs
│   │   │   ├── ExamesController.cs
│   │   │   ├── AmostrasController.cs
│   │   │   ├── LaudosController.cs
│   │   │   ├── PagamentosController.cs
│   │   │   └── AutenticacaoController.cs
│   │   └── Program.cs
│   │
│   ├── Cligen.Aplicacao/
│   │   ├── Interfaces/
│   │   │   ├── Repositorios/         # IPacienteRepositorio, IExameRepositorio, IPagamentoRepositorio, IUsuarioRepositorio...
│   │   │   └── Servicos/             # IArmazenamentoArquivos, IEnvioEmail, IEnvioWhatsApp
│   │   ├── DTOs/                     # PacienteDto, ExameSolicitadoDto, DisponibilizarLaudoRequest...
│   │   └── Servicos/                 # PacienteService, ExameService, AcolhimentoService, LaudoService, FinanceiroService...
│   │
│   ├── Cligen.Dominio/
│   │   ├── Entidades/                # Paciente, Exame, ExameCatalogo, Amostra, EtapaAndamento, Pagamento, Usuario...
│   │   ├── Excecoes/                 # DocumentoJaCadastradoException, AmostraNaoAcolhidaException...
│   │   └── Enums/                    # EstadoExame, StatusPagamento, TipoLogin...
│   │
│   └── Cligen.Infraestrutura/
│       ├── Persistencia/
│       │   ├── CligenDbContext.cs
│       │   ├── Migrations/
│       │   └── Repositorios/         # ExameRepositorio : IExameRepositorio, etc.
│       └── Integracoes/
│           ├── ArmazenamentoArquivos/  # implementação do provedor de object storage escolhido
│           ├── Email/                 # implementação do provedor de e-mail transacional
│           └── WhatsApp/              # implementação do cliente da WhatsApp Business API
│
└── tests/
    ├── Cligen.Dominio.Tests/
    ├── Cligen.Aplicacao.Tests/
    └── Cligen.Infraestrutura.Tests/
```

## Onde a "Gestão de Usuários" e a autenticação entram

`AutenticacaoController` na `Api` expõe o endpoint que o BFF (Next.js) chama para validar e-mail/senha (ver [autenticação](autenticacao.md)). A regra de "conta `google` ativa na criação / conta `local` recebe e-mail de definição de senha" vive em `UsuarioService` (Aplicação), que usa `IUsuarioRepositorio` e `IEnvioEmail` — nenhuma dessas duas é uma classe concreta na Aplicação, só interface. Quem decide *qual* provedor de e-mail realmente dispara o envio é a `Infraestrutura`.

## SOLID aplicado neste desenho

- **Single Responsibility** — Controller só traduz HTTP em chamada de Service e Service em resposta HTTP; toda a regra de negócio fica no Service (Aplicação) orquestrando o Domínio, nunca no Controller. Repositório só sabe persistir/consultar, não decide regra.
- **Open/Closed** — novo canal de notificação (ex. SMS, futuramente) só exige uma nova implementação de uma interface já existente (`IEnvioSms`), sem tocar no `LaudoService` que já dispara e-mail/WhatsApp.
- **Liskov Substitution** — qualquer implementação de `IArmazenamentoArquivos` (Azure Blob hoje, S3 amanhã) tem que se comportar de forma equivalente para quem consome a interface — a troca de provedor de object storage (pendência em aberto) não deve exigir mudar `Cligen.Aplicacao`.
- **Interface Segregation** — interfaces pequenas e específicas por responsabilidade (`IEnvioEmail`, `IEnvioWhatsApp`) em vez de uma `INotificador` genérica que obriga quem implementa e-mail a também implementar WhatsApp.
- **Dependency Inversion** — a regra de dependência da seção acima *é* a aplicação prática deste princípio: Aplicação e Domínio definem o que precisam via interface; Infraestrutura é quem depende delas, nunca o contrário. É isso que permite trocar SQL Server, provedor de storage ou provedor de e-mail sem tocar em regra de negócio.

## Exemplo ponta a ponta — disponibilizar laudo

Ilustra as quatro camadas trabalhando juntas num caso de uso já especificado (ver [fluxo-laudo.md](../05-regras-negocio/fluxo-laudo.md)):

1. **Api** — `LaudosController.Disponibilizar(exameId)` recebe a requisição do BFF, chama `_laudoService.DisponibilizarAsync(exameId)`.
2. **Aplicação** — `LaudoService` busca o `Exame` via `IExameRepositorio`, invoca a regra de domínio `exame.Disponibilizar()` (grava `data_liberacao_efetiva`, valida que o laudo revisado existe), persiste via `IExameRepositorio.AtualizarAsync`, e dispara `IEnvioEmail`/`IEnvioWhatsApp` no modelo correspondente ao tipo de médico solicitante (D11, D13).
3. **Domínio** — a entidade `Exame` é quem sabe que só pode disponibilizar se estiver no estado "Laudo revisado" (estado 5) e quem grava a data — essa regra vive no método `Disponibilizar()` da própria entidade, não no Service. Se a regra for violada, lança uma exceção de domínio (`EstadoInvalidoException`), que o Service/Controller traduz para um HTTP 409, por exemplo.
4. **Infraestrutura** — `ExameRepositorio` executa o `UPDATE` via EF Core; a implementação concreta de `IEnvioEmail`/`IEnvioWhatsApp` chama o provedor real (a definir).

## Em aberto

- Estratégia de mapeamento EF Core (Fluent API vs. Data Annotations) — recomenda-se Fluent API para manter as entidades de `Cligen.Dominio` livres de atributos do EF Core.
- Estratégia de tratamento de exceção de domínio → código HTTP (middleware global de exceção na `Api`).
- Se `Pagamento` é agregado próprio (repositório dedicado) ou parte do agregado `Exame` — proposta: agregado próprio, dado que relatórios financeiros (regime de caixa) cruzam pagamentos de vários exames e não fazem sentido carregando o `Exame` inteiro. Confirmar antes da primeira migration.
- Estrutura de testes automatizados (unitários no Domínio/Aplicação, integração na Infraestrutura) — ver também item de testes em [pendências](../06-pendencias/pendencias-abertas.md).
