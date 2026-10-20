# Modelo de dados

> Fonte: CLG-ESP-2026-001 §13. Derivado das decisões funcionais. **Sujeito a validação antes da primeira migration.**

## Entidades

| Entidade | Papel | Notas |
|---|---|---|
| `Paciente` | Cadastro único | PK `uniqueidentifier` não agrupada; índice único em (tipo de documento, número) |
| `ResponsavelLegal` | Responsável de menor | Vinculado ao paciente |
| `ExameCatalogo` | Catálogo com CRUD | `PrazoDias` (entrega, já com revisão), `PrecoReferencia`, `PrecoLimite` (piso), `NumeroGenes`, `Genes`, `DestinoId` (padrão), `InformacoesTecnicas`, ativo, autoria |
| `Exame` | Exame solicitado | `Numero` (sequência `Seq`, exibido como `EX-000001`), `OrigemId` (FK obrigatória), `DestinoId` (FK opcional), médico (externo com `CrmMedico`/`UfCrm`), `DataAcolhimento` (desnormalizada), datas prevista e efetiva, estado, exclusão lógica |
| `Comentario` | Comentários do exame | Coleção pertencente a `Exame`: `Texto` (≤ 2000), `AutorId`, `CriadoEm`. Só inserção |
| `Amostra` | Acolhimento | Data de acolhimento, `PrazoDias` usado no cálculo, situação (acolhida, rejeitada, recoletada) |
| `EtapaAndamento` | As três etapas do laudo | Data e arquivo por etapa |
| `Anexo` | Arquivos | `TipoAnexo` (PedidoMedico, TermoConsentimento, Outro), caminho, tipo, tamanho, hash; binário fora do banco |
| `Origem` · `Destino` · `Convenio` | Cadastros auxiliares | Base abstrata `CadastroSimples`: id, nome único (sem diferenciar maiúsculas), ativo, autoria. Nunca apagados, só desativados |
| `Pagamento` | Lançamento financeiro | Método, valor, status, data de recebimento, `ConvenioId` (FK, obrigatória quando método = Convênio), guia opcional |
| `ParcelaPagamento` | Parcelas de cartão | Informativa, não afeta status de pagamento |
| `Usuario` | Equipe Cligen | E-mail, `tipo_login` (google/local), hash de senha (contas locais), `google_subject_id` (contas google), ativo — ver [autenticação](../02-arquitetura/autenticacao.md) |
| `TokenAcesso` | Código de uso único do portal | Com expiração |
| `LogAcesso` | Auditoria de prontuário | Quem, o quê, quando |
| ~~`Parametro`~~ | — | **Removida em 19/10/2026** (tabela dropada). Abrigava os dias de revisão; o prazo do catálogo já inclui a revisão (P13 derrubada) |

## Notas de projeto

**`Amostra` como tabela própria.** Modelada como tabela própria, e não como campos do exame, embora a premissa vigente seja de um acolhimento por exame (**P8**). As questões em aberto — uma coleta gerando vários exames (Q1.4), ou um exame exigindo várias amostras (Q1.5) — seguem sem resposta do cliente. Se qualquer uma for afirmativa, a relação vira um-para-muitos. Modelar como tabela desde já absorve essa resposta sem migration destrutiva: o custo hoje é uma junção a mais; o custo de errar é reescrever o cálculo de prazo depois de ter dados em produção.

**Cadastros auxiliares (`Origem`, `Destino`, `Convenio`).** Decisão do cliente em 17/10/2026 (fecha **C4.5** e **Q38.1**). A migration `20261019174706_CadastrosCatalogoEAjustesCliente` criou as três tabelas, semeou as 4 origens com GUIDs fixos (Cligen, Clínica parceira, Site Cligen, Plataforma) e converteu os textos livres já gravados em `Exame.Destino` e `Pagamento.Convenio` em linhas dos cadastros, trocando as colunas de texto pelas FKs `DestinoId` e `ConvenioId`; o enum `OrigemExame` virou a FK `OrigemId`. Ver [exame solicitado](../05-regras-negocio/exame-solicitado.md#cadastros-auxiliares).

**`Exame.Numero` e `Exame.DataAcolhimento`.** O número sequencial (`Seq`) dá o código legível `EX-000001` que a equipe usa para se referir ao exame. A data de acolhimento é copiada da `Amostra` vigente para o exame (e apagada na rejeição) apenas para permitir os filtros por período da página Fluxos sem junção.

## Camadas e agregados

A implementação segue arquitetura em camadas DDD (Domínio / Aplicação / Infraestrutura / Api) com princípios SOLID — ver [arquitetura do backend](../02-arquitetura/arquitetura-backend.md), incluindo a proposta de quais entidades acima são agregados com repositório próprio (`Paciente`, `ExameCatalogo`, `Exame`, `Pagamento`, `Usuario`, `Origem`, `Destino`, `Convenio`) e quais são acessadas através de outro agregado (`ResponsavelLegal` via `Paciente`; `Amostra`, `EtapaAndamento`, `Anexo`, `Comentario` via `Exame`; `ParcelaPagamento` via `Pagamento`).

## Relação com as regras de negócio

Cada entidade tem suas regras detalhadas no módulo correspondente de [`05-regras-negocio/`](../05-regras-negocio/):

- `Paciente`, `ResponsavelLegal` → [paciente.md](../05-regras-negocio/paciente.md)
- `ExameCatalogo` → [catalogo-exames.md](../05-regras-negocio/catalogo-exames.md)
- `Exame`, `Comentario`, `Anexo`, `Origem`, `Destino`, `Convenio` → [exame-solicitado.md](../05-regras-negocio/exame-solicitado.md)
- `Amostra` → [amostra-e-prazo.md](../05-regras-negocio/amostra-e-prazo.md)
- `EtapaAndamento` → [fluxo-laudo.md](../05-regras-negocio/fluxo-laudo.md)
- `Pagamento`, `ParcelaPagamento` → [financeiro.md](../05-regras-negocio/financeiro.md)
- `TokenAcesso` → [portal-paciente.md](../05-regras-negocio/portal-paciente.md)
- `LogAcesso` → [conformidade.md](../05-regras-negocio/conformidade.md)
- `Usuario` → [autenticação](../02-arquitetura/autenticacao.md) (login próprio + Google, via BFF)
