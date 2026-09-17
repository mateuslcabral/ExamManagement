# Para Rafael — decisões tomadas, dúvidas e dilemas

> Situação em 17/09/2026. Escrito por Mateus.

Rafael, este documento reúne tudo que foi decidido durante a construção do sistema da Cligen nos pontos em que a especificação não dizia o que fazer, e tudo que continua em aberto. Preciso que você leia e me diga onde concorda, onde discorda e quem responde o que eu não sei responder.

Os códigos entre parênteses (Q1.2, C4.5, P14…) são os mesmos dos documentos formais do projeto: `Q` é resposta do questionário, `C` é resolução de conflito, `P` é premissa que adotamos sem resposta do cliente.

## Como ler

- **Parte 1:** decisões que mudam o que a equipe da Cligen vê ou pode fazer. Precisam de um "ok" ou de uma correção.
- **Parte 2:** decisões técnicas. Ficam registradas; só preciso de resposta se você discordar.
- **Parte 3:** perguntas que só o cliente ou você podem responder.
- **Parte 4:** dilemas, onde há dois caminhos razoáveis e eu escolhi um provisoriamente.
- **Parte 5:** o que eu preciso de volta, em ordem de urgência.

## O que já está pronto

Login por e-mail e senha, gestão de usuários, catálogo de exames, cadastro de pacientes, cadastro de exame solicitado com anexos e exclusão, e acolhimento de amostra com cálculo da data prevista. Ainda não existem o fluxo do laudo, o financeiro e o portal do paciente.

Tudo foi testado por mim na API e nas telas, mas ninguém da Cligen usou o sistema ainda.

---

## Parte 1 — Decisões que afetam a operação

### Catálogo de exames

| # | Decisão | Por quê | Se a resposta for outra |
|---|---|---|---|
| 1.1 | Não podem existir dois exames com o mesmo nome. "Exoma" e "exoma" contam como iguais. | Evita o mesmo exame cadastrado duas vezes com preços diferentes. | Mudança pequena. |
| 1.2 | Exame do catálogo nunca é apagado, só desativado. Desativado não aparece para novas solicitações e pode ser reativado. | Os exames já realizados precisam continuar apontando para ele. | Não recomendo mudar. |
| 1.3 | A tela mostra o "prazo de execução" e, ao lado, a entrega ao paciente já somada com os dias de revisão (ex.: 30 + 3 = 33). | A especificação pede isso para ninguém prometer 30 dias quando são 33. | — |
| 1.4 | Os 3 dias de revisão podem ser alterados pela equipe, na tela do catálogo. Mudar esse número **não altera** a data já prometida de exames com amostra acolhida. | Premissas P13 e P14. | Se a revisão variar por tipo de exame (Q1.6.1), vira um campo do catálogo. Mudança média. |

### Pacientes

| # | Decisão | Por quê | Se a resposta for outra |
|---|---|---|---|
| 1.5 | Do responsável legal pedimos nome, documento (CPF ou passaporte) e parentesco (opcional). | A especificação diz que o responsável existe, mas não diz quais dados dele guardar. | Acrescentar campo é simples. **Preciso confirmar com o cliente.** |
| 1.6 | O e-mail e o telefone do cadastro de um menor são os do responsável. Não há contato separado do paciente. | Premissa P9. | Mudança média. |
| 1.7 | Paciente maior de idade também pode ter responsável (opcional). | Cobre o idoso ou a pessoa sem e-mail próprio, que P9 menciona. | Basta tirar a opção. |
| 1.8 | Menor de idade é quem tem menos de 18 anos na data de Brasília. No dia do aniversário de 18 já é maior. | Critério objetivo. | — |
| 1.9 | O CPF é validado pelos dígitos verificadores. CPF digitado errado é recusado na hora. | Evita cadastro com documento inválido, que depois impediria o login no portal. | — |
| 1.10 | Passaporte aceita de 5 a 20 letras e números. | Não existe formato único de passaporte no mundo. | — |
| 1.11 | Telefone sem "+" é tratado como brasileiro e precisa de DDD. Número estrangeiro precisa começar com "+" e o código do país. | O WhatsApp exige o número em formato internacional. | — |
| 1.12 | Todos os dados do paciente podem ser corrigidos depois, inclusive o documento. A tela avisa que o documento é o login do portal. | Erro de digitação acontece. | Se o documento não puder mudar, a correção passa a exigir outro caminho. |
| 1.13 | Paciente não pode ser excluído. | É prontuário. A especificação só fala em exclusão de exame. | **Dúvida 3.9.** |
| 1.14 | A mensagem de cadastro duplicado é "Esse CPF já foi utilizado" ou "Esse passaporte já foi utilizado". | O cliente definiu a frase só para CPF (Q11). | — |

