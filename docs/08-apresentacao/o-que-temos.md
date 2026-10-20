# O que temos — inventário do sistema em 19/10/2026

Tudo abaixo está implementado e testado por chamadas à API e pelas telas (branch `main`; inclui os 17 pedidos da reunião com o cliente em 17/10/2026). O sistema roda localmente (API ASP.NET Core na porta 5053, frontend Next.js na 3000, SQL Server local).

## Sistema interno — telas

| Tela | O que faz |
|---|---|
| **Login** | E-mail e senha. Sessão de 8 h. Usuário desativado perde o acesso na hora. |
| **Início** | Cards dos módulos. |
| **Pacientes** | Busca por nome, CPF ou passaporte (aceita pontuação), com paginação. Cadastro e edição: nome, nascimento, documento (CPF validado pelos dígitos; passaporte), e-mail, telefone (normalizado para +55…), **cidade** (opcional), responsável legal (obrigatório para menor; opcional para maior). Atalhos "Exames" e "Novo exame" por paciente. Não há exclusão de paciente. |
| **Exames** | Lista com **código** (`EX-000001`), busca (código, paciente, documento, nome do exame), situação e data prevista. Novo exame: busca de paciente enquanto digita, exame do catálogo (preço herdado e editável; destino padrão pré-preenchido), origem e destino escolhidos dos **cadastros auxiliares**, médico interno ou externo (externo com CRM e UF opcionais). Lista de **excluídos** com motivo e botão Restaurar. |
| **Exame (detalhe)** | Cabeçalho com código, médico (CRM/UF). Seções: Amostra, Fluxo do laudo, Anexos, Comentários, Dados do exame, Financeiro, Excluir. Toda confirmação usa um diálogo próprio (`Dialogo`), não mais o `confirm` do navegador. |
| ↳ Amostra | Registro do acolhimento (data digitável, hoje por padrão; não futura nem anterior à entrada) — é o único lugar onde se acolhe. Mostra a previsão antes de confirmar. Rejeição com motivo; recoleta gera nova previsão; histórico das rejeitadas. |
| ↳ Fluxo do laudo | Três etapas (parceiro → Cligen para revisão → revisado), sem pular. Upload só de PDF (conferido pelo conteúdo, até 50 MB), substituição sem mudar data/estado, contador de substituições. Botão **Disponibilizar ao paciente** (com confirmação) grava a data efetiva e dispara e-mail e WhatsApp (hoje, só no log). |
| ↳ Anexos | Até 3 arquivos PDF/JPG/PNG (até 50 MB), cada um com **tipo** obrigatório (pedido médico, termo de consentimento, outro), formato conferido pelo conteúdo, SHA-256 guardado. Remoção lógica. |
| ↳ Comentários | Lista cronológica com autor e data; só inserção (nunca editado nem apagado). |
| ↳ Dados do exame | Abrem **somente leitura**; "Editar" pede confirmação para liberar os campos e "Salvar alterações" confirma de novo. |
| ↳ Financeiro | Preço, desconto em reais (motivo opcional; o valor a pagar não desce abaixo do **preço limite** do catálogo), valor a pagar, recebido, a receber, situação (Quitado / Falta lançar). Vários meios por exame; botão **Lançar pagamento (falta R$ X)** em destaque enquanto há saldo. Lançamentos: Pix, cartão de crédito (parcelas informativas), débito, dinheiro, boleto, transferência, convênio (escolhido do cadastro + guia opcional). Receber convênio (com data), glosar (motivo), excluir (motivo). |
| ↳ Excluir | Exclusão lógica com motivo obrigatório. |
| **Fluxos** | Todos os exames agrupados pelos seis estados, na ordem do fluxo (Aguardando amostra → Amostra acolhida → Laudo parceiro pronto → Laudo Cligen para revisão → Laudo revisado → Disponibilizado), com contagem por grupo e total. Filtro por data de acolhimento, prevista ou de disponibilização (de/até). Data prevista já passada em destaque. Substitui as antigas telas Amostras e Laudos. |
| **Financeiro** | Relatório por regime de caixa: recebidos por data de recebimento no período (padrão: mês corrente), totais por método e **por origem**; a receber de convênio, totais por operadora. Filtros por **convênio** e por **origem**; cada linha mostra o código do exame. |
| **Catálogo de exames** | `/catalogo`: **visão de consulta** — cards com nome, prazo de entrega (dias corridos, já com revisão), destino padrão, preço de referência e preço limite, nº de genes, genes e informações técnicas; inativos em seção separada. `/catalogo/gerenciar`: CRUD (nome único, ativo/inativo, autoria). Semeado com 3 exames de exemplo. |
| **Cadastros auxiliares** | `/cadastros`, três abas: origens (semeadas: Cligen, Clínica parceira, Site Cligen, Plataforma), destinos (laboratórios) e convênios. Criar, renomear, desativar/reativar; nunca apagados. |
| **Gestão de Usuários** | Criar (e-mail @gmail → conta Google, ativa; outros → local, recebe link para definir senha), renomear, desativar/reativar, reenviar link. Ninguém desativa a si mesmo. |

