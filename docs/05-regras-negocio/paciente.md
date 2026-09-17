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

## Implementação (16/09/2026)

Decisões tomadas na implementação, onde a especificação não detalhava — **confirmar com o cliente** os itens marcados:

- **Responsável legal — campos (confirmar):** nome e documento (CPF ou passaporte) obrigatórios; parentesco livre e opcional. E-mail e telefone não se repetem: são os do cadastro (P9).
- **Responsável para maior de idade:** opcional, para cobrir o paciente sem contato próprio (P9). Para menor é obrigatório (Q7), e a menoridade é calculada pela data de Brasília.
- **CPF** validado pelos dígitos verificadores; **passaporte** aceita de 5 a 20 letras e números. Ambos gravados sem pontuação; a mensagem de duplicidade segue Q11 (*"Esse CPF já foi utilizado"* / *"Esse passaporte já foi utilizado"*).
- **Telefone** gravado em formato internacional (`+5531999998888`), exigido pela API do WhatsApp. Número sem `+` é tratado como brasileiro com DDD (celular com 9 à frente ou fixo); estrangeiro precisa começar com `+`.
- **Edição** permitida em todos os campos, inclusive o documento (correção de digitação) — com o aviso de que ele é o login do portal. **Não há exclusão** de paciente (prontuário).
- **Autoria:** o cadastro guarda quem criou e quem alterou por último.
- **Boas-vindas (Q32):** disparadas por e-mail e WhatsApp ao cadastrar. Os dois canais ainda só registram em log (provedores pendentes, texto pendente — C7), e o envio é síncrono até existir fila com retry.

## Pendente

- **D2.1** — número de cadastro sequencial curto e legível (ex. `2026-000123`) ao lado do GUID interno, para uso da recepção e do paciente (proposta aguardando aprovação do cliente). Ver [`06-pendencias/decisoes-e-conflitos.md`](../06-pendencias/decisoes-e-conflitos.md).
