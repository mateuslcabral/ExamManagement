# Decisões fechadas por resolução de conflito (C1–C7)

> Fonte: CLG-PEND-2026-001 §1. Estas decisões já estão incorporadas à especificação (ver [`05-regras-negocio/`](../05-regras-negocio/)); este arquivo preserva o raciocínio/implicações de cada uma para referência de implementação.

## C1 — Identificação do paciente

A chave do paciente passa a ser um **identificador aleatório do tipo `uniqueidentifier` (GUID)**, sem relação com o CPF.

Implicações:

- O documento (CPF ou passaporte) deixa de ser chave, mas **mantém índice único** — sustenta a regra da mensagem *"Esse CPF já foi utilizado"*, válida para qualquer tipo de documento.
- O GUID é interno. O paciente faz login pelo número do documento cadastrado, nunca pelo GUID.
- **Ponto a decidir (D2.1):** a ata prometeu "um número de cadastro (ID) por paciente". Um GUID não é legível nem ditável por telefone. Recomendação: manter o GUID como chave interna e acrescentar um **número de cadastro sequencial curto** (ex.: `2026-000123`) para uso da recepção e do paciente. Custo desprezível, evita que a equipe leia 36 caracteres em voz alta. **Aguardando aprovação do cliente.**
- **Nota técnica:** GUID aleatório como PK agrupada fragmenta o índice. Implementado como PK não agrupada, com chave sequencial interna para ordenação física. Decisão de implementação, sem impacto funcional.

## C2 — Exclusão de exame

Exclusão lógica, permitida a **qualquer funcionário Cligen**. O paciente nunca exclui.

- Como os perfis internos foram diferidos, na v1 os 4 usuários da equipe podem excluir qualquer exame. Aceitável porque a exclusão é reversível e o registro guarda autor e data — anotado para revisão quando os perfis forem implementados.
- **Premissa P10:** motivo da exclusão obrigatório. Reverter para opcional se o cliente preferir.

## C3 — Retenção do laudo (cliente concordou)

Os 6 meses limitam a **exibição no portal**. O arquivo permanece armazenado e acessível à equipe pelo prazo legal de guarda. Redação obrigatória no termo de aceite (ver [conformidade](../05-regras-negocio/conformidade.md)).

## C4 — Convênio

**Decisão:** convênio registrado como **pagante**; valor entra no fluxo de caixa apenas quando o dinheiro é efetivamente recebido.

Implicações no modelo financeiro:

- Pagamento passa a ter **status** (`a receber`/`recebido`) e **data de recebimento**. Faturamento por regime de caixa considera apenas os recebidos.
- Flexibiliza a regra de "sem saldo em aberto do paciente": não existe pagamento parcial do paciente, mas passa a existir valor a receber de convênio — coisas distintas.
- Coparticipação resolvida sem regra nova: dois lançamentos no mesmo exame (convênio `a receber` + cartão do paciente `recebido`).
- **Premissa P11:** recusa de pagamento pelo convênio (glosa) tratada como status `glosado`, que retira o valor da previsão sem apagar o lançamento. **Confirmar com o cliente.**

Segue em aberto: **C4.2** (número de guia/autorização) e **C4.5** (convênio como cadastro ou texto livre) — ver [pendências abertas](pendencias-abertas.md).

## C5 — Desconto

Confirmado: `valor pago = valor do exame − desconto`, desconto **sempre em reais** (o sinal negativo era só a forma como o cliente escreveu).

**Premissa P12:** motivo do desconto campo livre e opcional. Elevar a obrigatório se a direção quiser controle sobre concessões.

## C6 — Cartão parcelado (cliente confirmou)

Venda parcelada reconhecida integralmente na data da transação. Parcelas são informação, não afetam status de pagamento.

## C7 — Texto da mensagem de cadastro

Textos-base da Cligen incluíam mensagem de credenciais com usuário/senha numéricos. Com o acesso por código, essa mensagem deixa de existir e precisa ser reescrita como boas-vindas. **Rascunho a propor junto com os protótipos (A8).**
