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

## Implementação (16/10/2026)

- **Desconto** fica no exame, em reais, com motivo opcional; não pode exceder o preço. `valor a pagar = preço − desconto`.
- **Lançamentos** (`Pagamento`) são agregado próprio, N por exame, com autoria. Particular (Pix, cartões, dinheiro, boleto, transferência) **nasce recebido** na data informada (hoje por padrão) — o paciente quita no ato (Q26). **Convênio nasce a receber** (C4) e só entra no caixa quando marcado como recebido, na data em que o dinheiro chegou.
- **Convênio como texto livre** + campo opcional de guia/autorização, até C4.5/C4.2. O relatório agrupa pelo nome digitado — grafias diferentes viram convênios diferentes; promover a cadastro depois é migration simples.
- **Glosa** (P11): só em lançamento de convênio a receber, com motivo; sai da previsão, lançamento permanece.
- **Cartão parcelado** (Q27, C6): guarda número de parcelas e valor de cada uma (calculado se não informado), só como informação; o valor entra integral na data.
- **Limite:** o total lançado (recebido + a receber) não pode passar do valor a pagar; aumentar o desconto também respeita isso. Não existe saldo em aberto do paciente (Q26): a tela mostra "Falta lançar" até o saldo zerar.
- **Exclusão** de lançamento errado é lógica, com motivo (P4). Lançamentos de exame excluído somem do relatório e voltam ao restaurar.
- **Relatório `/financeiro`** por regime de caixa (Q29): recebidos pela data de recebimento no período (padrão: mês corrente), totais por método; a receber de convênio sem corte de data, totais por convênio. Sem emissão fiscal nem exportação.
- **Não implementado:** relatório por exame/paciente, exportação (CSV/Excel), caixa diário, baixa em lote de convênio.

## Reconhecimento de receita

O faturamento é apurado por **regime de caixa** — considera a data em que o dinheiro entrou, não a data do exame (Q29). Somente lançamentos com status `recebido` compõem o faturamento.
