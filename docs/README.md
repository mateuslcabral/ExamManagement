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
| [`07-roadmap/`](07-roadmap/) | Próximos passos de desenvolvimento: fases, dependências e dívidas técnicas. |

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

## Status do projeto (22/08/2026)

Especificação funcional, stack de backend, **frontend e autenticação da equipe interna** fechados (React + Next.js/BFF por superfície; OAuth próprio + Google — ver [`02-arquitetura/autenticacao.md`](02-arquitetura/autenticacao.md)). **Implementado:** login por senha, Gestão de Usuários, **Pacientes** (P16–P21), **Catálogo de exames + parâmetro de dias de revisão** **Exames** com anexos em disco local provisório (P22–P28), **acolhimento de amostra e fluxo do laudo** até a disponibilização com notificação em log (P29–P34). Ainda não: financeiro, portal do paciente, provedores reais de e-mail/WhatsApp/storage. Seguem pendentes: modelo de convênio (C4.5/C4.2), regras finas de acolhimento de amostra (Q1.1–Q1.7), hospedagem, provedores de storage/e-mail/WhatsApp, e demais itens de planejamento técnico (ambientes, testes, backup/DR). Ver [`06-pendencias/`](06-pendencias/) e [`02-arquitetura/`](02-arquitetura/).
