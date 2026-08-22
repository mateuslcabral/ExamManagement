# Próximos passos de desenvolvimento

> Atualizado em 22/08/2026, após o commit `e65ed53`. Ordem sugerida de construção, o que cada etapa depende e quais premissas (P) ou pendências (Q/C) precisam ser fechadas com o cliente. Ver [`../06-pendencias/`](../06-pendencias/) para o detalhe de cada item.

## Onde estamos

| Módulo | Situação | Premissas |
|---|---|---|
| Login por senha, shell, Gestão de Usuários | ✅ Implementado | — |
| Pacientes + responsável legal | ✅ Implementado | P16–P21 |
| Catálogo de exames + dias de revisão | ✅ Implementado | P22–P24, P28 |
| Exames (1 paciente → N) + anexos | ✅ Implementado (storage em disco provisório) | P25–P27 |
| Acolhimento de amostra | ✅ Implementado (dentro da tela do exame) | P29–P31 |
| Fluxo do laudo + disponibilização | ✅ Implementado (notificações em log) | P32–P34 |
| Financeiro | ⬜ | C4.5, C4.2 |
| Portal do paciente | ⬜ | regras do código de uso único |
| Log de acesso / LGPD | ⬜ | escopo do log |
| Login Google | ⬜ | credenciais OAuth |
| Provedores reais (e‑mail, WhatsApp, storage) | ⬜ | Q36, Q37, hospedagem |

**Testes:** 59 testes de domínio (`dotnet test`). Ainda não há testes de Aplicação/Infraestrutura nem de frontend.

---

## Fase 1 — Fechar o ciclo operacional (próximas 1–2 semanas)

### 1.1 Filas de trabalho "Amostras" e "Laudos"
Hoje acolhimento e laudo vivem dentro de `/exames/[id]`. Os itens de menu **Amostras** e **Laudos** seguem como "em breve".
- `/amostras`: exames em *Aguardando amostra* e *Amostra acolhida*, com acolhimento/rejeição direto da lista.
- `/laudos`: exames nas etapas 3–5, com atalho para a próxima ação e destaque para *Laudo revisado* aguardando disponibilização.
- Só listagens filtradas por estado sobre o que já existe. Registrar como **P35** (Q1.1 pergunta se há tela própria do laboratório).
- Alternativa: remover os dois itens do menu.

### 1.2 Financeiro
Tudo especificado em [`financeiro.md`](../05-regras-negocio/financeiro.md), exceto convênio.
- Entidades `Pagamento` (N por exame; método, valor, status `a receber`/`recebido`/`glosado`, data de recebimento, desconto em R$ + motivo opcional) e `ParcelaPagamento` (informativa — P3).
- Convênio provisório como **texto livre** + campo opcional "guia/autorização" até C4.5/C4.2; promover a cadastro `Convenio` depois (migration simples). Registrar como **P36**.
- Relatório por regime de caixa (só `recebido`, pela data de recebimento) com filtro por período.
- Tela: aba "Pagamentos" no exame + página `/financeiro` com o relatório.
- Decidir: `Pagamento` como agregado próprio (recomendado — relatórios cruzam exames).

### 1.3 Pendências de produto que a Fase 1 expõe ao cliente
Levar numa única rodada: **C4.5, C4.2** (convênio) · **Q19.1 / Q1.7** (rejeição e recoleta — confirmar leitura de P31) · **Q1.2 / Q1.3** (data de acolhimento digitável, sem horário de corte — confirmar P29) · validação da matriz de transições **P32** · **D2.1** (número de cadastro legível) · lista oficial de exames do catálogo (substitui o seed P24) · **Q13.1** (CRM do Dr. Arsonval).

---

## Fase 2 — Portal do paciente

Bloqueado por regras finas ainda não escritas. Proposta a registrar como premissas antes de codar:
- Código de uso único: 6 dígitos, validade 10 min, 5 tentativas, rate limit por documento (ex.: 3 envios / 15 min), sessão de 30 min.
- Infra que já dá para adiantar **sem** os provedores: app `frontend/portal` (Next.js, mesmo padrão BFF), entidade `TokenAcesso`, endpoints `POST /api/portal/solicitar-codigo`, `POST /api/portal/validar-codigo`, `GET /api/portal/exames` (por documento), download do **laudo revisado** apenas (P33), mensagem D12 para exame não liberado, janela de 6 meses (C3/P5).
- Só fica funcional de ponta a ponta quando e‑mail/WhatsApp reais existirem (Fase 4).
- Menor de idade: acesso pelo documento do **paciente**, contato do responsável (P9/P19) — confirmar.

---

## Fase 3 — Conformidade

- **Log de acesso** (`LogAcesso`): decidir escopo (só portal ou também equipe; quais eventos; retenção). Recomendação: registrar visualização/download de laudo e anexos tanto no portal quanto no sistema interno — o registro é prontuário (Q45).
- **Termo de aceite** no cadastro do paciente (texto jurídico pendente) com o aviso dos 6 meses (C3).
- Job de **expiração da exibição no portal após 6 meses** (não apaga arquivo).
- Atendimento a pedido de titular (LGPD): exportação dos dados do paciente.
- Criptografia em repouso/trânsito: decorre da hospedagem.

---

## Fase 4 — Infraestrutura e produção

| Item | Decisão necessária | Onde plugar |
|---|---|---|
| Hospedagem (nuvem, região) | Cliente | — |
| Object storage (Blob/S3) | Decorre da hospedagem | Substituir `ArmazenamentoArquivosDisco` (P27) e migrar `dados/arquivos` |
| E‑mail transacional (SendGrid/SES/Brevo) + domínio/DNS (Q37) | Mateus | Substituir `EnvioEmailLog` |
| WhatsApp Business API + templates aprovados pela Meta (lead time!) | Textos C7/D13 | Substituir `EnvioWhatsAppLog` |
| Login Google (Client ID/Secret; Gmail pessoal vs Workspace) | Cliente/Mateus | Provider Google no Auth.js |
| Política de senha, 2FA | Direção | `UsuarioService` |
| Fila/retry de notificações; jobs em background | Engenharia | Hosted service ou Hangfire |
| Backup, DR, exportação do acervo (A6) | Engenharia + direção | — |
| Ambientes dev/homolog/prod, CI/CD, migrations no deploy | Engenharia | GitHub Actions |

---

## Dívidas técnicas conhecidas

- **Testes**: só domínio. Adicionar testes de Aplicação (serviços com repositórios fake) e de integração da Infraestrutura (SQL Server em container), e smoke de frontend (Playwright).
- **Paginação** nas listagens de pacientes/exames — hoje carregam tudo (ok para P2 = 200 exames/mês, mas não escala).
- **Autorização por perfil** diferida (Q41/R3): qualquer usuário exclui qualquer registro. Revisar quando perfis entrarem.
- **`Destino` texto livre** (Q38.1): higienizar e converter em lista quando o dashboard entrar.
- **Substituição de laudo após liberação não notifica nem versiona** (R2) — aceito, mas vale guardar o hash antigo em log.
- **Chrome da extensão** apontava para outra instância na porta 3000 — irrelevante para o produto, mas atrapalha validação visual automatizada.

---

## Como retomar o ambiente

```bash
# backend
cd backend
dotnet ef database update --project src/Cligen.Infraestrutura --startup-project src/Cligen.Api
dotnet run --project src/Cligen.Api          # http://localhost:5053/swagger

# frontend
cd frontend/sistema
npm install && npm run dev                   # http://localhost:3000
```

`appsettings.Development.json` e `.env.local` não vão para o git — ver os `.example`.
