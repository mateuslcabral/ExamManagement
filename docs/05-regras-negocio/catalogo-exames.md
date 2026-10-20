# Regras de negócio — Catálogo de exames

> Fonte: CLG-ESP-2026-001 §5

## Regras

- Catálogo com **CRUD completo** mantido pela própria equipe, sem dependência de carga de planilha. As primeiras versões trazem exames pré-carregados para uso imediato (Q3).
- O prazo é **fixo por exame**, sem variação por parceiro executor (Q4).
- O prazo é expresso em **dias corridos** (Q2).

## Semântica do campo de prazo — atenção

O prazo cadastrado é o **prazo de entrega ao paciente**, em dias corridos, **já incluindo a revisão da Cligen**. Decisão do cliente em 17/10/2026: os operadores já embutem o tempo de revisão no prazo que informam, então o sistema não soma nada por fora — a previsão de liberação é acolhimento + prazo do catálogo (ver [amostra e prazo](amostra-e-prazo.md)). O rótulo na tela é *"prazo de entrega (dias corridos)"*.

> Até 17/10/2026 o campo era o "prazo de execução" e o sistema somava 3 dias de revisão (parâmetro global, **P13**). A premissa P13 foi derrubada e o parâmetro removido — ver [premissas](../06-pendencias/premissas.md).

## Campos

| Campo | Observação |
|---|---|
| Nome do exame | Único (sem diferenciar maiúsculas/minúsculas) |
| Prazo de entrega (dias corridos) | `PrazoDias`, fixo por exame, já inclui a revisão |
| Preço de referência | `decimal`; pré-preenchido no exame |
| Preço limite | `decimal`; **piso para desconto**: após o desconto, o valor a pagar do exame não pode ficar abaixo dele. Deve ser ≤ preço de referência |
| Número de genes | inteiro ≥ 0, opcional |
| Genes | Texto livre (até 4000) — o cliente confirmou que "quais genes" é texto, não lista |
| Destino padrão | Laboratório executor padrão (cadastro de [destinos](exame-solicitado.md#cadastros-auxiliares)); pré-preenchido no exame e trocável lá |
| Informações técnicas | Texto livre (até 4000) |
| Ativo | Permite aposentar exame sem apagá-lo do histórico |

## Implementação (16/09/2026, revisada em 19/10/2026)

- Nome do exame **único** (sem diferenciar maiúsculas/minúsculas) — decisão de implementação para evitar duplicidade no catálogo; não havia regra explícita.
- Preço de referência ≥ 0, com no máximo 2 casas decimais; preço limite ≥ 0 e ≤ preço de referência; prazo ≥ 1 dia.
- Sem exclusão: exame aposentado é **desativado** e pode ser reativado.
- O catálogo guarda **autoria** (quem criou, quem alterou por último).
- **Duas telas:** `/catalogo` é **visão de consulta** (confirmado pelo cliente em 17/10/2026) — cards com nome, prazo, destino padrão, preços, nº de genes, genes e informações técnicas de todos os exames oferecidos; os inativos ficam numa seção separada. O CRUD fica em `/catalogo/gerenciar`.
- Alterar o prazo no catálogo não recalcula a data prevista de exames já acolhidos (**P14**).
- Removidos em 19/10/2026: a tabela `Parametro`, o endpoint `/api/parametros`, o DTO de dias de revisão e o formulário correspondente na tela do catálogo. `PrazoExecucaoDias` passou a chamar-se `PrazoDias`; `PrazoEntregaDias` (calculado) deixou de existir.

## Resolvidas

- **Q1.6.1** — resolvida em 17/10/2026: não existe "dia de revisão" separado; o prazo de cada exame já inclui a revisão. P13 derrubada.

## Pendente

- **Lista de exames pré-carregados** — precisa ser fornecida. Enquanto isso, a API semeia 3 exames de exemplo com prazos e preços ilustrativos quando o catálogo está vazio (`SeedCatalogo:Exemplos`, desligável).
