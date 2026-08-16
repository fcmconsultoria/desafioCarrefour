# Como Implementaria Utilizando Spec-Driven Development

## Exemplo Prático: Do Requisito ao Código

Vou mostrar o fluxo completo de como implementaria usando Spec-Driven Development, usando um exemplo real do projeto.

---

## FLUXO COMPLETO: Requisito → Especificação → Código → Teste

### PASSO 1: REQUISITO INICIAL (Do Desafio)

**Requisito:** "Serviço que faça o controle de lançamentos"

---

### PASSO 2: ESPECIFICAÇÃO DETALHADA (Spec-Driven)

**Eu transformaria em:**

#### 2.1. User Story (Formato tradicional)
```
Como comerciante
Quero registrar lançamentos financeiros (débitos e créditos)
Para controlar meu fluxo de caixa diário
```

#### 2.2. Critérios de Aceite (Gherkin-style)
```gherkin
Feature: Registro de Lançamentos Financeiros

  Scenario: Registrar lançamento de crédito
    Given que sou um comerciante
    When eu envio um lançamento de crédito de R$ 100,00
    Then o lançamento deve ser salvo no banco
    And deve gerar um ID único
    And deve publicar evento "LancamentoCriado"
    And deve retornar status 201 Created

  Scenario: Registrar lançamento com idempotency key
    Given que já enviei um lançamento com idempotency key "abc123"
    When eu envio o mesmo lançamento com a mesma key
    Then deve retornar o resultado cacheado
    And não deve criar duplicata
```

#### 2.3. Especificação Técnica
```
Endpoint: POST /api/lancamentos
Input:
  - valor: decimal (> 0)
  - tipo: string ("debito" ou "credito")
  - descricao: string (opcional, max 500 chars)
  - idempotency-key: string (opcional, header)

Output:
  - id: GUID
  - valor: decimal
  - tipo: string
  - descricao: string
  - dataHora: datetime
  - createdAt: datetime

Non-functional:
  - Latência: < 100ms (P95)
  - Disponibilidade: 99.5%
  - Idempotente: sim
```

---

### PASSO 3: IMPLEMENTAÇÃO BASEADA NA ESPECIFICAÇÃO

#### 3.1. Controller (API Layer)
**Arquivo:** `src/LedgerService/Controllers/LancamentosController.cs`

```csharp
[HttpPost]
public async Task<ActionResult<LancamentoResponse>> CreateLancamento(
    [FromBody] CreateLancamentoRequest request,
    [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
{
    // Especificação: Validar input
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // Especificação: Processar com idempotency
    var result = await _lancamentoService.CreateLancamentoAsync(request, idempotencyKey);
    
    // Especificação: Retornar 201 Created
    return CreatedAtAction(nameof(GetLancamentoById), new { id = result.Id }, result);
}
```

**Rastreabilidade:**
- Validar input → `ModelState.IsValid`
- Idempotency → `idempotencyKey` parameter
- Retornar 201 → `CreatedAtAction`

---

#### 3.2. Service Layer (Lógica de Negócio)
**Arquivo:** `src/LedgerService/Services/LancamentoService.cs`

```csharp
public async Task<LancamentoResponse> CreateLancamentoAsync(
    CreateLancamentoRequest request, 
    string? idempotencyKey = null)
{
    // Especificação: Verificar idempotency
    if (!string.IsNullOrEmpty(idempotencyKey))
    {
        var existingKey = await _idempotencyRepository.GetByKeyAsync(idempotencyKey);
        if (existingKey != null)
            return existingKey.GetResponse<LancamentoResponse>();
    }

    // Especificação: Criar lançamento
    var lancamento = new Lancamento
    {
        Id = Guid.NewGuid(),
        Valor = request.Valor,
        Tipo = request.Tipo,
        Descricao = request.Descricao,
        DataHora = DateTime.UtcNow
    };

    var created = await _lancamentoRepository.CreateAsync(lancamento);

    // Especificação: Publicar evento
    await _eventPublisher.PublishLancamentoCriadoAsync(
        created.Id, created.Valor, created.Tipo, created.DataHora);

    return MapToResponse(created);
}
```

**Rastreabilidade:**
- Verificar idempotency → `GetByKeyAsync`
- Criar lançamento → `new Lancamento()`
- Publicar evento → `PublishLancamentoCriadoAsync`

---

#### 3.3. Model (Contrato)
**Arquivo:** `src/LedgerService/Models/CreateLancamentoRequest.cs`

