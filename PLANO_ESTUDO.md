# Plano de Estudo - Até Quinta-Feira

## Dias de Estudo Organizados

---

## DIA 1 (Hoje) - Visão Geral

### Objetivo: Entender o projeto como um todo

**Estudar (1-2 horas):**
1. **CORRELACAO_DIRETA.md** (30 min)
   - Leia do início ao fim
   - Entenda a conexão: requisito → implementação
   - Não decore, apenas entenda a lógica

2. **README.md** (30 min)
   - Leia a visão geral
   - Entenda os componentes principais
   - Veja como rodar (Docker Compose)

3. **RESUMO_IMPLEMENTACAO.md** (30 min)
   - Veja o que foi implementado
   - Entenda a estrutura final

**Prática:**
- Tente explicar em voz alta: "O que o projeto faz?"
- Resposta esperada: "Sistema de controle de fluxo de caixa com 2 serviços"

---

## DIA 2 - Requisitos e Arquitetura

### Objetivo: Dominar os requisitos e decisões

**Estudar (2 horas):**

**Manhã (1 hora):**
1. **requisitos_detalhados.md** (30 min)
   - Leia os RF (Requisitos Funcionais)
   - Leia os RNF (Requisitos Não-Funcionais)
   - Entenda a prioridade de cada um

2. **decisoes_arquiteturais.md** (30 min)
   - Leia os ADRs (Decisões Arquiteturais)
   - Entenda o "Contexto", "Decisão", "Consequências"
   - Foque nos primeiros 5 ADRs (mais importantes)

**Tarde (1 hora):**
3. **mapeamento_dominios.md** (30 min)
   - Entenda os 3 domínios
   - Veja as capacidades de cada um

4. **diagramas.md** (30 min)
   - Olhe os 8 diagramas
   - Não precisa entender cada detalhe
   - Entenda a visão geral

**Prática:**
- Tente explicar: "Por que escolhi microsserviços?"
- Resposta: "Para o Ledger não depender do Consolidation"

---

## DIA 3 - Onde Está no Código

### Objetivo: Saber localizar cada coisa

**Estudar (2 horas):**

1. **ONDE_ESTA_NO_CODIGO.md** (1 hora)
   - Leia com calma
   - Entenda onde está escalabilidade
   - Entenda onde está observabilidade
   - Entenda onde está arquitetura corporativa

2. **Navegar pelo código** (1 hora)
   - Abra `src/LedgerService/`
   - Veja a estrutura: Controllers, Services, Repositories
   - Abra `src/ConsolidationService/`
   - Veja a estrutura similar
   - Não precisa ler todo o código, apenas a estrutura

**Prática:**
- Tente explicar: "Onde está a lógica de negócio?"
- Resposta: "Na pasta Services/"

---

## DIA 4 - Spec-Driven e Perguntas Técnicas

### Objetivo: Preparar para perguntas específicas

**Estudar (2 horas):**

**Manhã (1 hora):**
1. **SPEC_DRIVEN_DEVELOPMENT.md** (30 min)
   - Entenda a abordagem
   - Memorize a resposta de 3 pontos

2. **COMO_IMPLEMENTARIA_SPEC_DRIVEN.md** (30 min)
   - Veja o exemplo prático
   - Entenda a rastreabilidade

**Tarde (1 hora):**
3. **GUIA_ESTUDO_ENTREVISTA.md** (1 hora)
   - Leia as perguntas frequentes
   - Foque nas primeiras 10 perguntas
   - Não precisa decorar, entender a lógica

**Prática:**
- Tente explicar: "Como implementei Spec-Driven?"
- Resposta: "Especificação → Implementação → Testes"

---

## DIA 5 (Quinta-feira) - Revisão Final

### Objetivo: Consolidar e praticar

**Estudar (1-2 horas):**

