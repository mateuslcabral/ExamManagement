# Regras de negócio — Cadastro de paciente

> Fonte: CLG-ESP-2026-001 §4

## Regras

- O cadastro é feito exclusivamente pela equipe Cligen. Não existe autocadastro (D1).
- O cadastro é único e persistente: exames posteriores se vinculam ao mesmo paciente, sem recadastro (D2).
- A chave interna do paciente é um identificador aleatório do tipo `uniqueidentifier`, sem qualquer relação com o CPF (C1).
- O documento — CPF ou passaporte (Q8) — possui **índice único**. A tentativa de cadastrar documento já existente é bloqueada com a mensagem *"Esse CPF já foi utilizado"* (Q11, Q12).
- E-mail e telefone são **obrigatórios**. Sem eles não há acesso ao portal, e portanto não há cadastro (Q10).
- Não são coletados sexo, nome social nem endereço (Q9).
- No cadastro, o sistema dispara automaticamente a mensagem de boas-vindas com as instruções de acesso ao portal (Q32) — ver [comunicação](comunicacao.md).

## Campos

| Campo | Obrigatório | Observação |
|---|---|---|
| Nome | sim | |
| Data de nascimento | sim | Determina se há necessidade de responsável legal |
| Tipo de documento | sim | CPF ou passaporte |
| Número do documento | sim | Índice único |
| E-mail | sim | Destino dos disparos |
| Telefone | sim | Destino dos disparos por WhatsApp |
| Responsável legal | condicional | Obrigatório para menor de idade (Q7) |

## Menor de idade

O responsável legal é atribuído no momento do cadastro (Q7). O e-mail e o telefone registrados são os do responsável, e é para ele que vão todos os disparos e o acesso ao portal (**P9**).

## Exclusão, edição e contato (premissas P16–P21)

- **Exclusão lógica** com autor, data e motivo obrigatório; restaurável. Bloqueada se houver exame ativo (regra entra com o módulo de exames). Ver **P16**.
- Documento editável para correção, mantendo unicidade; CPF validado por dígitos verificadores (**P18**).
- Maioridade aos 18 anos; o responsável legal registra nome, documento e parentesco — o contato (e-mail/telefone) do cadastro já é o do responsável (**P19**).
- Boas-vindas disparadas por e-mail/WhatsApp com texto provisório até C7 (**P20**); telefone guardado só com dígitos (**P21**).

Detalhes em [`06-pendencias/premissas.md`](../06-pendencias/premissas.md).

## Pendente

- **D2.1** — número de cadastro sequencial curto e legível (ex. `2026-000123`) ao lado do GUID interno, para uso da recepção e do paciente (proposta aguardando aprovação do cliente). Ver [`06-pendencias/decisoes-e-conflitos.md`](../06-pendencias/decisoes-e-conflitos.md).