```csharp
public class CreateLancamentoRequest
{
    // Especificação: valor > 0
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Valor { get; set; }

    // Especificação: tipo = "debito" ou "credito"
    [Required]
    [RegularExpression("^(debito|credito)$")]
    public string Tipo { get; set; }

    // Especificação: descricao opcional, max 500
    [MaxLength(500)]
    public string? Descricao { get; set; }
}
```

**Rastreabilidade:**
- Valor > 0 → `[Range(0.01, double.MaxValue)]`
- Tipo válido → `[RegularExpression("^(debito|credito)$")]`
- Max 500 chars → `[MaxLength(500)]`

---

### PASSO 4: TESTES VALIDAM A ESPECIFICAÇÃO

#### 4.1. Teste Unitário (xUnit)
**Arquivo:** `src/LedgerService.Tests/Services/LancamentoServiceTests.cs`

```csharp
[Fact]
public async Task CreateLancamentoAsync_ShouldCreateLancamento()
{
    // Arrange (Dado)
    var request = new CreateLancamentoRequest
    {
        Valor = 100.50m,
        Tipo = "credito"
    };

    // Especificação: Deve criar com valor positivo
    Assert.True(request.Valor > 0);
    
    // Especificação: Deve criar com tipo válido
    Assert.True(request.Tipo == "credito" || request.Tipo == "debito");

    // Act (Quando)
    var result = await _service.CreateLancamentoAsync(request);

    // Assert (Então)
    // Especificação: Deve retornar resultado
    Assert.NotNull(result);
    
    // Especificação: Deve ter ID único
    Assert.NotEqual(Guid.Empty, result.Id);
    
    // Especificação: Deve publicar evento
    _mockPublisher.Verify(p => p.PublishLancamentoCriadoAsync(
        It.IsAny<Guid>(), 
        It.IsAny<decimal>(), 
        It.IsAny<string>(), 
        It.IsAny<DateTime>()), Times.Once);
}
```

**Rastreabilidade:**
- Valor positivo → `Assert.True(request.Valor > 0)`
- Tipo válido → `Assert.True(...)`
- ID único → `Assert.NotEqual(Guid.Empty, result.Id)`
- Publicar evento → `Verify(...)`

---

#### 4.2. Teste de Aceite (Gherkin-style - se usasse Cucumber)

```gherkin
Feature: Registro de Lançamentos

  Scenario: Registrar lançamento de crédito
    Given que a API está rodando
    When eu envio POST /api/lancamentos com:
      | valor | tipo    |
      | 100   | credito |
    Then o status deve ser 201
    And deve retornar um ID
    And o lançamento deve estar no banco
```

---

## COMO SERIA COM FERRAMENTA DE SPEC-DRIVEN (Exemplo com Cucumber)

### Exemplo de Feature File (Cucumber)
**Arquivo:** `Features/Lancamentos.feature`

```gherkin
Feature: Registro de Lançamentos Financeiros

  Scenario: Registrar lançamento de crédito com sucesso
    Given o Ledger Service está rodando
    When eu faço um POST para "/api/lancamentos" com:
      | valor | tipo    | descricao         |
      | 100   | credito | Venda de produtos |
    Then o status deve ser 201
    And o response deve conter:
      | campo     | tipo    |
      | valor     | decimal |
      | tipo      | string  |
      | id        | guid    |
    And deve existir no banco com valor 100
    And deve ter publicado evento "LancamentoCriado"

  Scenario: Tentar registrar lançamento com valor negativo
    When eu faço um POST para "/api/lancamentos" com:
      | valor | tipo    |
      | -50   | credito |
    Then o status deve ser 400
    And a mensagem deve conter "Valor deve ser maior que zero"
```

### Exemplo de Step Definitions (C#)
**Arquivo:** `Steps/LancamentoSteps.cs`

```csharp
[Binding]
public class LancamentoSteps
{
    private HttpClient _client;
    private HttpResponseMessage _response;

    [Given(@"o Ledger Service está rodando")]
    public void GivenOLedgerServiceEstaRodando()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };
    }

    [When(@"eu faço um POST para ""(.*)"" com:")]
    public async Task WhenEuFacoUmPostParaCom(string url, Table table)
    {
        var request = new CreateLancamentoRequest
        {
            Valor = decimal.Parse(table.Rows[0]["valor"]),
            Tipo = table.Rows[0]["tipo"],
            Descricao = table.Rows[0]["descricao"]
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        _response = await _client.PostAsync(url, content);
    }

    [Then(@"o status deve ser (\d+)")]
    public void ThenOStatusDeveSer(int status)
    {
        Assert.Equal(status, (int)_response.StatusCode);
    }

    [Then(@"o response deve conter:")]
    public void ThenOResponseDeveConter(Table table)
    {
        var content = await _response.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<LancamentoResponse>(content);
        
        Assert.NotNull(response);
        // Validar campos...
    }
}
```

