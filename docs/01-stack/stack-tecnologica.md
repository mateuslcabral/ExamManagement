# Stack tecnológica

> Fonte: CLG-ESP-2026-001 §3, §13 · CLG-PEND-2026-001 §1.9

## Definido

| Camada | Escolha |
|---|---|
| Backend | ASP.NET Core (C#) |
| Acesso a dados | Entity Framework Core |
| Banco de dados | SQL Server (Express atende o volume previsto — ver dimensionamento) |
| Armazenamento de arquivos | Object storage externo ao banco (provedor a definir — ver [pendências](../06-pendencias/pendencias-abertas.md)) |
| Mensageria WhatsApp | WhatsApp Business API oficial — conta já existente na Cligen (Q36) |
| Mensageria e-mail | E-mail transacional (provedor e domínio de envio a definir — Q37) |
| Frontend | React, servido por **Next.js** — um app por superfície (sistema interno e portal do paciente), atuando como **BFF** (Backend for Frontend) |
| Autenticação (sistema interno) | OAuth/OIDC via o BFF Next.js — login próprio (e-mail/senha) + login Google, restrito a domínio corporativo — ver [autenticação](../02-arquitetura/autenticacao.md) |

## Decisões de implementação decorrentes

- **Arquivos não são gravados no banco.** O banco guarda caminho, nome original, tipo, tamanho e hash; o binário fica em object storage. Isso mantém o banco relacional pequeno mesmo com 20 anos de guarda, e o SQL Server Express (gratuito, limitado a 10 GB por banco) atende com folga o volume previsto.
- **Valores monetários usam `decimal`**, com mapeamento direto para `decimal` do SQL Server. Nenhum valor financeiro trafega como ponto flutuante.
- **Chave primária de paciente é `uniqueidentifier` (GUID) aleatório, não agrupada**, com chave sequencial interna para ordenação física — evita a fragmentação típica de GUID aleatório como índice agrupado (C1).
- **Exclusão é sempre lógica.** Nenhuma rotina do sistema remove fisicamente registro de exame, laudo ou pagamento (P4).

## Dimensionamento adotado (P2)

- 200 exames por mês
- 4 usuários internos simultâneos
- Acúmulo de arquivos: ~24 GB/ano (laudo médio de 10 MB) → ~480 GB ao fim de 20 anos de guarda

O custo de armazenamento nesse volume é irrelevante. O risco real da retenção de 20 anos é de **continuidade** (nenhum contrato de nuvem dura 20 anos), não financeiro — ver [arquitetura de armazenamento](../02-arquitetura/armazenamento-e-retencao.md).

## Decisão — backend passa a ser API pura

Com o frontend em React/Next.js, o ASP.NET Core deixa de renderizar qualquer página — vira **exclusivamente uma Web API**, chamada apenas pelo runtime server-side do Next.js (nunca diretamente pelo navegador). Ver [arquitetura de autenticação](../02-arquitetura/autenticacao.md) para o porquê.

## Decisão — organização em camadas (DDD) + SOLID

A API é organizada em quatro projetos — `Cligen.Api`, `Cligen.Aplicacao`, `Cligen.Dominio`, `Cligen.Infraestrutura` — com regra de dependência de mão única e princípios SOLID aplicados em cada camada. Detalhamento completo, estrutura de pastas e exemplo ponta a ponta em [arquitetura do backend](../02-arquitetura/arquitetura-backend.md).

## Em aberto

- Provedor de object storage (Azure Blob, S3, outro)
- Provedor de e-mail transacional e domínio de envio (Q37 — a cargo do Mateus)
- Provedor/BSP da WhatsApp Business API (ou uso direto da conta já existente)
- Hospedagem: nuvem, região (preferência por região no Brasil, por dado de saúde) e responsável pela contratação
- Mecanismo exato de autenticação servidor-a-servidor entre o BFF e a API (client credentials, chave interna, ou rede isolada) — ver [autenticação](../02-arquitetura/autenticacao.md)

Ver detalhes em [`06-pendencias/pendencias-abertas.md`](../06-pendencias/pendencias-abertas.md).
