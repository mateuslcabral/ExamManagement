# Modelo de dados

> Fonte: CLG-ESP-2026-001 §13. Derivado das decisões funcionais. **Sujeito a validação antes da primeira migration.**

## Entidades

| Entidade | Papel | Notas |
|---|---|---|
| `Paciente` | Cadastro único | PK `uniqueidentifier` não agrupada; índice único em (tipo de documento, número) |
| `ResponsavelLegal` | Responsável de menor | Vinculado ao paciente |
| `ExameCatalogo` | Catálogo com CRUD | Prazo de execução, preço de referência, ativo |
| `Exame` | Exame solicitado | Origem, destino, médico, datas prevista e efetiva, estado, exclusão lógica |
| `Amostra` | Acolhimento | Data de acolhimento, situação (acolhida, rejeitada, recoletada) |
| `EtapaAndamento` | As três etapas do laudo | Data e arquivo por etapa |
| `Anexo` | Arquivos | Caminho, tipo, tamanho, hash; binário fora do banco |
| `Pagamento` | Lançamento financeiro | Método, valor, status, data de recebimento |
| `ParcelaPagamento` | Parcelas de cartão | Informativa, não afeta status de pagamento |
| `Usuario` | Equipe Cligen | E-mail, `tipo_login` (google/local), hash de senha (contas locais), `google_subject_id` (contas google), ativo — ver [autenticação](../02-arquitetura/autenticacao.md) |
| `TokenAcesso` | Código de uso único do portal | Com expiração |
| `LogAcesso` | Auditoria de prontuário | Quem, o quê, quando |
| `Parametro` | Configuração | Abriga os 3 dias de revisão (parâmetro global editável) |

## Notas de projeto

**`Amostra` como tabela própria.** Modelada como tabela própria, e não como campos do exame, embora a premissa vigente seja de um acolhimento por exame (**P8**). As questões em aberto — uma coleta gerando vários exames (Q1.4), ou um exame exigindo várias amostras (Q1.5) — seguem sem resposta do cliente. Se qualquer uma for afirmativa, a relação vira um-para-muitos. Modelar como tabela desde já absorve essa resposta sem migration destrutiva: o custo hoje é uma junção a mais; o custo de errar é reescrever o cálculo de prazo depois de ter dados em produção.

**`Convenio` como entidade — pendente.** Fica pendente a definição de `Convenio` como entidade própria (cadastro de operadoras), condicionada à resposta de **C4.5**. Ver [`06-pendencias/pendencias-abertas.md`](../06-pendencias/pendencias-abertas.md).

## Camadas e agregados

A implementação segue arquitetura em camadas DDD (Domínio / Aplicação / Infraestrutura / Api) com princípios SOLID — ver [arquitetura do backend](../02-arquitetura/arquitetura-backend.md), incluindo a proposta de quais entidades acima são agregados com repositório próprio (`Paciente`, `ExameCatalogo`, `Exame`, `Pagamento`, `Usuario`) e quais são acessadas através de outro agregado (`ResponsavelLegal` via `Paciente`; `Amostra`, `EtapaAndamento`, `Anexo` via `Exame`; `ParcelaPagamento` via `Pagamento`).

## Relação com as regras de negócio

Cada entidade tem suas regras detalhadas no módulo correspondente de [`05-regras-negocio/`](../05-regras-negocio/):

- `Paciente`, `ResponsavelLegal` → [paciente.md](../05-regras-negocio/paciente.md)
- `ExameCatalogo` → [catalogo-exames.md](../05-regras-negocio/catalogo-exames.md)
- `Exame` → [exame-solicitado.md](../05-regras-negocio/exame-solicitado.md)
- `Amostra` → [amostra-e-prazo.md](../05-regras-negocio/amostra-e-prazo.md)
- `EtapaAndamento` → [fluxo-laudo.md](../05-regras-negocio/fluxo-laudo.md)
- `Pagamento`, `ParcelaPagamento` → [financeiro.md](../05-regras-negocio/financeiro.md)
- `TokenAcesso` → [portal-paciente.md](../05-regras-negocio/portal-paciente.md)
- `LogAcesso` → [conformidade.md](../05-regras-negocio/conformidade.md)
- `Usuario` → [autenticação](../02-arquitetura/autenticacao.md) (login próprio + Google, via BFF)
