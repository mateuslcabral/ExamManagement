# Regras de negócio — Amostra, prazo e datas

> Fonte: CLG-ESP-2026-001 §7 · CLG-PEND-2026-001 §1.7, §1.8

## Regra de cálculo

A contagem do prazo começa no **acolhimento da amostra no laboratório**, não no cadastro (Q1).

```
data de liberação prevista = data de acolhimento da amostra
                           + prazo de execução do exame (dias corridos)
                           + 3 dias de revisão da Cligen
```

Os 3 dias de revisão valem para todos os exames, inclusive os executados internamente, e são armazenados como **parâmetro global editável** — a Cligen pode alterá-los sem nova versão do sistema (**P13**).

Enquanto a amostra não for acolhida, **a data de liberação não existe**. A tela de cadastro do exame exibe apenas o prazo em dias; a data aparece depois do acolhimento.

## As duas datas

| Campo | Quando é gravado | Muda depois? |
|---|---|---|
| `data_liberacao_prevista` | Uma vez, no acolhimento | **Não** |
| `data_liberacao_efetiva` | Ao acionar o botão de disponibilizar | Não se aplica |

A data prometida é uma **estimativa fixa**: não é recalculada quando o executor entrega antes ou depois do previsto (Q1.6.2). Atrasos e antecipações são absorvidos sem alterar o número anunciado.

É a diferença entre as duas datas que torna o atraso mensurável, e é dela que a futura ação de indicador de atraso (A7, diferida) se alimentará quando o dashboard entrar. Se a previsão fosse recalculada a cada evento, nenhum exame jamais apareceria atrasado.

Na primeira versão a data prevista **não é exibida ao paciente** (**P15**), coerente com a mensagem definida em [comunicação](comunicacao.md) (D12), que promete comunicação e não data.

## Rejeição e recoleta

A rejeição da amostra zera o prazo, e o exame retorna à condição de aguardando amostra. A recoleta, ao ser acolhida, reinicia o fluxo e gera nova previsão (Q19).

> **Pendente — Q19.1:** confirmar com o cliente a leitura exata desta regra (rejeição zera o prazo; recoleta reinicia o fluxo e recalcula a data).

## Pendente (bloqueia detalhamento da tela de acolhimento)

| ID | Pergunta | Impacto |
|---|---|---|
| Q1.1 | Quem registra o acolhimento e em qual tela — mesma pessoa do cadastro ou tela própria do laboratório? | Tela e perfil |
| Q1.2 | A data de acolhimento é automática ou digitável? (amostra que chega no sábado e é registrada na segunda encurta o prazo do paciente em dois dias) | Regra de cálculo |
| Q1.3 | Existe horário de corte — amostra recebida às 19h conta como do dia seguinte? | Regra de cálculo |
| Q1.4 | Uma coleta pode gerar vários exames do mesmo paciente, com um acolhimento alimentando N exames? | Modelo de dados — ver [schema](../04-schema/modelo-de-dados.md) |
| Q1.5 | Um exame pode exigir mais de uma amostra (sangue + saliva, ou trio familiar)? Se sim, o prazo conta da última. | Modelo de dados |
| Q1.7 | Na recoleta, o novo acolhimento recalcula a data de liberação do zero, reescrevendo a previsão que o paciente já viu? | Regra de negócio |

Premissa vigente enquanto essas não são respondidas: **P7** (dias corridos, sem horário de corte), **P8** (um acolhimento por exame).

## Implementação (premissas P29–P31)

Acolhimento na tela do exame com data digitável (padrão hoje); `Amostra` como histórico 1‑N com uma ativa; rejeição só no estado 2, recoleta = novo acolhimento. Ver [`06-pendencias/premissas.md`](../06-pendencias/premissas.md).