### Exame solicitado

| # | Decisão | Por quê | Se a resposta for outra |
|---|---|---|---|
| 1.15 | O preço vem do catálogo ao escolher o exame e pode ser alterado. | Está na especificação. | — |
| 1.16 | O paciente de um exame não pode ser trocado. Se foi lançado no paciente errado, exclui-se com motivo e cadastra-se de novo. | Fica o rastro do erro, o que importa num prontuário. | Permitir a troca é simples, mas perde-se o rastro. |
| 1.17 | O exame do catálogo só pode ser trocado antes de a amostra ser acolhida. | Depois disso, o prazo daquele exame já entrou na data prometida. | — |
| 1.18 | "Destino" é opcional. Médico externo exige o nome digitado. | A especificação trata o destino como descritivo. | — |
| 1.19 | Anexos: o sistema confere o formato pelo conteúdo do arquivo. Um arquivo renomeado para ".pdf" é recusado. | O nome do arquivo é controlado por quem envia. | — |
| 1.20 | "Remover" um anexo só o tira da tela e libera a vaga. O arquivo continua guardado. | Nada do prontuário é apagado. | — |
| 1.21 | Exame excluído some de todas as telas. **Ainda não existe tela para desfazer a exclusão nem para consultar os excluídos.** | Não deu tempo. O documento C2 diz que a exclusão é reversível. | **Dilema 4.5.** |

### Acolhimento de amostra

Estas são as mais importantes, porque respondem por conta própria perguntas que o cliente ainda não respondeu (Q1.1 a Q1.7).

| # | Pergunta pendente | O que implementei | Se a resposta for outra |
|---|---|---|---|
| 1.22 | Q1.1 — quem registra e em que tela? | Qualquer usuário. Há uma fila "Amostras" com os exames aguardando, e também dá para registrar dentro do exame. | Se for uma tela só do laboratório, depende dos perfis de acesso, que estão fora da v1. |
| 1.23 | Q1.2 — a data é automática ou digitada? | Digitada, com "hoje" já preenchido. Não pode ser futura nem anterior ao cadastro do exame. O sistema guarda separadamente quando e quem lançou. | Se for automática, é só travar o campo. |
| 1.24 | Q1.3 — há horário de corte? | Não. Vale a data informada (P7). | Com horário de corte, passamos a pedir hora também. Mudança média. |
| 1.25 | Q1.4 e Q1.5 — várias amostras por exame, ou uma coleta para vários exames? | Uma amostra por exame (P8). O banco já foi desenhado para aceitar mais de uma sem perda de dados. | Mudança de tela e de cálculo, sem retrabalho no banco. |
| 1.26 | Q1.7 e Q19.1 — o que acontece na rejeição e na recoleta? | A rejeição exige motivo, apaga a data prevista e o exame volta a aguardar amostra. A amostra rejeitada fica no histórico. O acolhimento seguinte aparece marcado como "recoleta" e gera data nova. | Se a data original tiver de ser mantida, é outra regra. **Dilema 4.4.** |
| 1.27 | (sem código) — até quando se pode rejeitar? | Só enquanto o exame está em "amostra acolhida", antes de qualquer laudo. | Depende da matriz de transições, que ainda não existe (dúvida 3.7). |
| 1.28 | P14 — a data prevista muda? | Nunca. É gravada uma vez no acolhimento. Mudar o catálogo ou os dias de revisão depois não a altera. A amostra guarda os números usados no cálculo. | Não recomendo mudar: sem isso nenhum exame apareceria atrasado. |

### Usuários

| # | Decisão | Por quê |
|---|---|---|
| 1.29 | Ninguém consegue desativar a si mesmo. | Evita a equipe ficar sem acesso por um clique. |
| 1.30 | Usuário desativado perde o acesso na hora, mesmo que esteja com o sistema aberto. | É prontuário. Antes ele continuaria dentro por até 8 horas. |
| 1.31 | Senha com no mínimo 8 caracteres, sem outras regras. | Provisório. **Dúvida 3.5.** |

---

## Parte 2 — Decisões técnicas (para registro)

