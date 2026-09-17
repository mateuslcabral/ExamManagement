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

## Implementação (16/09/2026)

- O exame nasce em **Cadastrado — aguardando amostra**, com data de entrada automática (data de Brasília). Acolhimento da amostra e fluxo do laudo implementados — ver [amostra e prazo](amostra-e-prazo.md) e [fluxo do laudo](fluxo-laudo.md).
- **Preço** herdado do preço de referência do catálogo ao escolher o exame, editável. Só exames **ativos** do catálogo podem ser escolhidos.
- **Destino** opcional (texto livre). **Médico externo** exige nome; para o interno, o nome é fixo.
- **Edição:** o paciente não muda — exame lançado no paciente errado se exclui (com motivo) e se cadastra de novo, preservando o rastro. O exame do catálogo só pode ser trocado **antes do acolhimento**, porque o prazo dele alimenta a data prevista (P14).
- **Anexos:** até 3 ativos; formato conferido pelos **primeiros bytes** do arquivo (não pelo nome), 50 MB cada. Guardam nome original, tipo, tamanho, SHA-256, quem enviou e quando. "Remover" é lógico: libera a vaga, mas o arquivo e o registro permanecem.
- **Exclusão:** lógica, com autor, data e motivo obrigatório; o exame some das listas e das consultas da API (404). A lista **Exames excluídos** (`/exames?excluidos=1`) mostra motivo e data e permite **restaurar** (C2: reversível). Ao restaurar, autor, data e motivo da exclusão desfeita não ficam guardados.

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
