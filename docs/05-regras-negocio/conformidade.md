# Regras de negócio — Conformidade

> Fonte: CLG-ESP-2026-001 §12

| Tema | Definição |
|---|---|
| Natureza do registro | É **prontuário** (Q45) |
| Log de acesso | Obrigatório — registrar quem acessou qual laudo e quando (Q45) |
| Exclusão | Sempre lógica, com autor, data e motivo (**P4**, **P10**) |
| Assinatura do laudo | Feita **fora** da plataforma; o sistema apenas anexa e preserva (Q44) |
| Integridade do arquivo | O PDF assinado é imutável desde o upload — nenhuma recompressão, conversão ou marca d'água, sob pena de invalidar a assinatura |
| Encarregado pelos dados | Gerente da clínica (Q43) |
| Base de dados | Dados de saúde são dados pessoais sensíveis pela LGPD |

## Guarda de longo prazo (ação A6, em aberto)

No volume previsto, o custo de armazenamento é irrelevante — cerca de 480 GB ao fim de 20 anos, todos em arquivo e nenhum em banco. O risco real não é financeiro, é de continuidade: nenhum contrato de nuvem dura 20 anos. Detalhado em [armazenamento e retenção](../02-arquitetura/armazenamento-e-retencao.md).

## Pendente de especificação (não coberto pelos documentos formais)

- **Termo de aceite do cadastro** — a redação obrigatória sobre os 6 meses de disponibilidade do laudo (ver [portal do paciente](portal-paciente.md)) precisa ser escrita e revisada juridicamente; texto completo do termo ainda não existe.
- **Escopo exato do log de acesso** — só o portal do paciente, ou também os acessos da equipe interna? Quais eventos exatamente (visualização, download, tentativa de acesso negado)?
- **Retenção do próprio log de acesso** — por quanto tempo o log fica guardado.
- **Criptografia** em repouso e em trânsito — não especificada nos documentos formais.
- **Atendimento a pedido de titular (LGPD)** — processo para paciente solicitar seus dados, correção ou informação sobre uso.

Ver [`06-pendencias/pendencias-abertas.md`](../06-pendencias/pendencias-abertas.md).
