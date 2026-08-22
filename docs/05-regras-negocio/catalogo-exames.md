# Regras de negócio — Catálogo de exames

> Fonte: CLG-ESP-2026-001 §5

## Regras

- Catálogo com **CRUD completo** mantido pela própria equipe, sem dependência de carga de planilha. As primeiras versões trazem exames pré-carregados para uso imediato (Q3).
- O prazo é **fixo por exame**, sem variação por parceiro executor (Q4).
- O prazo é expresso em **dias corridos** (Q2).

## Semântica do campo de prazo — atenção

O prazo cadastrado é o **prazo de execução**, não o prazo total até o paciente. O rótulo na tela é *"prazo de execução (dias)"*, e ao lado dele o sistema exibe a previsão de entrega ao paciente já calculada (soma dos 3 dias de revisão — ver [amostra e prazo](amostra-e-prazo.md)).

Sem essa distinção, um exame cadastrado com 30 dias seria anunciado como 30 quando o paciente receberá em 33 (Q1.6).

## Campos

| Campo | Observação |
|---|---|
| Nome do exame | |
| Prazo de execução (dias corridos) | Fixo por exame |
| Preço de referência | `decimal` |
| Ativo | Permite aposentar exame sem apagá-lo do histórico |

## Pendente

- **Q1.6.1** — os 3 dias de revisão são iguais para todos os exames, ou algum tipo tem revisão mais longa? Se variar por exame, vira campo do catálogo em vez de parâmetro global (assumido global por ora — **P13**).
- **Lista de exames pré-carregados** — precisa ser fornecida/confirmada antes do seed inicial.
