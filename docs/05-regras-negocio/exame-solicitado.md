# Regras de negócio — Exame solicitado

> Fonte: CLG-ESP-2026-001 §6

## Campos

| Campo | Observação |
|---|---|
| Paciente | Vínculo obrigatório |
| Exame do catálogo | Traz prazo e preço de referência |
| Origem | Cligen, Clínica Parceira, Site Cligen ou Plataforma — atributo do exame, não do paciente (D4) |
| Destino | Campo descritivo (D7) — ver ressalva abaixo |
| Médico solicitante | Interno ou externo (D7) |
| Data de entrada | Data do cadastro do exame |
| Preço | `decimal`, herdado do catálogo e editável |
| Anexos | Até 3 arquivos (Q22) |

## Médico solicitante

O único médico interno é **Dr. Arsonval Lamounier Junior** (Q13); é o nome que entra no modelo de comunicação de médico interno. O médico externo é digitado livremente a cada exame, sem cadastro e sem vínculo com clínica (Q14, Q15). Médicos não têm acesso ao sistema (Q16).

Pendente: CRM do Dr. Arsonval Lamounier Junior (Q13.1, não informado).

## Campo Destino — ressalva importante

Permanece descritivo nesta versão porque o dashboard foi diferido (Q38). Quando o indicador *"Destino por período"* entrar, texto livre não agregará — grafias diferentes viram destinos diferentes. A conversão para lista controlada e a higienização do já digitado está registrada como pendência (**Q38.1**).

## Anexos

Até 3 arquivos, sem nomeação semântica, porque o contratante ainda não especificou o que são (Q22). Formatos PDF, JPG e PNG, limite de 50 MB por arquivo (**P1**). Nenhum deles é visível ao paciente (Q23).

## Exclusão

O exame pode ser excluído em qualquer etapa, por qualquer funcionário Cligen (Q18). A exclusão é **lógica**: o registro sai das telas e dos números da operação, mas permanece armazenado com autor, data e motivo, sendo o motivo obrigatório (C2, **P4**, **P10**). O paciente nunca exclui.

Isso decorre da natureza de prontuário do registro (Q45): apagar fisicamente contrariaria a obrigação de guarda e esvaziaria o log de acesso.

> Nota: como os perfis internos foram diferidos, na v1 qualquer um dos 4 usuários da equipe pode excluir qualquer exame — ver risco **R3** em [pendências](../06-pendencias/riscos-aceitos.md).

## Implementação (premissas P22–P28)

Origem como enum; médico interno com nome fixo; preço herdado e editável; estado inicial 1 sem transições; anexos em disco local provisório (P27). Ver [`06-pendencias/premissas.md`](../06-pendencias/premissas.md).
