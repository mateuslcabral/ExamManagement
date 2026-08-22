# Escopo da primeira versão

> Fonte: CLG-ESP-2026-001 §2, §14 · CLG-PEND-2026-001 §4

## Incluído na v1

- Cadastro de pacientes com identificação por documento e responsável legal para menores
- Cadastro de exames do catálogo, com CRUD completo (Q3)
- Registro de exame solicitado, com origem, médico solicitante e anexos
- Registro de acolhimento de amostra, com cálculo automático da previsão de liberação
- Fluxo de laudo em três etapas, com upload em cada uma
- Disponibilização manual do laudo ao paciente, com disparo de e-mail e WhatsApp
- Registro financeiro com múltiplos métodos de pagamento, desconto e valores a receber de convênio
- Portal do paciente com acesso por código de uso único
- Log de acesso a laudos, exigido pela natureza de prontuário

## Não incluído nesta versão

| Item | Origem |
|---|---|
| Dashboard gerencial e indicadores | Q38 |
| Perfis de acesso internos e restrição de campos por perfil | Q41, Q42 |
| Alarme de vencimento de prazo e critério de atraso (ação A7) | Q40 |
| Laudo retificado e laudo complementar | Q17 |
| Cadastro estruturado de médico externo e vínculo com clínica parceira | Q14, Q15 |
| Nomeação semântica dos anexos do exame | Q22 |
| Gateway de pagamento e emissão fiscal | Q30, Q31 |

**Sobre os perfis.** Com o dashboard fora da primeira versão, a única separação de acesso indispensável é a do **paciente**, que enxerga apenas os próprios exames e apenas o laudo revisado. A separação entre funções da equipe interna pode esperar — na prática, os 4 usuários internos têm as mesmas permissões na v1 (inclusive exclusão de qualquer exame — ver risco R3 em [pendências](../06-pendencias/riscos-aceitos.md)).

**Sobre a retificação.** Sem esse tratamento, corrigir um laudo já liberado significa **substituir o arquivo**, o que é permitido. O paciente não é notificado da troca e não fica histórico da versão anterior.

## Riscos aceitos pelo cliente relacionados ao escopo

Ver [`06-pendencias/riscos-aceitos.md`](../06-pendencias/riscos-aceitos.md) para R1 (dado de saúde nas mensagens), R2 (ausência de retificação) e R3 (qualquer funcionário exclui exame).

## Ações da ata — situação

| Ação | Situação |
|---|---|
| A1 — credenciais do paciente | Encerrada — resolvida pelo acesso por código de uso único |
| A2 — data-base do cálculo de prazo | Encerrada |
| A3 — lista de exames do catálogo | Encerrada — CRUD completo, sem depender de carga de planilha |
| A4 — meio de pagamento | Encerrada — lançamento manual, sem gateway |
| A5 — assinatura digital | Encerrada — assinado fora, sistema só preserva |
| A6 — retenção e arquitetura de armazenamento | **Aberta** |
| A7 — critérios de atraso | **Aberta** — diferida junto com o dashboard |
| A8 — protótipos (identidade visual de `cligen.com.br`) | **Aberta** |
