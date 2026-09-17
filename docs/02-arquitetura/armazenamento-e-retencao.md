# Armazenamento de arquivos e retenção de longo prazo

> Fonte: CLG-ESP-2026-001 §3, §10.3, §12 · CLG-PEND-2026-001 §1.3, §1.9, nota da §3

## Princípio

Arquivos (anexos de exame e laudos) nunca são gravados no banco de dados. O banco guarda apenas caminho, nome original, tipo, tamanho e hash; o binário fica em armazenamento de objetos externo.

Isso mantém o banco relacional pequeno mesmo com 20 anos de guarda, e permite usar SQL Server Express (gratuito, limite de 10 GB por banco) com folga.

## Retenção vs. exibição — não confundir

Os **6 meses** de disponibilidade do laudo no portal do paciente (Q34) limitam apenas a **exibição** ao paciente. Depois desse prazo:

- O paciente continua vendo o **registro** do exame no portal, sem o arquivo.
- **O arquivo não é apagado.** Permanece armazenado e acessível à equipe Cligen pelo prazo legal de guarda de prontuário (C3).

O termo de aceite do cadastro precisa deixar isso explícito por escrito (redação obrigatória, ver [conformidade](../05-regras-negocio/conformidade.md)).

## Risco de continuidade de 20 anos (ação A6 — em aberto)

No volume previsto (200 exames/mês, laudo médio 10 MB), o acúmulo é de ~24 GB/ano, ou ~480 GB ao fim de 20 anos. **O custo de armazenamento é irrelevante nesse volume.**

O risco real não é financeiro, é de **continuidade**: nenhum contrato de nuvem dura 20 anos. Consequência para a arquitetura:

- A arquitetura precisa prever **exportação íntegra do acervo** desde o início (formato, verificação de hash, portabilidade entre provedores).
- Isso deve ser resolvido antes de travar a escolha de provedor de object storage, não depois.

## Implementação provisória — disco local (16/09/2026)

Enquanto o provedor de object storage não é escolhido, `IArmazenamentoArquivos` é implementado em **disco local** (`ArmazenamentoArquivosLocal`, diretório `ArmazenamentoLocal:Diretorio`, padrão `App_Data/arquivos` na Api, fora do git). Trocar de provedor é trocar a implementação, sem tocar em Aplicação ou Domínio.

- Arquivos organizados por ano/mês de envio, com nome aleatório; o nome original fica só no banco.
- Bytes gravados **sem alteração** (requisito do laudo assinado), primeiro em arquivo temporário e depois movidos — upload interrompido não deixa arquivo parcial.
- **SHA-256** calculado na gravação e guardado no banco: base para a verificação de integridade e para a futura exportação íntegra do acervo.
- Nada é apagado pela aplicação.
- **Sem redundância:** não serve para produção sem backup do diretório.

## Em aberto

- Provedor de object storage (Azure Blob, S3, outro) e região de hospedagem (preferência por região no Brasil, dado de saúde)
- Política de backup do banco e do storage
- Formato e periodicidade da exportação íntegra do acervo
- Tamanho máximo de arquivo — premissa **P1**: 50 MB por arquivo, formatos PDF/JPG/PNG (Q24 sem resposta do cliente)

Ver [`06-pendencias/pendencias-abertas.md`](../06-pendencias/pendencias-abertas.md).
