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
| P16 | Exclusão de **paciente** é lógica, com autor, data e motivo obrigatório — mesma natureza de prontuário do exame. O paciente excluído sai das listagens e pode ser restaurado; o documento permanece no índice único, então recadastrar o mesmo CPF continua bloqueado (*"Esse CPF já foi utilizado"*) — o caminho é restaurar. A exclusão será bloqueada enquanto houver exame ativo vinculado (regra entra junto com o módulo de exames) | Q45, C2, P4 — adotada em 22/08/2026 |
| P17 | O número sequencial interno (`Seq`) já é gravado desde a primeira migration de `Paciente`; o número de cadastro legível (D2.1) será só formatação sobre ele e **não é exibido** até aprovação do cliente | D2.1 pendente |
| P18 | Tipo e número do documento são editáveis após o cadastro (correção de digitação), mantendo a validação de unicidade. CPF é validado pelos dígitos verificadores e armazenado só com dígitos; passaporte é alfanumérico livre (6–20 caracteres), armazenado em maiúsculas | Q8, Q11 |
| P19 | Maioridade aos 18 anos completos na data de referência. Responsável legal obrigatório para menor; e-mail e telefone do cadastro são os do responsável (P9), sem campos de contato duplicados na entidade `ResponsavelLegal` (nome, documento e grau de parentesco) | Q7, P9 |
| P20 | A mensagem de boas-vindas do cadastro (Q32) é disparada pelas interfaces `IEnvioEmail`/`IEnvioWhatsApp` com texto provisório, até C7 definir o texto e Q37/Q36 definirem os provedores — implementações atuais só registram em log | Q32, C7, Q36, Q37 |
| P21 | Telefone armazenado apenas com dígitos (DDI opcional + DDD + número, 10–13 dígitos), para servir de destino WhatsApp sem nova normalização | Q10, Q36 |
| P22 | **Origem** do exame é enum fechado (Cligen, Clínica Parceira, Site Cligen, Plataforma); **Destino** segue texto livre até Q38.1 | D4, Q38.1 |
| P23 | Médico solicitante: tipo `Interno`/`Externo` + nome. No interno o nome é fixado em "Dr. Arsonval Lamounier Junior" e o CRM fica nulo até Q13.1; no externo o nome é digitado livremente, sem cadastro | Q13–Q15 |
| P24 | Catálogo nasce com 3 exames de exemplo (seed) até a lista oficial ser fornecida; itens inativos não aparecem para seleção em novo exame, mas exames já vinculados os preservam | Q3 |
| P25 | Exame só pode ser criado/editado para paciente não excluído; paciente com exame ativo (não excluído) não pode ser excluído | P16 |
| P26 | O exame nasce no estado 1 (Cadastrado — aguardando amostra) e só sai dele pelo módulo de acolhimento; a matriz formal de transições segue pendente. Preço é herdado do catálogo no cadastro e editável depois | P14, fluxo-laudo |
| P27 | **Armazenamento de anexos provisório em disco local** do servidor da API (`Armazenamento:DiretorioBase`), atrás de `IArmazenamentoArquivos`. Binário fora do banco, com caminho, tipo, tamanho e hash SHA-256 registrados em `Anexo`. Trocar por object storage sem tocar na Aplicação quando a hospedagem for definida. Remoção de anexo (enviado por engano) é física | Q22, armazenamento-e-retencao |
| P28 | Os 3 dias de revisão (P13) vivem na tabela `Parametro` (chave `DiasRevisao`), editável pela equipe | P13 |
| P29 | **Acolhimento** registrado na própria tela do exame, por qualquer usuário da equipe (Q1.1). Data de acolhimento **digitável, com padrão = hoje**, não futura e não anterior à data de entrada do exame (Q1.2). Sem horário de corte (P7) | Q1.1, Q1.2, Q1.3 |
| P30 | `Amostra` é 1‑N por exame como **histórico**: só uma fica ativa (acolhida). A previsão = data de acolhimento + prazo de execução + dias de revisão vigentes, gravada uma vez (P14). Uma coleta que alimente N exames (Q1.4) vira N acolhimentos com a mesma data | Q1.4, Q1.5, P8, P14 |
| P31 | **Rejeição** só no estado 2 (amostra acolhida, antes de qualquer laudo). Exige motivo; a amostra fica `Rejeitada` no histórico, o exame volta ao estado 1 e a previsão é zerada. A **recoleta** é um novo acolhimento, que recalcula a previsão do zero (leitura de Q19 — **a confirmar, Q19.1/Q1.7**) | Q19, Q19.1, Q1.7 |
| P32 | **Matriz de transições** mínima: 1→2 acolhimento · 2→3, 3→4, 4→5 pelo upload da etapa correspondente, sem pular etapa · 5→6 botão "Disponibilizar ao paciente" · 2→1 rejeição. O arquivo de uma etapa já registrada pode ser **substituído** em qualquer estado posterior sem alterar estado nem data da etapa; após o estado 6 a substituição **não notifica** (risco R2). Exame excluído não transita | fluxo-laudo, Q20, Q21, R2 |
| P33 | Arquivos das etapas do laudo: **somente PDF**, até 50 MB (P1), gravados sem qualquer transformação (integridade da assinatura — conformidade). Mesmo `IArmazenamentoArquivos` dos anexos (disco local provisório, P27). Só o arquivo da etapa 5 (laudo revisado) será exposto ao portal | Q44, P1, P27 |
| P34 | Ao disponibilizar, o sistema grava `data_liberacao_efetiva` e dispara e‑mail + WhatsApp no modelo do tipo de médico (interno com convite de retorno/aconselhamento; externo simplificado — D13), com **texto provisório** e envio em log até os provedores/templates existirem | D10, D11, D13, C7 |

**Nota sobre P2 e a guarda de 20 anos (ação A6):** a 200 exames/mês com laudo médio de 10 MB, o acúmulo é de cerca de 24 GB/ano, ou ~480 GB ao fim de 20 anos. O custo de armazenamento é irrelevante nesse volume. O risco real da retenção longa não é financeiro, é de continuidade — nenhum contrato de nuvem dura 20 anos. Ver [armazenamento e retenção](../02-arquitetura/armazenamento-e-retencao.md).
