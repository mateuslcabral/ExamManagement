# Regras de negócio — Portal do paciente

> Fonte: CLG-ESP-2026-001 §10

**Endereço:** `https://resultados.cligen.com.br`

## Acesso

O acesso se dá pelo **número do documento cadastrado mais um código de uso único**, enviado na hora por e-mail e WhatsApp (Q33). Não existe senha armazenada.

Essa decisão substitui o modelo original de login e senha derivados de CPF (ação A1 da ata, encerrada). Elimina senha fraca, senha esquecida e fluxo de recuperação, e retira do sistema a responsabilidade de guardar credencial de paciente.

No caso de menor de idade, quem acessa é o responsável legal, com os dados de contato registrados no cadastro (Q7, **P9**).

> **Pendente de especificação técnica** (não coberto pelos documentos formais): formato do código de uso único, validade (minutos?), número de tentativas permitidas, rate limit por documento, expiração de sessão após login. A entidade `TokenAcesso` existe no [modelo de dados](../04-schema/modelo-de-dados.md) com campo de expiração, mas as regras finas ainda precisam ser definidas — ver [pendências](../06-pendencias/pendencias-abertas.md).

## O que o paciente vê

- A lista de **todos** os seus exames (Q34).
- Apenas o **laudo revisado e disponibilizado** pela equipe. Nenhum outro anexo, nenhuma versão intermediária (Q17, Q23).
- Ao tentar acessar exame ainda não liberado: *"Seu exame ainda não está liberado. Te comunicaremos quando disponível."* (D12)
- A data prevista de liberação **não** é exibida nesta versão (**P15**).

## Disponibilidade do laudo

O PDF fica disponível para download por **6 meses** após a liberação. Depois desse prazo, o paciente continua vendo o registro do exame, mas sem o arquivo (Q34).

**O arquivo não é apagado.** Os 6 meses limitam a exposição no portal, não a retenção — ver [armazenamento e retenção](../02-arquitetura/armazenamento-e-retencao.md) (C3).

**Redação obrigatória no termo de aceite do cadastro:** que o laudo fica disponível para download por 6 meses; que após esse período o registro permanece visível; e que o documento continua guardado pela clínica, podendo ser solicitado.