| # | Decisão | Motivo | Quando rever |
|---|---|---|---|
| 2.1 | O sistema interno avisa à API quem é o usuário logado por um cabeçalho simples (`X-Usuario-Id`), sem assinatura. | Só o nosso servidor conhece a chave de acesso à API. Quem tivesse a chave poderia falsificar uma assinatura também, então assinar não acrescentaria proteção. | Se a API passar a ter outro consumidor além do sistema interno. |
| 2.2 | Todo cadastro guarda quem criou e quem alterou por último. Exclusão de exame, rejeição de amostra e envio ou remoção de anexo guardam autor e data. | Exigência de prontuário. | — |
| 2.3 | O catálogo de exames **não** guarda autor. | Foi feito antes de a API conhecer o usuário. | Posso acrescentar, é pequeno. |
| 2.4 | Os arquivos ficam em disco, na máquina que roda a API, numa pasta fora do controle de versão. | Decidi manter local por enquanto, até escolhermos o provedor. Trocar depois não mexe nas regras de negócio. | Antes de qualquer dado real. **Não há cópia de segurança.** |
| 2.5 | Cada arquivo guarda uma impressão digital (SHA-256). Os bytes são gravados sem nenhuma alteração. | Laudo assinado não pode ser reprocessado, e a impressão digital permite provar que o acervo está íntegro numa migração. | — |
| 2.6 | O upload de arquivos usa um caminho próprio no sistema interno. | O Next.js corta, sem avisar, qualquer envio acima de 10 MB no caminho padrão. Um laudo de 30 MB chegaria corrompido. Testei com 15 MB e 40 MB. | — |
| 2.7 | As mensagens de boas-vindas (e-mail e WhatsApp) são "disparadas" no cadastro do paciente, mas hoje só ficam escritas no log do servidor. **Nada é enviado.** | Não há provedor contratado nem texto aprovado. | Quando 3.3 e 3.4 forem resolvidas. |
| 2.8 | O envio das mensagens acontece junto com o cadastro, sem fila. | Não há decisão sobre processamento em segundo plano. | **Dilema 4.3.** |
| 2.9 | Datas de calendário (menoridade, entrada do exame, acolhimento) usam o fuso de Brasília. | O servidor pode estar em outro fuso. | — |
| 2.10 | A API responde "não encontrado" para exame excluído. | A exclusão tira o exame de todas as telas (C2). | Junto com o dilema 4.5. |

---

## Parte 3 — Dúvidas que eu não consigo responder

### Bloqueiam o próximo módulo

| # | Dúvida | O que trava | Quem responde |
|---|---|---|---|
| 3.1 | **C4.5** — convênio é um cadastro de operadoras ou um texto digitado? Recomendo cadastro: sem ele não dá para saber quanto cada operadora deve. | O módulo financeiro inteiro. | Cliente |
| 3.2 | **C4.2** — é preciso registrar número de guia ou de autorização? | Tela de pagamento. | Cliente |
| 3.3 | **Q37** — qual e-mail e domínio de envio, e quem tem acesso ao DNS de `cligen.com.br`? Qual provedor de e-mail? | Nenhuma mensagem sai do sistema até isso existir. | Eu levanto o DNS. A escolha do provedor é nossa. |
| 3.4 | **C7** — texto da mensagem de boas-vindas e dos dois avisos de laudo. Os modelos de WhatsApp precisam ser aprovados pela Meta, e isso demora. | Disponibilização do laudo. | Eu proponho o rascunho; o cliente aprova. |

### Bloqueiam a entrada em produção

| # | Dúvida | Observação |
|---|---|---|
| 3.5 | Política de senha: tamanho, expiração, bloqueio por tentativas. Validade do link de primeiro acesso (hoje 24 h). | Hoje não há bloqueio por tentativas. |
| 3.6 | O login com Google é com Gmail pessoal dos funcionários ou com conta corporativa? | Com Gmail pessoal não dá para restringir por domínio. **Atenção:** hoje um usuário cadastrado com e-mail `@gmail.com` nasce ativo e não consegue entrar, porque o botão do Google ainda não existe. Até lá, cadastrem só e-mails que não sejam Gmail. |
| 3.7 | Matriz de transições do exame: em que estados se pode rejeitar amostra, substituir laudo, voltar etapa? Trocar um laudo já disponibilizado avisa o paciente? | Preciso disso para o fluxo do laudo, que é o próximo módulo. |
| 3.8 | Hospedagem: qual nuvem, qual região, quem contrata. | Define onde os arquivos ficam de verdade e o plano de backup. |
| 3.9 | O que fazer com paciente cadastrado por engano ou em duplicidade, com documentos diferentes? | Hoje não há saída. |
| 3.10 | Log de acesso a laudos: só o portal, ou também a equipe? Quais eventos? Por quanto tempo o log fica guardado? | Exigência de prontuário (Q45), ainda não implementada. |
| 3.11 | Termo de aceite e LGPD: o texto não existe. | Precisa de revisão jurídica. |
| 3.12 | **D2.1** — número de cadastro curto e legível (ex.: `2026-000123`) para a recepção. | Aguardando aprovação. Dá para acrescentar depois sem quebrar nada. |
| 3.13 | Lista dos exames que já devem vir cadastrados no catálogo. | O catálogo está vazio. |
| 3.14 | **Q13.1** — CRM do Dr. Arsonval. | Entra nas mensagens de médico interno. |