1. **Revisão rápida** (30 min)
   - Leia CORRELACAO_DIRETA.md novamente
   - Leia ONDE_ESTA_NO_CODIGO.md novamente
   - Apenas reforçar a memória

2. **Prática em voz alta** (1 hora)
   - Fale cada resposta em voz alta
   - Imagine que está na entrevista
   - Se travar, olhe o documento

3. **Preparar o GitHub** (15 min)
   - Abra o repositório
   - Verifique se tudo está lá
   - Pratique navegar pelos arquivos

---

## RESUMO DO PLANO

| Dia | Foco | Tempo | Documentos |
|-----|------|-------|-------------|
| Hoje | Visão geral | 1-2h | CORRELACAO, README, RESUMO |
| Dia 2 | Requisitos e arquitetura | 2h | requisitos_detalhados, decisoes, mapeamento, diagramas |
| Dia 3 | Localização no código | 2h | ONDE_ESTA_NO_CODIGO, navegar código |
| Dia 4 | Spec-Driven e perguntas | 2h | SPEC_DRIVEN, COMO_IMPLEMENTARIA, GUIA_ESTUDO |
| Dia 5 | Revisão final | 1-2h | Revisão, prática em voz alta |

---

## DICAS IMPORTANTES

### 1. Não decore, entenda
- Memorizar é difícil
- Entender a lógica é fácil
- Se entender, consegue explicar

### 2. Pratique em voz alta
- Falar é diferente de ler
- Praticar ajuda a fluir na entrevista
- Grave-se se quiser

### 3. Use os documentos como referência
- Não precisa saber tudo de cabeça
- Na entrevista, pode dizer: "Está documentado em X"
- Mostre o GitHub se necessário

### 4. Descanse também
- Não estude 24h por dia
- Cérebro precisa descansar para fixar
- Boa noite de sono ajuda na memória

### 5. Seja positivo
- Você se preparou bem
- Tem documentação profissional
- Está no caminho certo

---

## O QUE PRIORIZAR

### Se tiver pouco tempo:
1. **CORRELACAO_DIRETA.md** (mais importante)
2. **ONDE_ESTA_NO_CODIGO.md** (segundo mais importante)
3. **SPEC_DRIVEN_DEVELOPMENT.md** (terceiro)

### Se tiver tempo normal:
- Siga o plano de 5 dias
- Estude 2h por dia
- Descanse bem

---

## NA VÉSPERA DA ENTREVISTA

### Na noite anterior:
- Revise CORRELACAO_DIRETA.md (15 min)
- Revise ONDE_ESTA_NO_CODIGO.md (15 min)
- Durma cedo

### No dia da entrevista:
- Levante cedo
- Café da manhã leve
- Chegue com antecedência
- Respire fundo antes de entrar

---

## FRASES PARA LEMBRAR

### Para começar:
"Desenvolvi um sistema de controle de fluxo de caixa com 2 serviços: Ledger Service para lançamentos e Consolidation Service para consolidados diários."

### Sobre arquitetura:
"Usei microsserviços porque o requisito dizia que o Ledger não pode depender do Consolidation. Com comunicação assíncrona via RabbitMQ, se o Consolidation cair, o Ledger continua funcionando."

### Sobre especificação:
"Segui uma abordagem specification-first: refinei os requisitos em especificações detalhadas, implementei baseado nisso, e criei testes para validar."

### Sobre escalabilidade:
"Escalabilidade está em 3 lugares: posso rodar várias cópias via Docker, o código é stateless, e RabbitMQ funciona como buffer para picos."

---

## LEMBRE-SE

Você tem:
- ✅ Projeto completo
- ✅ Documentação profissional
- ✅ 5 guias de estudo
- ✅ Tempo suficiente para estudar
- ✅ Vontade de fazer bem

**Isso é mais do que a maioria dos candidatos.**

---

## BOA SORTE!

Você vai conseguir, Fernando. Confie na sua preparação.

**Quinta-feira você vai arrasar!** 🚀