---

## COMO FOI NA PRÁTICA (Sem Ferramenta Específica)

### Especificação em Documento
**Arquivo:** `passoapasso/requisitos_detalhados.md`

```markdown
### RF-001: Registrar Lançamento Financeiro

**Descrição:** O sistema deve permitir o registro de lançamentos financeiros

**Critérios de Aceite:**
- [x] Deve aceitar lançamentos do tipo "débito" e "crédito"
- [x] Deve registrar valor numérico positivo
- [x] Deve gerar identificador único
- [x] Deve publicar evento "LancamentoCriado"
```

### Implementação Segue Especificação
- Cada checkbox do critério de aceite → Código correspondente
- Validação de input → Data annotations
- ID único → `Guid.NewGuid()`
- Publicar evento → `RabbitMQEventPublisher`

### Testes Validam Especificação
- Cada teste valida um critério de aceite
- Se todos passam, especificação foi atendida

---

## DIFERENÇA: Com vs Sem Ferramenta

### COM FERRAMENTA (Cucumber/SpecFlow):
- ✅ Especificação em Gherkin (legível por não-técnicos)
- ✅ Testes automatizados lêem Gherkin
- ✅ Documentação viva (especificação = testes)
- ❌ Curva de aprendizado da ferramenta
- ❌ Overhead de manutenção

### SEM FERRAMENTA (Abordagem que usei):
- ✅ Especificação em markdown (mais flexível)
- ✅ Testes em xUnit (mais simples)
- ✅ Documentação separada (mais detalhada)
- ✅ Sem overhead de ferramenta
- ❌ Não é "documentação viva" (testes não leem especificação)

---

## COMO EXPlicAR NA ENTREVISTA

### Pergunta: "Como implementou usando Spec-Driven Development?"

### Resposta:

"Segui o princípio de Spec-Driven Development em 4 passos:

**1. Especificação Primeiro:**
Comecei refinando os requisitos do desafio em especificações detalhadas com critérios de aceite claros. Por exemplo, para 'Registrar Lançamento', defini que deve aceitar débito/crédito, valor positivo, gerar ID único, e publicar evento.

**2. Visualização da Especificação:**
Criei diagramas para visualizar a especificação - diagrama de contexto, fluxo de dados, componentes. Isso ajuda a entender melhor antes de codificar.

**3. Implementação Guiada:**
Implementei cada feature baseada na especificação. Por exemplo, o critério 'valor positivo' virou `[Range(0.01, double.MaxValue)]` no modelo. O critério 'publicar evento' virou chamada ao RabbitMQPublisher.

**4. Validação via Testes:**
Criei testes que validam cada critério de aceite. Se todos os testes passam, a especificação foi atendida.

Não usei uma ferramenta específica como Cucumber, mas segui a filosofia: especificação clara → implementação guiada → testes validando. O resultado é o mesmo: código que atende exatamente o que foi especificado."

---

## RASTREABILIDADE COMPLETA (Exemplo Prático)

### Especificação → Código → Teste

| Especificação | Código | Teste |
|--------------|--------|-------|
| "Aceitar débito/crédito" | `[RegularExpression("^(debito\|credito)$")]` | `Test_ValidTipo` |
| "Valor positivo" | `[Range(0.01, double.MaxValue)]` | `Test_ValorPositivo` |
| "Gerar ID único" | `Guid.NewGuid()` | `Test_IdUnico` |
| "Publicar evento" | `PublishLancamentoCriadoAsync()` | `Verify(Publish, Times.Once)` |
| "Idempotência" | `GetByKeyAsync()` | `Test_Idempotency` |

---

## CONCLUSÃO

A diferença entre usar ou não usar ferramenta de Spec-Driven Development é **a ferramenta, não a abordagem**.

Minha abordagem:
- Especificação clara em documento
- Implementação guiada pela especificação
- Testes validando a especificação
- Rastreabilidade explícita (especificação → código → teste)

Isso atende o princípio de Spec-Driven Development mesmo sem usar Cucumber ou SpecFlow.

---

**O importante é: especificação antes de código, implementação guiada pela especificação, e testes que validam a especificação.** ✅
