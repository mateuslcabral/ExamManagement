# Regras de negócio — Financeiro

> Fonte: CLG-ESP-2026-001 §9 · CLG-PEND-2026-001 §1.4, §1.5, §1.6

O módulo é **registro interno**, sem emissão fiscal e sem integração contábil (Q30). Não há gateway de pagamento: todo lançamento é manual, feito pela equipe (Q31).

## Valores

```
valor pago = valor do exame − desconto
```

O desconto é sempre em **reais**, nunca percentual (C5). O motivo do desconto é campo livre e opcional (**P12**).

## Pagamentos

Cada exame admite **N lançamentos de pagamento**, o que permite combinar métodos livremente — parte em cartão e parte em dinheiro, por exemplo (Q25).

Métodos aceitos: Pix, cartão de crédito, cartão de débito, dinheiro, boleto, transferência e convênio.

| Campo do lançamento | Observação |
|---|---|
| Método | Conforme lista acima |
| Valor | `decimal` |
| Status | `a receber` · `recebido` · `glosado` |
| Data de recebimento | Preenchida quando o status vira `recebido` |
| Parcelas | Quando cartão parcelado |

## Cartão parcelado

Registram-se o valor total e o valor de cada parcela (Q27), mas a venda é reconhecida **integralmente na data da transação**. As parcelas são informação, não afetam o status de pagamento (C6, **P3**).

## Convênio

O convênio é registrado como **pagante**, e o valor entra no fluxo de caixa **apenas quando o dinheiro é efetivamente recebido** (C4). Enquanto isso, o lançamento fica com status `a receber`.

A recusa de pagamento pelo convênio é tratada pelo status `glosado`, que retira o valor da previsão sem apagar o lançamento (**P11**).

**Coparticipação** não exige regra nova: são dois lançamentos no mesmo exame, um de convênio como `a receber` e outro do paciente como `recebido`.

**Não existe saldo em aberto do paciente** (Q26). O paciente particular quita no ato; o que pode ficar pendente é o valor a receber de convênio, que é outra coisa.

> **Pendente — C4.5:** convênio vira **cadastro** (lista de operadoras, permitindo saber quanto cada uma deve) ou apenas texto digitado no pagamento? Recomendação registrada: cadastro — sem ele não há como responder "quanto a Unimed tem a pagar". Impacta diretamente o [modelo de dados](../04-schema/modelo-de-dados.md) (entidade `Convenio`).
>
> **Pendente — C4.2:** é necessário registrar número de guia ou autorização do convênio?

## Reconhecimento de receita

O faturamento é apurado por **regime de caixa** — considera a data em que o dinheiro entrou, não a data do exame (Q29). Somente lançamentos com status `recebido` compõem o faturamento.
