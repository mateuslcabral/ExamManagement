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

## Fora de escopo (v1)

Laudo retificado e laudo complementar (Q17). Sem esse tratamento, corrigir um laudo já liberado significa substituir o arquivo — o paciente não é notificado da troca e não fica histórico da versão anterior. Ver risco **R2** em [pendências](../06-pendencias/riscos-aceitos.md).
