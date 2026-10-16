# O que temos — inventário do sistema em 16/10/2026

Tudo abaixo está implementado, testado por chamadas à API e pelas telas, e publicado no GitHub (`main`, commit `d1b4067`). O sistema roda localmente (API ASP.NET Core na porta 5053, frontend Next.js na 3000, SQL Server local).

## Sistema interno — telas

| Tela | O que faz |
|---|---|
| **Login** | E-mail e senha. Sessão de 8 h. Usuário desativado perde o acesso na hora. |
| **Início** | Cards dos módulos. |
| **Pacientes** | Busca por nome, CPF ou passaporte (aceita pontuação), com paginação. Cadastro e edição: nome, nascimento, documento (CPF validado pelos dígitos; passaporte), e-mail, telefone (normalizado para +55…), responsável legal (obrigatório para menor; opcional para maior). Atalhos "Exames" e "Novo exame" por paciente. Não há exclusão de paciente. |
| **Exames** | Lista com busca (paciente, documento, nome do exame), situação e data prevista. Novo exame: busca de paciente enquanto digita, exame do catálogo (preço herdado e editável), origem, destino (texto livre, opcional), médico interno ou externo. Lista de **excluídos** com motivo e botão Restaurar. |
| **Exame (detalhe)** | Seções: Amostra, Fluxo do laudo, Anexos, Dados do exame, Financeiro, Excluir. |
| ↳ Amostra | Registro do acolhimento (data digitável, hoje por padrão; não futura nem anterior à entrada). Mostra a previsão antes de confirmar. Rejeição com motivo; recoleta gera nova previsão; histórico das rejeitadas. |
| ↳ Fluxo do laudo | Três etapas (parceiro → Cligen para revisão → revisado), sem pular. Upload só de PDF (conferido pelo conteúdo, até 50 MB), substituição sem mudar data/estado, contador de substituições. Botão **Disponibilizar ao paciente** (com confirmação) grava a data efetiva e dispara e-mail e WhatsApp (hoje, só no log). |
| ↳ Anexos | Até 3 arquivos PDF/JPG/PNG (até 50 MB), formato conferido pelo conteúdo, SHA-256 guardado. Remoção lógica. |
| ↳ Financeiro | Preço, desconto em reais (motivo opcional), valor a pagar, recebido, a receber, situação (Quitado / Falta lançar). Lançamentos: Pix, cartão de crédito (parcelas informativas), débito, dinheiro, boleto, transferência, convênio (texto livre + guia opcional). Receber convênio (com data), glosar (motivo), excluir (motivo). |
| ↳ Excluir | Exclusão lógica com motivo obrigatório. |
| **Amostras** | Fila dos exames aguardando amostra, com acolhimento direto na linha. |
| **Laudos** | Fila dos exames nos estados 2 a 5, agrupados pela próxima ação; data prevista já passada em destaque. |
| **Financeiro** | Relatório por regime de caixa: recebidos por data de recebimento no período (padrão: mês corrente), totais por método; a receber de convênio, totais por operadora. |
| **Catálogo de exames** | CRUD: nome (único), prazo de execução (dias corridos), preço de referência, ativo/inativo. Mostra a entrega ao paciente (prazo + revisão). Edita os **dias de revisão** globais (padrão 3). Semeado com 3 exames de exemplo. |
| **Gestão de Usuários** | Criar (e-mail @gmail → conta Google, ativa; outros → local, recebe link para definir senha), renomear, desativar/reativar, reenviar link. Ninguém desativa a si mesmo. |

## Regras de negócio implementadas (resumo)

- Paciente: documento único por tipo ("Esse CPF já foi utilizado"); contato obrigatório; menor exige responsável; mensagem de boas-vindas disparada no cadastro (em log).
- Exame: nasce "Aguardando amostra"; paciente não muda (exclui e recadastra); catálogo só muda antes do acolhimento.
- Previsão de liberação = acolhimento + prazo de execução + dias de revisão, **gravada uma vez e nunca recalculada** (P14); a amostra guarda os números usados.
- Matriz de transições: 1→2 (acolher), 2→1 (rejeitar), 2→3→4→5 (etapas, sem pular), 5→6 (disponibilizar). Sem volta após a etapa 3.
- Financeiro: particular nasce recebido; convênio nasce a receber; total lançado ≤ valor a pagar; glosa só em convênio; regime de caixa.
- Tudo é prontuário: nenhuma exclusão é física; exclusões e remoções guardam autor, data e motivo; cadastros guardam quem criou/alterou; arquivos nunca são apagados e têm hash.

## Base técnica

- **Backend**: ASP.NET Core 10, EF Core 10, SQL Server, 4 projetos em camadas (Domínio, Aplicação, Infraestrutura, Api). 8 migrations. **114 testes de domínio.**
- **Frontend**: Next.js 16 (App Router) como BFF, React 19, Tailwind 4, Auth.js. Identidade visual do site cligen.com.br.
- **Segurança**: o navegador nunca fala com a API; o BFF autentica com chave de serviço e repassa o usuário logado (`X-Usuario-Id`), que a API valida em toda chamada.
- **Arquivos**: disco local da máquina da API (`App_Data/arquivos`), provisório, sem redundância. Upload em stream (corrigido o corte de 10 MB do Next).
- **Mensagens**: e-mail e WhatsApp só registram no log (sem provedor).
- **Documentação**: `docs/` com especificação, regras por módulo com seção "Implementação", pendências, roadmap e `para-rafael.md` (decisões, dúvidas e dilemas).

## O que não existe

Portal do paciente, log de acesso a laudos, login com Google, provedores reais (e-mail, WhatsApp, armazenamento em nuvem), ambiente de produção/backup, termo de aceite/LGPD, perfis de acesso (todos os usuários fazem tudo), dashboard, exportação de relatórios.
