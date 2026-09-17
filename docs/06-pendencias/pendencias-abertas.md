# Pendências abertas

> Fonte: CLG-PEND-2026-001 §2, §6, §7 — pendências de negócio que dependem do cliente — mais uma seção adicional de planejamento técnico que não constava em nenhum documento anterior.

## 1. Bloqueiam o modelo de dados / implementação

| ID | Pergunta | O que trava |
|---|---|---|
| **C4.5** | Convênio será um **cadastro** (lista de operadoras, permitindo saber quanto cada uma deve) ou apenas texto digitado no pagamento? Recomendação: cadastro. | [Modelo financeiro](../05-regras-negocio/financeiro.md) / entidade `Convenio` no [schema](../04-schema/modelo-de-dados.md) |
| **C4.2** | É necessário registrar número de guia ou autorização do convênio? | Campo da tela de pagamento |
| **Q1.6.1** | Os 3 dias de revisão são iguais para todos os exames, ou algum tipo tem revisão mais longa? | Se variar por exame, vira campo do catálogo em vez de parâmetro global (assumido global — **P13**) |
| **Q1.6.3** | O paciente vê a data prevista no portal, ou ela é interna à equipe? | Tela do portal (recomendado não exibir — **P15**) |

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
| Q38.1 | Campo Destino segue como texto livre nesta versão; quando o dashboard entrar, será preciso lista fechada e higienização do já digitado | Diferido junto com o dashboard |
| — | Hospedagem: nuvem, região e responsável pela contratação | Ver [stack](../01-stack/stack-tecnologica.md) |
| — | Lista de exames pré-carregados para o seed inicial do catálogo | A fornecer |

## 4. Planejamento técnico ainda não registrado

Identificado na revisão da especificação — não é pendência do cliente, é lacuna de planejamento de engenharia a fechar durante o desenho técnico.

**Fechados em 22/08/2026:** framework de frontend (React + Next.js/BFF, dois apps, Tailwind), autenticação da equipe interna (OAuth próprio + Google), fluxo de provisionamento na Gestão de Usuários, mecanismo BFF → API (chave de serviço `X-Api-Key`), organização do backend (DDD, 4 projetos) e identidade visual (ação A8, extraída de cligen.com.br) — ver [stack](../01-stack/stack-tecnologica.md), [autenticação](../02-arquitetura/autenticacao.md) e [arquitetura do backend](../02-arquitetura/arquitetura-backend.md). **Implementado e validado:** login por senha, shell do sistema interno, Gestão de Usuários, Catálogo de exames com o parâmetro global de dias de revisão, repasse da identidade do usuário à API, Cadastro de Pacientes, Exame solicitado com anexos e exclusão lógica, e acolhimento de amostra com previsão de liberação, rejeição e recoleta (`frontend/sistema`, `backend/`). Resíduos que ainda faltam fechar:

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
| Matriz de transições de estado do laudo | Formalizar quais transições são permitidas em cada estado |
| Aprovação de templates WhatsApp | Depende dos textos (C7); tem lead time junto à Meta |

## Encaminhamento (herdado de CLG-PEND-2026-001 §7)

1. Decidir **C4.5** e **C4.2** — únicas pendências de negócio que ainda travam o modelo de dados.
2. Definir hospedagem: nuvem, região e responsável pela contratação.
3. Aprovar **D2.1** e as premissas **P10** a **P15** (ver [premissas.md](premissas.md)).
4. Responder o restante do bloco Q1.1–Q1.7 (regra de negócio, não trava a modelagem).
5. Mateus: verificar DNS e domínio de envio (Q37) e obter o CRM do médico interno (Q13.1).
6. Fechar as escolhas da seção 4 acima (planejamento técnico) antes de travar a arquitetura definitiva.
