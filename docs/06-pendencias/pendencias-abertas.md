# Pendências abertas

> Fonte: CLG-PEND-2026-001 §2, §6, §7 — pendências de negócio que dependem do cliente — mais uma seção adicional de planejamento técnico que não constava em nenhum documento anterior.

## 1. Bloqueiam o modelo de dados / implementação

| ID | Pergunta | O que trava |
|---|---|---|
| **Q1.6.3** | O paciente vê a data prevista no portal, ou ela é interna à equipe? | Tela do portal (recomendado não exibir — **P15**) |

### Resolvidas na reunião de 17/10/2026 (implementadas em 19/10/2026)

| ID | Pergunta | Resolução |
|---|---|---|
| **C4.5** | Convênio: cadastro ou texto? | **Cadastro** — entidade `Convenio` (cadastro auxiliar, tela `/cadastros`). Textos já digitados convertidos pela migration. |
| **C4.2** | Número de guia/autorização obrigatório? | **Opcional**, como já estava. |
| **Q1.6.1** | Dias de revisão iguais para todos os exames? | **Não existe parcela de revisão separada**: o prazo de cada exame no catálogo já a inclui. Parâmetro removido; **P13 derrubada**. |
| **Q22** | O que são os 3 anexos? | Anexo **tipado**: pedido médico, termo de consentimento ou outro. Limite de 3 mantido. |
| **Q9** | Endereço do paciente? | Só a **cidade**, campo opcional. |
| **Q38.1** | Destino como lista fechada? | **Cadastro auxiliar** `Destino`, com destino padrão por exame do catálogo. |

## 2. Derivadas do acolhimento da amostra

Ver detalhamento em [amostra-e-prazo.md](../05-regras-negocio/amostra-e-prazo.md) — Q1.1, Q1.2, Q1.3, Q1.4, Q1.5, Q1.7.

## 3. Demais itens em aberto

| ID | Item | Situação |
|---|---|---|
| C7 | Novo texto da mensagem de cadastro, sem usuário e senha | Rascunho será proposto com os protótipos (A8) |
| D2.1 | Número de cadastro legível ao lado do GUID | Proposta aguardando aprovação |
| Q24 | Tamanho máximo do laudo em PDF | Cliente não soube — premissa P1 (50 MB) |
| Q37 | E-mail e domínio de envio; acesso ao DNS de `cligen.com.br` | **A cargo do Mateus** |
| Q46 | Volume real de exames/mês e usuários simultâneos | Cliente não soube — premissa P2 (200/mês, 4 usuários) |
| Q13.1 | CRM do Dr. Arsonval Lamounier Junior | Não informado |
| Q19.1 | Confirmar leitura: rejeição zera o prazo e o exame volta a aguardar amostra; a recoleta, ao ser acolhida, reinicia o fluxo e recalcula a data | Interpretação a confirmar |
| — | Hospedagem: nuvem, região e responsável pela contratação | Ver [stack](../01-stack/stack-tecnologica.md) |
| — | Lista de exames pré-carregados para o seed inicial do catálogo | A fornecer |

## 4. Planejamento técnico ainda não registrado

Identificado na revisão da especificação — não é pendência do cliente, é lacuna de planejamento de engenharia a fechar durante o desenho técnico.

**Fechados em 22/08/2026:** framework de frontend (React + Next.js/BFF, dois apps, Tailwind), autenticação da equipe interna (OAuth próprio + Google), fluxo de provisionamento na Gestão de Usuários, mecanismo BFF → API (chave de serviço `X-Api-Key`), organização do backend (DDD, 4 projetos) e identidade visual (ação A8, extraída de cligen.com.br) — ver [stack](../01-stack/stack-tecnologica.md), [autenticação](../02-arquitetura/autenticacao.md) e [arquitetura do backend](../02-arquitetura/arquitetura-backend.md). **Implementado e validado:** login por senha, shell do sistema interno, Gestão de Usuários, Catálogo de exames (consulta em `/catalogo`, CRUD em `/catalogo/gerenciar`, com preço limite, genes, destino padrão e informações técnicas), Cadastros auxiliares (origens, destinos, convênios), repasse da identidade do usuário à API, Cadastro de Pacientes (com cidade), Exame solicitado com código `EX-000001`, anexos tipados, comentários e exclusão lógica, acolhimento de amostra com previsão de liberação, rejeição e recoleta, fluxo do laudo até a disponibilização (notificação em log), restauração de exame excluído, página Fluxos (seis grupos, filtro por data) e financeiro (desconto, lançamentos, convênio cadastrado a receber/glosa, relatório por regime de caixa com filtros por convênio e origem) (`frontend/sistema`, `backend/`). Faltam: portal do paciente, log de acesso, login Google e provedores reais. Resíduos que ainda faltam fechar:

