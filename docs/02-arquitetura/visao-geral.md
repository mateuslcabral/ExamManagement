# Visão geral de arquitetura

> Deriva de CLG-ESP-2026-001 §1, §3. Este documento cobre o que já foi decidido; o restante do planejamento de arquitetura (não decidido ainda) está listado em "Em aberto" ao final e detalhado em [`06-pendencias/`](../06-pendencias/).

## Aplicações do sistema

O projeto tem duas superfícies de uso distintas, cada uma com **seu próprio app Next.js/React**, atuando como BFF (ver [autenticação](autenticacao.md)):

1. **Sistema interno** (`frontend/sistema`) — operado exclusivamente pela equipe Cligen (4 usuários). Cadastro de pacientes, catálogo de exames, exames solicitados, acolhimento de amostra, fluxo de laudo, financeiro, disponibilização de laudo. Login OAuth (próprio + Google).
2. **Portal do paciente** (`frontend/portal`, `https://resultados.cligen.com.br`) — acesso só de consulta, sem cadastro ou edição (D1). Login por documento + código de uso único, sem senha armazenada.

Dois apps separados, não um único com rotas — os dois perímetros de segurança e modelos de autenticação são incompatíveis demais para compartilhar o mesmo deploy. Ambos falam com o mesmo backend (ASP.NET Core) e o mesmo banco (SQL Server), mas **nunca diretamente do navegador** — sempre pelo runtime server-side de cada Next.js.

## Componentes

- **Backend** — ASP.NET Core (C#) + Entity Framework Core. API pura (nenhuma renderização de página), chamada apenas pelos dois BFFs Next.js, nunca pelo navegador diretamente. Organizado em camadas DDD (Domínio, Aplicação, Infraestrutura, Api) — ver [arquitetura do backend](arquitetura-backend.md).
- **Frontend** — dois apps Next.js (React), um por superfície, cada um atuando como BFF de autenticação e sessão. Ver [autenticação](autenticacao.md).
- **Banco de dados** — SQL Server. Guarda apenas dados estruturados; nenhum binário de arquivo.
- **Object storage** — armazenamento externo ao banco para todos os anexos e laudos (PDF, JPG, PNG). O banco referencia por caminho + hash.
- **Mensageria** — integração com WhatsApp Business API oficial e provedor de e-mail transacional, disparados em dois momentos: cadastro do paciente (boas-vindas) e disponibilização de laudo.

## Integridade e conformidade (resumo)

O sistema trata o registro de exame como **prontuário médico**:

- Exclusão sempre lógica, nunca física (autor, data e motivo obrigatórios).
- Log de acesso a laudos obrigatório (quem, o quê, quando).
- Laudo assinado fora da plataforma; o sistema apenas anexa e preserva, sem reprocessar (nenhuma recompressão/conversão, sob pena de invalidar a assinatura).

Detalhamento completo em [`05-regras-negocio/conformidade.md`](../05-regras-negocio/conformidade.md).

## Em aberto (arquitetura ainda não decidida)

Frontend, autenticação da equipe interna e organização do backend já foram fechados — ver [stack](../01-stack/stack-tecnologica.md), [autenticação](autenticacao.md) e [arquitetura do backend](arquitetura-backend.md). Seguem sem decisão:

- Jobs em background (envio de e-mail/WhatsApp com retry e fila, expiração de disponibilidade do laudo no portal após 6 meses)
- Ambientes (dev/homolog/produção), CI/CD, estratégia de migrations do EF Core
- Backup e disaster recovery do banco e do storage
- Hospedagem: nuvem, região, responsável pela contratação (ação A6 da ata)
- Mecanismo de autenticação servidor-a-servidor entre cada BFF e a API (ver [autenticação](autenticacao.md))

Ver [`06-pendencias/pendencias-abertas.md`](../06-pendencias/pendencias-abertas.md).
