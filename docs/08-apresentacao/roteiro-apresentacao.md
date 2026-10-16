# Roteiro de apresentação para a Cligen

> Duração sugerida: 60 min (40 de demonstração, 20 de perguntas). Público: direção, recepção e laboratório da Cligen. Objetivo: mostrar o ciclo completo funcionando e **sair da reunião com as respostas da seção 6**.

## Antes da reunião

- [ ] Subir API e frontend; entrar com o usuário inicial.
- [ ] Banco com: catálogo com 3 ou 4 exames **com nomes reais da Cligen** (pedir a lista antes; se não vier, avisar que os nomes são exemplo), 2 pacientes (um adulto, um menor com responsável), 1 exame já disponibilizado (para mostrar o fim do fluxo) e 1 exame aguardando amostra (para acolher ao vivo).
- [ ] Um PDF de laudo real ou de exemplo, e um JPG, na área de trabalho.
- [ ] Abrir o log da API numa janela ao lado, para mostrar as mensagens "disparadas".
- [ ] Imprimir ou abrir `pendencias.md` (seção 1) para a rodada de perguntas.
- [ ] Combinar quem anota as respostas.

## 1. Abertura (5 min)

**Dizer:**
- O que vai ser mostrado: o ciclo inteiro de um exame, do cadastro do paciente até o laudo liberado e o pagamento registrado, no sistema real, não em protótipo.
- O que **não** existe ainda, para não gerar expectativa: portal do paciente, envio real de e-mail e WhatsApp, acesso com Google, servidor em nuvem. Tudo isso tem motivo (depende de decisões que serão pedidas no fim).
- Pedir que interrompam quando algo na tela não bater com o dia a dia deles — é para isso que a reunião existe.

## 2. Cadastro do paciente (7 min) — quem opera: recepção

1. Menu **Pacientes** → **Novo paciente**. Cadastrar ao vivo um paciente adulto com os dados que a recepção ditar.
   - Mostrar: CPF inválido é recusado na hora; telefone aceita "(31) 99999-8888"; cadastrar o mesmo CPF de novo dá "Esse CPF já foi utilizado".
2. Mudar a data de nascimento para um menor: a seção **Responsável legal** aparece sozinha e fica obrigatória.
   - **Perguntar:** "Do responsável, nome, documento e parentesco bastam?"
   - **Perguntar:** "O e-mail e o telefone de um menor são sempre os do responsável?"
3. Mostrar a busca por nome e por CPF com pontuação.
4. Apontar no log: a mensagem de boas-vindas "saiu" (é onde entrará o e-mail/WhatsApp real).

## 3. Exame solicitado (6 min) — recepção

1. Na linha do paciente, **Novo exame**. Escolher o exame do catálogo: o preço vem preenchido; mostrar que pode ser ajustado. Origem, médico interno/externo.
   - **Perguntar:** "O campo Destino: vocês têm uma lista fixa de laboratórios? Se sim, vira lista."
2. Salvar. Mostrar o estado "Aguardando amostra" e que **não há data prevista ainda** — explicar que o prazo só começa quando a amostra chega.
3. Anexar o JPG e tentar anexar um arquivo renomeado (opcional): o sistema confere o conteúdo.
4. Mostrar a exclusão com motivo e a lista de excluídos com Restaurar — reforçar: nada é apagado, é prontuário.

## 4. Amostra e prazo (7 min) — laboratório

1. Menu **Amostras**: a fila do que ainda não chegou. Na linha, a data vem com hoje; mudar para ontem e mostrar a previsão recalculando antes de confirmar. **Acolher**.
   - Explicar a conta: acolhimento + prazo do exame + 3 dias de revisão.
   - **Perguntar:** "Quem registra a chegada da amostra e onde? A data é digitada (amostra de sábado lançada na segunda) ou automática?"
   - **Perguntar:** "Existe horário de corte?"
   - **Perguntar:** "Uma coleta pode servir para mais de um exame? Um exame pode precisar de duas amostras?"
2. Abrir o exame: seção Amostra mostra a data, a previsão e quem lançou. Mostrar **Rejeitar amostra** com motivo: a previsão some e o exame volta para a fila; o próximo acolhimento aparece como "recoleta" com data nova.
   - **Perguntar (importante):** "Quando a amostra é rejeitada e recoletada, a data prometida ao paciente é a nova, certo? A antiga deixa de valer?"
3. Catálogo → mostrar os **dias de revisão** editáveis e que mudar não altera datas já prometidas.
   - **Perguntar:** "Os 3 dias valem para todos os exames?"