### Menores

**Q1.6.3:** o paciente vê a data prevista no portal? Assumi que não (P15).
**Q24:** tamanho máximo de arquivo. Assumi 50 MB (P1).

---

## Parte 4 — Dilemas

### 4.1 Os 4 usuários podem tudo

A especificação aceita isso na v1 (risco R3). Com o que já está pronto, "tudo" inclui excluir qualquer exame, rejeitar qualquer amostra, mudar os dias de revisão de todos os exames e alterar o documento de qualquer paciente. Cada uma dessas ações guarda autor e data, mas nenhuma pede uma segunda pessoa.

**Minha posição:** mantenho como está, porque perfis estão fora do escopo. Se você achar que mudar os dias de revisão é sensível demais, posso restringir só isso.

### 4.2 Arquivos em disco local

É rápido e não custa nada, mas um disco com defeito perde os laudos. A especificação fala em 20 anos de guarda.

**Minha posição:** serve para desenvolver e demonstrar. Não pode receber dado real antes de termos a hospedagem (3.8) ou, no mínimo, um backup automático da pasta.

### 4.3 Mensagens sem fila

Hoje, se o provedor de e-mail estiver fora do ar no momento do cadastro, o paciente fica cadastrado, a mensagem se perde e a tela mostra erro. Com uma fila, o sistema tentaria de novo sozinho.

**Minha posição:** construir a fila junto com a integração real de e-mail e WhatsApp, e não antes. Enquanto nada é enviado, não faz diferença.

### 4.4 Recoleta apaga a data que o paciente já conhecia

Se a Cligen disser "fica pronto dia 20" e a amostra for rejeitada, a nova data será mais tarde e a antiga some da tela do exame (continua no histórico da amostra). Na v1 o paciente não vê a data no portal, então o impacto é só interno. Quando o indicador de atraso existir, um exame com recoleta não aparecerá como atrasado em relação à primeira promessa.

**Minha posição:** está coerente com a resposta Q19, mas é exatamente o que a Q19.1 pede para confirmar. Quero que o cliente leia esta frase antes de dizer sim.

### 4.5 Exclusão sem volta na tela

O motivo, o autor e a data ficam guardados e nada é apagado do banco. Mas hoje só eu consigo reverter uma exclusão, mexendo direto no banco de dados.

**Minha posição:** fazer uma tela "Exames excluídos", com botão de restaurar, antes da entrega. É pequeno. Só não fiz porque não estava na especificação e eu queria sua opinião antes.

### 4.6 Data de acolhimento digitável

Digitável permite lançar na segunda-feira a amostra que chegou no sábado, o que é o correto para o paciente. Também permite alguém "ajustar" a data para esconder um atraso. Automática é à prova de ajuste, mas encurta o prazo do paciente quando o lançamento atrasa.

**Minha posição:** digitável, com o registro de quando e quem lançou. Quando o painel de indicadores existir, dá para listar os acolhimentos lançados com dias de diferença.

### 4.7 Nada disso foi usado por alguém da Cligen

Testei tudo por chamadas à API e abrindo as telas, mas não cliquei em cada botão como um usuário faria, e ninguém da equipe viu o sistema. Há decisões de tela (a fila de amostras, o formulário de paciente) que só uma pessoa da recepção consegue validar.

**Minha posição:** fazer uma demonstração com alguém da Cligen antes de começar o financeiro.

---

## Parte 5 — O que eu preciso de você

1. **Urgente, trava o financeiro:** respostas de C4.5 e C4.2 (dúvidas 3.1 e 3.2).
2. **Urgente, trava o fluxo do laudo:** a matriz de transições (3.7) e os textos das mensagens (3.4). Posso propor os dois; preciso saber quem aprova.
3. **Confirmar com o cliente:** os itens 1.5, 1.22 a 1.27 e o dilema 4.4.
4. **Sua opinião:** dilemas 4.1, 4.5 e 4.7.
5. **Decidir juntos:** hospedagem (3.8), porque dela dependem arquivos, backup e e-mail.
6. **Sem pressa:** o restante da Parte 3.

Tudo que está neste documento também está detalhado, por módulo, em `docs/05-regras-negocio/` e em `docs/06-pendencias/pendencias-abertas.md`.
