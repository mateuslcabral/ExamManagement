# Regras de negócio — Comunicação automática

> Fonte: CLG-ESP-2026-001 §11

| Momento | Gatilho | Conteúdo |
|---|---|---|
| Cadastro do paciente | Automático (Q32) | Boas-vindas com endereço do portal e instruções de acesso — **texto a redefinir**, ver pendência C7 |
| Laudo disponibilizado | Botão manual (D10) | Aviso de laudo disponível, em um de dois modelos |

## Dois modelos de aviso de laudo (D13)

- **Médico interno** — inclui convite para agendamento de retorno com o Dr. Arsonval Lamounier Junior e menção ao aconselhamento genético.
- **Médico externo** — versão simplificada.

## Canais

- **WhatsApp** pela API oficial, com mensagens em template previamente aprovado; a Cligen já possui a conta (Q36).
- **E-mail transacional**, com domínio de envio a definir (Q37).

> Nota de planejamento: templates de WhatsApp precisam de aprovação prévia da Meta antes de entrarem em produção — isso tem lead time e deveria ser iniciado assim que os textos estiverem prontos, não deixado para o fim do desenvolvimento.

## Pendência C7

Os textos-base fornecidos pela Cligen incluem uma mensagem de credenciais com usuário e senha numéricos. Com a adoção do acesso por código de uso único, essa mensagem deixa de existir e precisa ser **reescrita como boas-vindas**. Rascunho a ser proposto junto com os protótipos (ação A8).

## Risco aceito

As mensagens automáticas citam o nome do exame (ex.: *"Sequenciamento Completo do Exoma"*), fazendo dado de saúde trafegar por e-mail e WhatsApp. Recomendada a remoção; o cliente optou por manter (Q35) — ver risco **R1** em [pendências](../06-pendencias/riscos-aceitos.md).
