# Próximos passos de desenvolvimento

> Atualizado em 17/09/2026. Ordem sugerida de construção, o que cada etapa depende e quais pendências (Q/C) precisam ser fechadas com o cliente. Detalhe de cada item em [`../06-pendencias/`](../06-pendencias/); decisões já tomadas e dúvidas consolidadas em [`../para-rafael.md`](../para-rafael.md).

## Onde estamos

| Módulo | Situação |
|---|---|
| Login por senha, shell, Gestão de Usuários | ✅ |
| Identidade do usuário na API (autoria em tudo) | ✅ |
| Pacientes + responsável legal | ✅ |
| Catálogo de exames + dias de revisão (+ 3 exemplos semeados) | ✅ |
| Exames + anexos (storage em disco, provisório) + exclusão lógica com restauração | ✅ |
| Acolhimento de amostra, rejeição, recoleta + fila Amostras | ✅ |
| Fluxo do laudo + disponibilização (notificações em log) + fila Laudos | ✅ |
| Financeiro | ⬜ trava em C4.5 / C4.2 |
| Portal do paciente | ⬜ regras do código de uso único |
| Log de acesso / LGPD | ⬜ escopo do log |
| Login Google | ⬜ credenciais OAuth; Gmail pessoal × Workspace |
| Provedores reais (e-mail, WhatsApp, storage) | ⬜ Q36, Q37, hospedagem |

**Testes:** 101 de domínio (`dotnet test`). Não há testes de Aplicação/Infraestrutura nem de frontend. Todos os módulos foram validados por chamadas à API e abrindo as telas; **ninguém da Cligen usou o sistema ainda.**

---

## Fase 1 — Fechar o ciclo operacional

### 1.1 Demonstração com a Cligen
Antes do financeiro: uma sessão com alguém da recepção e alguém do laboratório usando as telas de paciente, exame, amostra e laudo. É a única forma de validar as decisões de tela.

### 1.2 Financeiro
Tudo especificado em [`financeiro.md`](../05-regras-negocio/financeiro.md), exceto convênio.
- `Pagamento` (N por exame; método, valor, status `a receber`/`recebido`/`glosado`, data de recebimento, desconto em R$ + motivo opcional) e `ParcelaPagamento` (informativa — P3). Agregado próprio, com repositório próprio: relatórios cruzam exames.
- Convênio provisório como **texto livre** + campo opcional "guia/autorização" até C4.5/C4.2; promover a cadastro `Convenio` depois (migration simples).
- Relatório por regime de caixa (só `recebido`, pela data de recebimento) com filtro por período.
- Tela: seção "Pagamentos" no exame + página `/financeiro` com o relatório.

### 1.3 Rodada de pendências com o cliente
Levar de uma vez: **C4.5, C4.2** (convênio) · **Q19.1 / Q1.7** (rejeição e recoleta) · **Q1.2 / Q1.3** (data de acolhimento digitável, sem corte) · matriz de transições do laudo · campos do responsável legal · **D2.1** · lista oficial do catálogo · **Q13.1**. Lista completa em [`para-rafael.md`](../para-rafael.md).

---

## Fase 2 — Portal do paciente

Bloqueado por regras finas ainda não escritas. Proposta a registrar antes de codar:
- Código de uso único: 6 dígitos, validade 10 min, 5 tentativas, limite de envios por documento, sessão de 30 min.
- App `frontend/portal` (Next.js, mesmo padrão BFF), entidade `TokenAcesso`, endpoints `solicitar-codigo`, `validar-codigo`, listagem por documento, download **só do laudo revisado**, mensagem D12 para exame não liberado, janela de 6 meses (C3/P5).
- Só funciona de ponta a ponta com e-mail/WhatsApp reais (Fase 4).
- Menor de idade: acesso pelo documento do **paciente**, contato do responsável (P9) — confirmar.

---

## Fase 3 — Conformidade

- **Log de acesso** (`LogAcesso`): decidir escopo. Recomendação: visualização/download de laudo e anexos, no portal e no sistema interno.
- **Termo de aceite** no cadastro do paciente (texto jurídico pendente) com o aviso dos 6 meses (C3).
- Job de expiração da exibição no portal após 6 meses (não apaga arquivo).
- Atendimento a pedido de titular (LGPD): exportação dos dados do paciente.
- Criptografia em repouso/trânsito: decorre da hospedagem.

---

## Fase 4 — Infraestrutura e produção

| Item | Decisão necessária | Onde plugar |
|---|---|---|
| Hospedagem (nuvem, região) | Cliente | — |
| Object storage (Blob/S3) | Decorre da hospedagem | Substituir `ArmazenamentoArquivosLocal` e migrar `App_Data/arquivos` (hash SHA-256 permite conferir a migração) |
| E-mail transacional + domínio/DNS (Q37) | Mateus | Substituir `EnvioEmailLog` |
| WhatsApp Business API + templates aprovados pela Meta (lead time) | Textos C7/D13 | Substituir `EnvioWhatsAppLog` |
| Login Google (Client ID/Secret; Gmail pessoal × Workspace) | Cliente/Mateus | Provider Google no Auth.js. **Até lá, não cadastrar usuários `@gmail.com`: nascem ativos e não conseguem entrar.** |
| Política de senha, bloqueio por tentativas, 2FA | Direção | `UsuarioService` |
| Fila/retry de notificações; jobs em background | Engenharia | Hosted service ou Hangfire — junto com os provedores reais |
| Backup, DR, exportação do acervo (A6) | Engenharia + direção | — |
| Ambientes dev/homolog/prod, CI/CD, migrations no deploy | Engenharia | GitHub Actions |

---

## Dívidas técnicas conhecidas

- **Testes**: só domínio. Adicionar testes de Aplicação (repositórios fake), integração da Infraestrutura (SQL Server em container) e smoke de frontend (Playwright).
- **Autorização por perfil** diferida (Q41/R3): qualquer usuário faz tudo, inclusive mudar os dias de revisão.
- **`Destino` texto livre** (Q38.1): higienizar e converter em lista quando o dashboard entrar.
- **Arquivo substituído de etapa do laudo** fica no disco sem referência e sem tela: guardar o hash antigo em log de auditoria quando o log de acesso existir.
- **Catálogo sem autoria** (criado antes da identidade na API): acrescentar `CriadoPorId`/`AtualizadoPorId`.
- **Fila de amostras** some a linha ao acolher sem mensagem de confirmação.

## Como retomar o ambiente

```bash
# backend
cd backend
dotnet ef database update --project src/Cligen.Infraestrutura --startup-project src/Cligen.Api
dotnet run --project src/Cligen.Api          # http://localhost:5053/swagger (mande X-Api-Key e X-Usuario-Id)

# frontend
cd frontend/sistema
npm install && npm run dev                   # http://localhost:3000
```

`appsettings.Development.json` e `.env.local` não vão para o git — ver os `.example`.