| Item | Nota |
|---|---|
| Confirmar se o login Google é Gmail pessoal ou Workspace corporativo | Gmail pessoal não suporta restrição de domínio (`hd`) — ver [autenticação](../02-arquitetura/autenticacao.md) |
| Campos do responsável legal | Implementado com nome, documento e parentesco opcional — confirmar com o cliente (ver [paciente.md](../05-regras-negocio/paciente.md)) |
| Credenciais OAuth do Google (Client ID/Secret) | Criar projeto no Google Cloud — pré-requisito do botão "Entrar com Google", ainda não implementado |
| Política de senha das contas locais | Tamanho mínimo, expiração, bloqueio por tentativas |
| Validade do link de definição de senha (primeiro acesso) | Implementado como parâmetro (`TokenDefinicaoSenha:ValidadeHoras`, padrão 24 h) — confirmar o valor com o cliente |
| 2FA para o sistema interno | Não decidido — considerar para v2, dado que é prontuário |
| Provedor de object storage | Azure Blob, S3, ou outro — decorre da escolha de hospedagem. **Provisório: disco local** (ver [armazenamento](../02-arquitetura/armazenamento-e-retencao.md)) |
| Provedor de e-mail transacional | SendGrid, SES, Brevo, etc. |
| Regras finas do código de uso único do portal | Formato, validade, tentativas, rate limit, expiração de sessão (entidade `TokenAcesso` existe, regras não) |
| Escopo do log de acesso | Só portal ou também equipe interna; quais eventos; retenção do próprio log |
| Backup, DR e exportação do acervo (ação A6) | Política de backup, formato de exportação íntegra, verificação de hash |
| Ambientes e entrega | Dev/homolog/produção, CI/CD, estratégia de migrations do EF Core |
| Jobs em background | Envio de mensagens com retry/fila; expiração da disponibilidade do laudo após 6 meses |
| Testes e critério de aceite | Estratégia de testes, quem homologa cada funcionalidade |
| LGPD operacional | Texto do termo de aceite, base legal, atendimento a pedido de titular, criptografia em repouso/trânsito |
| Plano de releases | Ordem de construção dos módulos, prazos |
| Matriz de transições de estado do laudo | Implementada uma matriz (ver [fluxo do laudo](../05-regras-negocio/fluxo-laudo.md)) — **confirmar com o cliente** |
| Aprovação de templates WhatsApp | Depende dos textos (C7); tem lead time junto à Meta |

## Encaminhamento (herdado de CLG-PEND-2026-001 §7)

1. ~~Decidir **C4.5** e **C4.2**~~ — resolvidas em 17/10/2026 (cadastro de convênios; guia opcional). Nenhuma pendência de negócio trava mais o modelo de dados.
2. Definir hospedagem: nuvem, região e responsável pela contratação.
3. Aprovar **D2.1** e as premissas **P10** a **P15** (ver [premissas.md](premissas.md); P13 já derrubada).
4. Responder o restante do bloco Q1.1–Q1.7 (regra de negócio, não trava a modelagem).
5. Mateus: verificar DNS e domínio de envio (Q37) e obter o CRM do médico interno (Q13.1).
6. Fechar as escolhas da seção 4 acima (planejamento técnico) antes de travar a arquitetura definitiva.