## 5. Fluxo do laudo (10 min) — laboratório e direção

1. Menu **Laudos**: a fila agrupada pela próxima ação; data prevista vencida fica em vermelho.
2. Abrir o exame acolhido: **Registrar etapa** com o PDF → "Laudo parceiro pronto", data automática. Tentar pular para a etapa 5: o sistema recusa.
   - **Perguntar:** "A etapa do parceiro existe mesmo quando o exame é feito internamente? Hoje é obrigatória."
3. **Substituir arquivo** da etapa: data e estado não mudam; o contador registra.
4. Registrar as etapas 4 e 5. Botão **Disponibilizar ao paciente** → confirmação → data efetiva gravada; mostrar no log as duas mensagens (modelo médico interno/externo).
5. Substituir o laudo já disponibilizado: o sistema permite, pede confirmação e **não avisa o paciente** — explicar que foi o que o cliente aceitou (R2) e confirmar que continua assim.
   - **Perguntar:** "Alguma vez precisam voltar uma etapa? Desfazer uma disponibilização?"
   - **Perguntar:** "Os textos das mensagens: quem escreve e quem aprova? Lembrar que o WhatsApp precisa de aprovação da Meta, que demora."

## 6. Financeiro (7 min) — recepção e direção

1. No exame: aplicar um desconto com motivo; valor a pagar muda. Lançar parte em Pix e parte em cartão 3x — "Quitado".
2. Em outro exame: lançar convênio "Unimed" com guia → fica **a receber**; marcar **Receber** com a data; mostrar **Glosar** com motivo.
3. Menu **Financeiro**: relatório do mês, totais por método, a receber por convênio. Mudar o período.
   - **Perguntar (decisivo):** "Convênio precisa ser um cadastro (Unimed, Bradesco…) para o relatório somar certo, ou texto basta?"
   - **Perguntar:** "O número da guia é obrigatório?"
   - **Perguntar:** "Paciente particular sempre paga no ato? Nunca fica devendo?"
   - **Perguntar:** "Precisam exportar para Excel?"

## 7. Usuários e segurança (3 min) — direção

1. **Gestão de Usuários**: criar um usuário local (recebe link para definir a própria senha), desativar/reativar. Explicar: desativado cai na hora.
2. Dizer com clareza: nesta versão **todos os usuários fazem tudo** (excluir exame, mudar dias de revisão). Confirmar que é aceitável até os perfis entrarem.
   - **Perguntar:** "O login com Google é com Gmail pessoal de cada um ou a Cligen tem Google Workspace?"
   - **Perguntar:** "Regras de senha: tamanho, troca periódica, bloqueio por tentativas?"

## 8. O que falta e o que precisamos de vocês (10 min)

Mostrar a lista curta:

**Falta construir:** portal do paciente · envio real de e-mail e WhatsApp · arquivos em nuvem · log de acesso a laudos · login Google · ambiente de produção com backup.

**Precisamos de vocês (levar impresso):**
1. Lista oficial dos exames: nome, prazo, preço.
2. Textos das três mensagens (boas-vindas, laudo com médico interno, laudo com médico externo).
3. Acesso à conta do WhatsApp Business e ao DNS de cligen.com.br (ou quem tem).
4. Hospedagem: quem contrata e onde (recomendação: nuvem no Brasil).
5. CRM do Dr. Arsonval.
6. As respostas das perguntas feitas durante a demonstração (lista em `pendencias.md`, seção 1).

Fechar com: data para a próxima rodada e quem é o contato para cada item.

## Se perguntarem…

- **"Quando o paciente vai conseguir ver o laudo?"** — Depois do portal e das mensagens reais; as duas coisas dependem dos itens 2 e 3 acima. O portal em si leva de 2 a 3 semanas depois disso.
- **"Dá para usar já?"** — Internamente, sim, para teste com dados reais em uma máquina da clínica, **desde que** haja backup da pasta de arquivos e do banco. Para produção de verdade, só depois da hospedagem.
- **"E se o laudo tiver um erro depois de liberado?"** — Substitui-se o arquivo; o paciente não é avisado e a versão anterior fica no disco sem referência. Laudo retificado com histórico ficou para a versão 2, por decisão de escopo.
- **"Os dados estão seguros?"** — Hoje estão numa máquina local, sem redundância. É o motivo de a hospedagem ser o item mais urgente.
- **"Pode apagar um paciente cadastrado errado?"** — Não. Hoje não há saída para duplicidade; é uma das perguntas da lista.
