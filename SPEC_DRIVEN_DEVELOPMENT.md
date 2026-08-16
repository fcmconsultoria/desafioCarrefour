# Como Responder: Spec-Driven Development

## Pergunta: "Como implementou o projeto utilizando Spec-Driven Development?"

---

## Resposta Curta:

"Segui uma abordagem **specification-first**: comecei com os requisitos do desafio como especificação, refinei em requisitos detalhados, criei diagramas para visualizar, e implementei cada feature baseado nessa especificação. Os testes validam se a especificação foi atendida."

---

## Resposta Detalhada:

### 1. COMECEI COM A ESPECIFICAÇÃO (Requisitos do Desafio)

**O que fiz:**
- Li os requisitos do desafio como a "especificação inicial"
- Identifiquei: 2 serviços (lançamentos + consolidado), resiliência, escalabilidade, 50 req/s, max 5% perda

**Arquivo:** `passoapasso/README.md` (linhas 9-22)

---

### 2. REFINEI A ESPECIFICAÇÃO (Requisitos Detalhados)

**O que fiz:**
- Transformei os requisitos em RF (Requisitos Funcionais) e RNF (Requisitos Não-Funcionais)
- Para cada requisito, defini critérios de aceite claros
- Priorizei os requisitos

**Arquivo:** `passoapasso/requisitos_detalhados.md`

**Exemplo:**
```
RF-001: Registrar Lançamento Financeiro
Critérios de Aceite:
- Deve aceitar lançamentos do tipo "débito" e "crédito"
- Deve registrar valor numérico positivo
- Deve gerar identificador único
```

---

### 3. VISUALIZEI A ESPECIFICAÇÃO (Diagramas)

**O que fiz:**
- Criei diagramas para visualizar a especificação
- Diagrama de contexto → como os componentes interagem
- Diagrama de fluxo → como os dados fluem
- Diagrama de componentes → estrutura interna

**Arquivo:** `diagrams/` (8 diagramas)

**Como isso é Spec-Driven:**
- Diagramas são a "especificação visual"
- Código foi implementado para seguir os diagramas
- Se diagrama muda, código deve mudar

---

### 4. DOCUMENTEI DECISÕES BASEADAS NA ESPECIFICAÇÃO (ADRs)

**O que fiz:**
- Para cada decisão técnica, documentei:
  - Contexto (qual requisito da especificação)
  - Decisão (o que escolhi)
  - Consequências (impacto na especificação)

**Arquivo:** `passoapasso/decisoes_arquiteturais.md`

**Exemplo:**
```
ADR-001: Event-Driven Architecture
Contexto: Requisito RNF-001 - Ledger não pode depender de Consolidation
Decisão: Usar RabbitMQ para comunicação assíncrona
Consequências: Atende especificação de resiliência
```

---

### 5. IMPLEMENTEI BASEADO NA ESPECIFICAÇÃO

**O que fiz:**
- Cada feature no código atende a um requisito específico
- Mapeamento direto: Requisito → Código

**Exemplo:**
```
Requisito: "Serviço de controle de lançamentos"
Código: src/LedgerService/Controllers/LancamentosController.cs
Endpoint: POST /api/lancamentos
```

```
Requisito: "Não depender do consolidado"
Código: src/LedgerService/Messaging/RabbitMQEventPublisher.cs
Comunicação: Assíncrona via fila
```

---

### 6. TESTES VALIDAM A ESPECIFICAÇÃO

**O que fiz:**
- Cada teste valida um requisito da especificação
- Se teste passa, especificação foi atendida

**Arquivos:** `src/LedgerService.Tests/` e `src/ConsolidationService.Tests/`

**Exemplo:**
```csharp
[Fact]
public async Task CreateLancamentoAsync_WithExistingIdempotencyKey_ShouldReturnCachedResponse()
{
    // Valida requisito: "Prevenir duplicações em retries"
    // → Especificação de confiabilidade
}
```

---

## COMO EXPLICAR NA ENTREVISTA:

### Resposta em 3 pontos:

**1. Especificação Inicial:**
"Comecei com os requisitos do desafio como minha especificação inicial. Li cada requisito e entendi o que era esperado."

**2. Refinamento da Especificação:**
"Refinei a especificação em requisitos detalhados (funcionais e não-funcionais), com critérios de aceite claros. Isso é o coração do Spec-Driven Development - ter uma especificação clara antes de codificar."

**3. Implementação Guiada pela Especificação:**
"Cada linha de código foi escrita para atender a um requisito específico. Os diagramas visualizam a especificação, os ADRs documentam decisões baseadas na especificação, e os testes validam se a especificação foi atendida."

---

## SE PERGUNTAREM: "Mas não usou ferramenta X de Spec-Driven?"

### Resposta:

"Não usei uma ferramenta específica de Spec-Driven Development como Cucumber ou SpecFlow, mas segui a **filosofia** de Spec-Driven Development:

- Especificação primeiro (requisitos detalhados)
- Visualização da especificação (diagramas)
- Implementação guiada pela especificação
- Testes validando a especificação

Isso atende o princípio de Spec-Driven Development mesmo sem usar ferramentas específicas. O importante é ter uma especificação clara e implementar baseado nela."

---

## EXEMPLO DE RASTREABILIDADE (Spec → Código):

| Especificação | Implementação | Teste |
|--------------|---------------|-------|
| RF-001: Registrar lançamento | POST /api/lancamentos | Test_CreateLancamentoAsync |
| RNF-001: Resiliência | RabbitMQ async | Test_Resilience |
| RNF-002: 50 req/s | Docker replicas | Test_Scalability |
| RNF-003: Max 5% perda | Retry + DLQ | Test_Reliability |

---

## BENEFÍCIOS DESSA ABORDAGEM:

1. **Clareza:** Especificação clara antes de codificar
2. **Rastreabilidade:** Cada requisito → código → teste
3. **Manutenibilidade:** Se requisito muda, sei onde mudar
4. **Comunicação:** Especificação pode ser compartilhada com times de negócio
5. **Validação:** Testes provam que especificação foi atendida

---

## NA ENTREVISTA, FALA ASSIM:

"Implementei seguindo uma abordagem specification-first, que é o princípio do Spec-Driven Development.

1. **Especificação inicial:** Os requisitos do desafio
2. **Refinamento:** Detalhei em RF e RNF com critérios de aceite
3. **Visualização:** Criei diagramas para visualizar a especificação
4. **Implementação:** Cada feature atende a um requisito específico
5. **Validação:** Testes verificam se a especificação foi atendida

Não usei ferramentas específicas como Cucumber, mas segui a filosofia: especificação clara → implementação guiada → testes validando. Isso garante que o código atende exatamente o que foi especificado."

---

## Documentos que provam Spec-Driven:

1. **requisitos_detalhados.md** - Especificação refinada
2. **diagramas/** - Especificação visual
3. **decisoes_arquiteturais.md** - Decisões baseadas na especificação
4. **CORRELACAO_DIRETA.md** - Mapeamento requisito → implementação
5. **Testes/** - Validação da especificação

---

**O importante não é a ferramenta, mas a abordagem: especificação clara → implementação guiada → validação.** ✅
