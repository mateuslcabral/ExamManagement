# Pendências — o que falta fazer e o que falta ver com o cliente

> 16/10/2026. Consolida [`06-pendencias/`](../06-pendencias/), [`07-roadmap/`](../07-roadmap/proximos-passos.md) e [`para-rafael.md`](../para-rafael.md) numa lista única por área. Os códigos (Q, C, D, P) são os dos documentos formais.

Legenda de quem resolve: **Cliente** (Cligen), **Mateus**, **Rafael/direção**, **Eng** (engenharia, sem decisão externa).

---

## 1. O que falta ver com o cliente

### 1.1 Respostas que mudam regra já implementada (prioridade alta)

| # | Pergunta | O que está implementado hoje | Impacto se mudar |
|---|---|---|---|
| C4.5 | Convênio é **cadastro** de operadoras ou texto digitado? | Texto livre; o relatório agrupa pelo nome digitado. | Migration simples + padronizar os nomes já digitados. |
| C4.2 | Registrar número de guia/autorização é obrigatório? | Campo opcional. | Trivial. |
| Q1.2 | Data de acolhimento é automática ou digitada? | Digitada, hoje por padrão; não futura nem anterior à entrada; guarda quem/quando lançou. | Trivial travar o campo. |
| Q1.3 | Há horário de corte para a amostra? | Não (P7). | Passa a pedir hora. Médio. |
| Q1.4 / Q1.5 | Uma coleta para vários exames? Um exame com várias amostras? | Uma amostra por exame (P8). Banco preparado para N. | Tela e cálculo. Médio. |
| Q1.7 / Q19.1 | Na recoleta, a data prevista é recalculada do zero? | Sim: rejeição apaga a previsão; novo acolhimento gera nova data. | Outra regra. Médio. |
| — | Matriz de transições do laudo: pode voltar etapa? Pode desfazer "disponibilizado"? | Sem volta após a etapa 3; sem desfazer disponibilização. Substituir laudo liberado é permitido e não avisa o paciente (R2). | Médio. |
| Q1.6.1 | Os 3 dias de revisão são iguais para todos os exames? | Parâmetro global editável (P13). | Vira campo do catálogo. Médio. |
| — | Responsável legal: quais dados? | Nome, documento (CPF/passaporte), parentesco opcional. Contato é o do cadastro (P9). | Acrescentar campo é simples. |
| — | Paciente maior de idade pode ter responsável? | Sim, opcional. | Trivial. |
| — | O total lançado pode passar do valor a pagar? | Não (recusa). | Vira aviso. Trivial. |
| Q26 | Particular sempre quita no ato? | Sim: lançamento particular nasce "recebido". | Precisaria de "a receber" para particular. Pequeno. |

### 1.2 Informações que o cliente precisa fornecer

| # | Item | Trava o quê |
|---|---|---|
| Q3 | **Lista oficial de exames do catálogo** (nome, prazo, preço). | Hoje há 3 exemplos inventados. |
| C7 / D13 | **Textos** da mensagem de boas-vindas e dos dois avisos de laudo (médico interno / externo). | Envio real de e-mail e WhatsApp; aprovação dos templates pela Meta (lead time de semanas). |
| Q13.1 | CRM do Dr. Arsonval Lamounier Junior. | Mensagem de médico interno. |
| Q36 | Acesso à conta WhatsApp Business existente (ou BSP). | Integração do WhatsApp. |
| Q37 | E-mail/domínio de envio e acesso ao DNS de cligen.com.br. | Envio de e-mail. (Mateus levanta o DNS.) |
| — | Login Google: Gmail pessoal dos 4 funcionários ou Google Workspace? | Botão "Entrar com Google" e restrição por domínio. |
| — | Destinos (laboratórios executores) são uma lista conhecida? | Campo "Destino" pode virar lista agora, evitando limpeza futura (Q38.1). |
| D2.1 | Aprovar o número de cadastro curto (ex.: 2026-000123). | Pequeno, pode entrar depois. |
| Q24 | Tamanho máximo de arquivo. | Assumido 50 MB (P1). |
| Q1.6.3 | Paciente vê a data prevista no portal? | Assumido que não (P15). |
| Q46 | Volume real de exames/mês. | Assumido 200/mês (P2). |

### 1.3 Decisões de direção (Cligen e Rafael)