## Regras de negócio implementadas (resumo)

- Paciente: documento único por tipo ("Esse CPF já foi utilizado"); contato obrigatório; menor exige responsável; mensagem de boas-vindas disparada no cadastro (em log).
- Exame: nasce "Aguardando amostra" com código sequencial; paciente não muda (exclui e recadastra); catálogo só muda antes do acolhimento; comentários nunca são apagados.
- Previsão de liberação = acolhimento + prazo do catálogo (que já inclui a revisão — não há mais "dias de revisão"), **gravada uma vez e nunca recalculada** (P14); a amostra guarda o prazo usado.
- Matriz de transições: 1→2 (acolher), 2→1 (rejeitar), 2→3→4→5 (etapas, sem pular), 5→6 (disponibilizar). Sem volta após a etapa 3.
- Financeiro: particular nasce recebido; convênio (cadastrado) nasce a receber; total lançado ≤ valor a pagar; desconto respeita o preço limite; glosa só em convênio; regime de caixa.
- Origens, destinos e convênios são cadastros: desativar preserva o histórico; nada é apagado.
- Tudo é prontuário: nenhuma exclusão é física; exclusões e remoções guardam autor, data e motivo; cadastros guardam quem criou/alterou; arquivos nunca são apagados e têm hash.

## Base técnica

- **Backend**: ASP.NET Core 10, EF Core 10, SQL Server, 4 projetos em camadas (Domínio, Aplicação, Infraestrutura, Api). Migrations EF Core (última: `20261019174706_CadastrosCatalogoEAjustesCliente`). **124 testes de domínio.**
- **Frontend**: Next.js 16 (App Router) como BFF, React 19, Tailwind 4, Auth.js. Identidade visual do site cligen.com.br.
- **Segurança**: o navegador nunca fala com a API; o BFF autentica com chave de serviço e repassa o usuário logado (`X-Usuario-Id`), que a API valida em toda chamada.
- **Arquivos**: disco local da máquina da API (`App_Data/arquivos`), provisório, sem redundância. Upload em stream (corrigido o corte de 10 MB do Next).
- **Mensagens**: e-mail e WhatsApp só registram no log (sem provedor).
- **Documentação**: `docs/` com especificação, regras por módulo com seção "Implementação", pendências, roadmap e `para-rafael.md` (decisões, dúvidas e dilemas).

## O que não existe

Portal do paciente, log de acesso a laudos, login com Google, provedores reais (e-mail, WhatsApp, armazenamento em nuvem), ambiente de produção/backup, termo de aceite/LGPD, perfis de acesso (todos os usuários fazem tudo), dashboard, exportação de relatórios.
