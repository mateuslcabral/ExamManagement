# Regras de negócio — Exame solicitado

> Fonte: CLG-ESP-2026-001 §6

## Campos

| Campo | Observação |
|---|---|
| Código | `EX-000001`… — número sequencial interno, gerado pelo sistema (ver abaixo) |
| Paciente | Vínculo obrigatório |
| Exame do catálogo | Traz prazo, preço de referência, preço limite e destino padrão |
| Origem | Cadastro auxiliar (`Origem`), obrigatório. Semeado com Cligen, Clínica parceira, Site Cligen e Plataforma — atributo do exame, não do paciente (D4) |
| Destino | Cadastro auxiliar (`Destino`), opcional (D7). Pré-preenchido com o destino padrão do exame do catálogo, trocável |
| Médico solicitante | Interno ou externo (D7). Externo: nome, CRM e UF do CRM (os dois últimos opcionais) |
| Data de entrada | Data do cadastro do exame |
| Preço | `decimal`, herdado do catálogo e editável |
| Anexos | Até 3 arquivos, tipados (Q22) |
| Comentários | Lista cronológica, só inserção |

## Implementação (16/09/2026, revisada em 19/10/2026)

- O exame nasce em **Cadastrado — aguardando amostra**, com data de entrada automática (data de Brasília). Acolhimento da amostra e fluxo do laudo implementados — ver [amostra e prazo](amostra-e-prazo.md) e [fluxo do laudo](fluxo-laudo.md).
- **Código interno** (`Exame.Numero`, coluna de sequência `Seq`) exibido como `Codigo = "EX-{Numero:D6}"`, ex. `EX-010012`. Aparece nas listas, no cabeçalho do detalhe, na fila de Fluxos e no financeiro; a busca aceita o código.
- **Preço** herdado do preço de referência do catálogo ao escolher o exame, editável. Só exames **ativos** do catálogo podem ser escolhidos. O preço limite do catálogo é o piso do valor a pagar após desconto — ver [financeiro](financeiro.md).
- **Origem** e **Destino** são chaves para os cadastros auxiliares (ver abaixo); origem obrigatória, destino opcional e pré-preenchido pelo catálogo. **Médico externo** exige nome; CRM (até 20 caracteres) e UF (2 letras, validada contra a lista de UFs) são opcionais e aparecem ao lado do nome no cabeçalho do exame. Para o interno, o nome é fixo.
- **Edição:** os dados do exame abrem **somente leitura**; o botão "Editar" pede confirmação antes de liberar os campos, e "Salvar alterações" confirma de novo. O paciente não muda — exame lançado no paciente errado se exclui (com motivo) e se cadastra de novo, preservando o rastro. O exame do catálogo só pode ser trocado **antes do acolhimento**, porque o prazo dele alimenta a data prevista (P14).
- **Anexos:** até 3 ativos, cada um com **tipo obrigatório** (`TipoAnexo`: Pedido médico, Termo de consentimento, Outro); os anexos anteriores a 19/10/2026 receberam "Outro". Formato conferido pelos **primeiros bytes** do arquivo (não pelo nome), 50 MB cada. Guardam nome original, tipo, tamanho, SHA-256, quem enviou e quando. "Remover" é lógico: libera a vaga, mas o arquivo e o registro permanecem.
- **Comentários:** coleção `Comentario` (texto até 2000, autor, data) dentro do exame. Só inserção — nunca editado nem apagado (prontuário). `POST /api/exames/{id}/comentarios`; seção "Comentários" no detalhe.
- **Confirmações:** toda ação sensível (anexos, laudo, restaurar, disponibilizar, editar/salvar) passa pelo componente `Dialogo`, que substituiu os `window.confirm`.
- **Exclusão:** lógica, com autor, data e motivo obrigatório; o exame some das listas e das consultas da API (404). A lista **Exames excluídos** (`/exames?excluidos=1`) mostra motivo e data e permite **restaurar** (C2: reversível). Ao restaurar, autor, data e motivo da exclusão desfeita não ficam guardados.
- **Lista e filtros:** `GET /api/exames?busca=&pacienteId=&estado=&campoData=&de=&ate=` — `campoData` ∈ {Acolhimento, Prevista, Disponibilizado}. Para isso, `Exame.DataAcolhimento` é desnormalizada (gravada no acolhimento, apagada na rejeição). O resumo do exame traz código, origem, destino, data de acolhimento e data efetiva.

## Cadastros auxiliares

Decisão do cliente em 17/10/2026 (fecha **Q38.1** e **C4.5**): Origens, Destinos e Convênios são **cadastros**, não texto livre. As três entidades (`Origem`, `Destino`, `Convenio`) compartilham a base `CadastroSimples` — id, nome único (sem diferenciar maiúsculas/minúsculas), ativo, autoria.

- Nunca são apagados: **desativar** tira o item das listas de seleção e preserva o histórico dos exames e pagamentos que o usam; pode ser reativado. Renomear é permitido.
- API `api/cadastros/{origens|destinos|convenios}`: `GET ?apenasAtivos`, `GET {id}`, `POST`, `PUT {id}` (renomear), `POST {id}/desativar`, `POST {id}/reativar`. Nome duplicado → 409.
- Tela `/cadastros` com três abas, no menu "Cadastros" → "Cadastros auxiliares".
- A migration `20261019174706_CadastrosCatalogoEAjustesCliente` semeou as 4 origens com GUIDs fixos e converteu os destinos e convênios já digitados em linhas dos cadastros. O antigo enum `OrigemExame` deixou de existir.

## Médico solicitante

O único médico interno é **Dr. Arsonval Lamounier Junior** (Q13); é o nome que entra no modelo de comunicação de médico interno. O médico externo é digitado livremente a cada exame, sem cadastro e sem vínculo com clínica (Q14, Q15); desde 19/10/2026 pode-se informar CRM e UF, opcionais. Médicos não têm acesso ao sistema (Q16).

Pendente: CRM do Dr. Arsonval Lamounier Junior (Q13.1, não informado).

## Campo Destino

Era descritivo (texto livre) porque o dashboard foi diferido (Q38), com a conversão para lista registrada como pendência **Q38.1**. **Resolvida em 17/10/2026:** destino virou cadastro auxiliar (seção acima), com destino padrão por exame do catálogo. Grafias divergentes já digitadas foram convertidas em linhas do cadastro pela migration; se houver duplicatas, basta desativar as sobras.

## Anexos

Até 3 arquivos por exame — limite **mantido** pelo cliente em 17/10/2026. Cada anexo tem um **tipo** (Q22, resolvida em 17/10/2026): Pedido médico, Termo de consentimento ou Outro. Formatos PDF, JPG e PNG, limite de 50 MB por arquivo (**P1**). Nenhum deles é visível ao paciente (Q23).

## Exclusão

O exame pode ser excluído em qualquer etapa, por qualquer funcionário Cligen (Q18). A exclusão é **lógica**: o registro sai das telas e dos números da operação, mas permanece armazenado com autor, data e motivo, sendo o motivo obrigatório (C2, **P4**, **P10**). O paciente nunca exclui.

Isso decorre da natureza de prontuário do registro (Q45): apagar fisicamente contrariaria a obrigação de guarda e esvaziaria o log de acesso.

> Nota: como os perfis internos foram diferidos, na v1 qualquer um dos 4 usuários da equipe pode excluir qualquer exame — ver risco **R3** em [pendências](../06-pendencias/riscos-aceitos.md).