| Item | Observação |
|---|---|
| **Hospedagem**: nuvem, região (Brasil, por dado de saúde), quem contrata. | Dela dependem arquivos, backup, e-mail e produção. |
| **Política de senha** e bloqueio por tentativas; validade do link de primeiro acesso (hoje 24 h); 2FA. | Hoje: mínimo 8 caracteres, sem bloqueio. |
| **Perfis de acesso**: aceitar que os 4 usuários fazem tudo na v1 (R3), inclusive excluir exames e mudar os dias de revisão? | Especificação aceita; vale ciência explícita. |
| **Regras do código de uso único do portal**: dígitos, validade, tentativas, limite de envios, sessão. | Proposta: 6 dígitos, 10 min, 5 tentativas, sessão de 30 min. |
| **Escopo do log de acesso**: só portal ou também equipe; quais eventos; retenção. | Exigência de prontuário (Q45). |
| **Termo de aceite / LGPD**: texto, base legal, atendimento a titular. | Revisão jurídica. |
| **Paciente cadastrado por engano ou em duplicidade** (documentos diferentes): o que fazer? | Hoje não há saída. |
| **Retenção de 20 anos e exportação do acervo** (ação A6). | Antes de escolher o provedor de armazenamento. |

---

## 2. O que falta fazer — por área

### 2.1 Sistema interno (pequeno)

| Item | Esforço |
|---|---|
| Fila de Amostras vazia: explicar que exames acolhidos foram para Laudos; confirmação ao acolher pela fila. | Pequeno |
| Catálogo: guardar quem criou/alterou. | Pequeno |
| Exportar o relatório financeiro (CSV/Excel); relatório por paciente. | Pequeno |
| Convênio como cadastro (quando C4.5 responder). | Pequeno |
| Campo "Destino" como lista (quando houver a lista). | Pequeno |
| Número de cadastro curto do paciente (D2.1). | Pequeno |
| Login com Google (depende das credenciais e da resposta Gmail × Workspace). | Médio |

### 2.2 Portal do paciente (app novo)

- App `frontend/portal` em Next.js, mesmo padrão BFF.
- Login por documento + código de uso único enviado por e-mail/WhatsApp; entidade `TokenAcesso`.
- Lista dos exames do paciente; download só do laudo revisado; mensagem D12 para exame não liberado; janela de 6 meses de exibição (o arquivo não é apagado).
- Menor de idade: acesso pelo documento do paciente, contato do responsável (confirmar).
- **Só funciona de ponta a ponta com e-mail/WhatsApp reais.**
- Esforço: grande (2 a 3 semanas, mais a integração das mensagens).

### 2.3 Conformidade

- Log de acesso a laudos e anexos (`LogAcesso`): quem, o quê, quando — no portal e no sistema interno. Médio.
- Termo de aceite no cadastro do paciente (texto pendente). Pequeno, após o texto.
- Expiração da exibição no portal após 6 meses (job). Pequeno.
- Exportação dos dados de um paciente (pedido de titular). Pequeno.
- Registro de exclusão/restauração e substituição de laudo no log. Pequeno, junto com o log.

### 2.4 Integrações reais

| Item | Depende de |
|---|---|
| E-mail transacional (SendGrid/SES/Brevo) + domínio e DNS. | Q37, provedor escolhido. |
| WhatsApp Business API + templates aprovados pela Meta. | Textos (C7), conta (Q36). **Começar cedo: aprovação demora.** |
| Armazenamento em nuvem (Blob/S3) e migração dos arquivos locais (hash permite conferir). | Hospedagem. |
| Fila com reenvio automático das mensagens; jobs em segundo plano. | Junto com os provedores. |

### 2.5 Produção

- Ambientes (dev/homologação/produção), deploy automatizado (GitHub Actions), migrations no deploy.
- Backup do banco e dos arquivos; plano de recuperação; exportação íntegra do acervo.
- Criptografia em repouso/trânsito (decorre da hospedagem).
- Política de senha e bloqueio por tentativas; 2FA (v2).

### 2.6 Qualidade

- Testes de Aplicação (repositórios simulados), de integração (SQL Server em container) e de tela (Playwright). Hoje só há testes de domínio (114).
- **Demonstração e uso real pela equipe da Cligen**: até hoje, só Mateus usou o sistema. As decisões de tela (filas, formulários) só a recepção e o laboratório validam.

---

## 3. Ordem sugerida

1. Rodada única de perguntas ao cliente (seções 1.1 e 1.2) — **leva junto a apresentação**.
2. Ajustes pequenos do sistema interno (2.1) conforme as respostas.
3. Mateus: DNS/e-mail (Q37); Cligen: textos (C7) e conta WhatsApp (Q36) — iniciar a aprovação dos templates.
4. Hospedagem decidida → armazenamento em nuvem, e-mail real, ambiente de homologação.
5. Portal do paciente (2.2) + log de acesso (2.3).
6. Produção (2.5) com backup antes de qualquer dado real.
