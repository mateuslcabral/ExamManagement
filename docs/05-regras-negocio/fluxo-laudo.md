# Regras de negócio — Fluxo do laudo

> Fonte: CLG-ESP-2026-001 §8

## Estados

| # | Estado | Como avança |
|---|---|---|
| 1 | Cadastrado — aguardando amostra | Registro do acolhimento |
| 2 | Amostra acolhida | Previsão calculada; em execução |
| 3 | Laudo Parceiro Pronto | Upload do arquivo, data automática |
| 4 | Laudo Cligen para revisão | Upload do arquivo, data automática |
| 5 | Laudo revisado | Upload do arquivo, data automática |
| 6 | Disponibilizado ao paciente | Botão manual da equipe |

Cada uma das três etapas de laudo registra data automaticamente e comporta upload do respectivo arquivo (D9). O arquivo pode ser **substituído** caso tenha sido enviado por engano (Q20). A etapa *Laudo Parceiro Pronto* está sempre presente, mesmo em exame executado internamente (Q21).

> Nota: não há, ainda, uma matriz formal de transições permitidas (ex.: em quais estados a rejeição de amostra é possível; se a substituição de arquivo após a etapa 6 dispara nova notificação). Ver [pendências de arquitetura](../06-pendencias/pendencias-abertas.md).

## Disponibilização

A disponibilização ao paciente é **ato manual**, acionado por botão dedicado (D10). O laudo revisado pode permanecer no sistema indefinidamente sem estar visível ao paciente.

Ao acionar o botão, o sistema:

1. Grava a `data_liberacao_efetiva`.
2. Torna o laudo revisado visível no portal.
3. Dispara e-mail e WhatsApp ao paciente, no modelo correspondente ao tipo de médico solicitante (D11, D13) — ver [comunicação](comunicacao.md).

## Implementação (17/09/2026)

Matriz de transições adotada — **confirmar com o cliente** (pendência "matriz de transições" em [pendências](../06-pendencias/pendencias-abertas.md)):

| De | Para | Como | Regra |
|---|---|---|---|
| 1 Aguardando amostra | 2 Amostra acolhida | Acolhimento | Ver [amostra e prazo](amostra-e-prazo.md) |
| 2 | 1 | Rejeição da amostra | Só neste estado; com motivo |
| 2 → 3 → 4 → 5 | Etapa seguinte | Upload do PDF da etapa | **Sem pular etapa.** Data gravada no primeiro envio, nunca alterada |
| 3, 4, 5 ou 6 | (mesmo estado) | Substituir arquivo de etapa já registrada (Q20) | Não muda data nem estado; conta as substituições, com autor e data da última |
| 5 Laudo revisado | 6 Disponibilizado | Botão manual (D10) | Grava `data_liberacao_efetiva`; dispara e-mail e WhatsApp no modelo do médico (D13) |
| 6 | — | — | Não há volta. Substituir o laudo revisado depois disso **é permitido e não avisa o paciente** (R2); a tela pede confirmação |

Outras decisões:

- Só **PDF**, conferido pelos primeiros bytes do arquivo (não pelo nome). Até 50 MB (P1). Gravado sem nenhuma transformação (Q44).
- O arquivo substituído **não é apagado** do armazenamento: fica sem referência. Coerente com "nada do prontuário é removido"; sem tela para consultá-lo.
- A etapa *Laudo Parceiro Pronto* é obrigatória mesmo em exame interno (Q21): não há atalho.
- Cada etapa guarda quem registrou; a substituição guarda quem substituiu e quando.
- As mensagens de disponibilização ainda só vão para o log (provedores e textos pendentes). O envio é síncrono, sem fila.
- A **data prevista não é apagada** na disponibilização: a diferença entre prevista e efetiva é o que mede o atraso (A7).
- Fila `/laudos`: exames dos estados 2 a 5 agrupados pela próxima ação; data prevista já passada aparece em destaque (não é o alarme de A7, só exibição).

## Fora de escopo (v1)

Laudo retificado e laudo complementar (Q17). Sem esse tratamento, corrigir um laudo já liberado significa substituir o arquivo — o paciente não é notificado da troca e não fica histórico da versão anterior. Ver risco **R2** em [pendências](../06-pendencias/riscos-aceitos.md).
