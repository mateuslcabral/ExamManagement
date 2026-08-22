# Premissas em vigor

> Fonte: CLG-PEND-2026-001 §3. Premissas técnicas adotadas na ausência de resposta do cliente ou por decisão de implementação — reversíveis se o cliente/direção preferir outro caminho.

| ID | Premissa | Origem |
|---|---|---|
| P1 | Limite de 50 MB por arquivo; formatos PDF, JPG e PNG | Q24 sem resposta |
| P2 | Dimensionamento para 200 exames/mês e 4 usuários simultâneos | Q46 |
| P3 | Venda parcelada reconhecida integralmente na data da transação | C6 confirmado |
| P4 | Exclusão de exame é lógica, com autor, data e motivo preservados | C2 |
| P5 | Os 6 meses limitam a visibilidade no portal, não a retenção | C3 confirmado |
| P6 | Chave do paciente é GUID aleatório; documento com índice único; login pelo documento + código de uso único | C1 |
| P7 | Prazo em dias corridos a partir do acolhimento, sem horário de corte | Q2, Q1.3 pendente |
| P8 | Um acolhimento por exame, até resposta de Q1.4 e Q1.5 | Q1.4, Q1.5 pendentes |
| P9 | Menor de idade e paciente sem contato próprio: e-mail e telefone são os do responsável legal, destinatário dos disparos | Q7, Q10 |
| P10 | Motivo da exclusão obrigatório | C2 |
| P11 | Glosa tratada como status `glosado` do lançamento | C4 |
| P12 | Motivo do desconto livre e opcional | C5 |
| P13 | Os 3 dias de revisão valem para todos os exames e são parâmetro global editável | Q1.6 |
| P14 | O exame carrega duas datas independentes — prevista (fixa, gravada no acolhimento) e efetiva (gravada na disponibilização) | Q1.6.2 |
| P15 | A data prevista não é exibida ao paciente na v1 | Q1.6.3 pendente |

**Nota sobre P2 e a guarda de 20 anos (ação A6):** a 200 exames/mês com laudo médio de 10 MB, o acúmulo é de cerca de 24 GB/ano, ou ~480 GB ao fim de 20 anos. O custo de armazenamento é irrelevante nesse volume. O risco real da retenção longa não é financeiro, é de continuidade — nenhum contrato de nuvem dura 20 anos. Ver [armazenamento e retenção](../02-arquitetura/armazenamento-e-retencao.md).
