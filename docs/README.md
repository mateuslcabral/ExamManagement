# Documentação — Cligen

Índice da documentação técnica e funcional do projeto Cligen (plataforma de gestão de exames e pacientes).

## Como este /docs está organizado

| Pasta | Conteúdo |
|---|---|
| [`00-documentos-formais/`](00-documentos-formais/) | Documentos originais e formais do projeto (ata, especificação consolidada, pendências, planilha de levantamento). **Fonte de verdade histórica** — os arquivos abaixo são uma quebra navegável do mesmo conteúdo para uso no dia a dia de desenvolvimento. |
| [`01-stack/`](01-stack/) | Stack tecnológica escolhida e decisões de implementação decorrentes. |
| [`02-arquitetura/`](02-arquitetura/) | Arquitetura geral, armazenamento de arquivos, retenção de longo prazo. |
| [`03-escopo/`](03-escopo/) | O que entra e o que fica de fora da primeira versão (funcionalidade). |
| [`04-schema/`](04-schema/) | Modelo de dados / entidades. |
| [`05-regras-negocio/`](05-regras-negocio/) | Regras de negócio já definidas, uma pasta por módulo. |
| [`06-pendencias/`](06-pendencias/) | O que ainda está em aberto: decisões de conflito, pendências, premissas assumidas, riscos aceitos, itens diferidos. |
| [`07-roadmap/`](07-roadmap/) | Situação atual dos módulos e ordem sugerida do que falta. |
| [`08-apresentacao/`](08-apresentacao/) | Inventário do que existe, pendências por área (o que falta fazer e o que falta ver com o cliente) e roteiro de apresentação. |
| [`para-rafael.md`](para-rafael.md) | Todas as decisões tomadas na implementação, dúvidas e dilemas, consolidadas para validação. |

## Documentos de origem

- **CLG-ATA-2026-001** — ata de levantamento (29/07/2026)
- **Dados Mateus** — planilha de levantamento
- Questionário Q1–Q48 e rodadas de conflito C1–C7 (06/08/2026)
- **CLG-ESP-2026-001 v1.0** — especificação funcional consolidada (06/08/2026)
- **CLG-PEND-2026-001 v1.3** — pendências, premissas e riscos (06/08/2026)

## Convenção

Cada regra de negócio nos documentos formais traz entre parênteses a referência de origem:
`D` = decisão da ata · `Q` = resposta do questionário · `C` = resolução de conflito · `P` = premissa técnica adotada.
Essa referência foi preservada nos documentos divididos abaixo sempre que relevante, para rastreabilidade.

## Status do projeto (19/10/2026)

Sistema interno operacional de ponta a ponta (pacientes, catálogo, cadastros auxiliares, exames, amostra, laudo, fluxos, financeiro, usuários) — ver [`08-apresentacao/o-que-temos.md`](08-apresentacao/o-que-temos.md). Reunião com o cliente em 17/10/2026 fechou C4.5/C4.2 (convênio como cadastro), Q1.6.1 (sem dias de revisão; P13 derrubada), Q22, Q9 e Q38.1; os 17 pedidos foram implementados em 19/10 — resumo em [`para-rafael.md`](para-rafael.md). Seguem pendentes: regras finas de acolhimento de amostra (Q1.1–Q1.7), hospedagem, provedores de storage/e-mail/WhatsApp, portal do paciente, log de acesso e demais itens de planejamento técnico. Ver [`06-pendencias/`](06-pendencias/) e [`07-roadmap/`](07-roadmap/).

### Rotas do sistema interno (`frontend/sistema`)

`/` · `/pacientes` · `/exames` (+ `/exames/[id]`, `?excluidos=1`) · `/fluxos` · `/financeiro` · `/catalogo` (consulta) · `/catalogo/gerenciar` (CRUD) · `/cadastros` (origens, destinos, convênios) · `/usuarios`. Endpoints da API em [`../backend/README.md`](../backend/README.md).
