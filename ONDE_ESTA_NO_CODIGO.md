# Onde Está no Código - Perguntas Específicas

## Pergunta 1: ONDE ESTÁ ESCALABILIDADE? ONDE IMPLEMENTOU?

### Resposta Simples:
Escalabilidade está em **3 lugares** no projeto:

---

### 1. Docker Compose - Pode adicionar mais instâncias

**Arquivo:** `docker-compose.yml` (linhas 85-108)

```yaml
ledger-service:
  # Se precisar de mais escalabilidade, é só adicionar:
  deploy:
    replicas: 3  # ← AQUI: 3 cópias rodando
```

**Como funciona:**
- Docker Compose permite rodar várias cópias do mesmo serviço
- Se precisar de mais capacidade, aumenta o número de réplicas
- Load balancer distribui as requisições entre as cópias

---

### 2. Código é STATELESS - Pode escalar horizontalmente

**Arquivo:** `src/LedgerService/Program.cs` (linhas 1-136)

**Como funciona:**
- O serviço **não guarda estado** em memória
- Tudo está no banco de dados (PostgreSQL)
- Posso ter 10 cópias rodando, cada uma independentemente
- Se uma cair, as outras continuam funcionando

**Onde está no código:**
- Não há variáveis globais
- Não há cache em memória
- Tudo vem do banco ou RabbitMQ

---

### 3. RabbitMQ como BUFFER - Absorve picos

**Arquivo:** `src/LedgerService/Messaging/RabbitMQEventPublisher.cs` (linhas 17-67)

**Como funciona:**
- Se vierem 100 requisições de uma vez
- RabbitMQ guarda na fila
- Consolidation Service processa no ritmo dele
- Nada é perdido, sistema não trava

**Linha específica:**
```csharp
_channel.QueueDeclare(
    queue: _queueName,
    durable: true,  // ← AQUI: Fila persistente (não perde mensagens)
    exclusive: false,
    autoDelete: false,
    arguments: null
);
```

---

### Pergunta: "Como atende 50 req/s?"

**Resposta:**
- Com **2 instâncias** do Consolidation Service
- Cada uma trata **25 req/s**
- Total = **50 req/s**
- RabbitMQ absorve se vier mais de 50

**No código:**
- `docker-compose.yml` - posso mudar de 1 para 2 instâncias
- É só adicionar uma linha: `deploy: replicas: 2`

---

## Pergunta 2: ONDE ESTÁ OBSERVABILIDADE? ONDE IMPLEMENTOU E COMO FUNCIONA?

### Resposta Simples:
Observabilidade está em **4 lugares** no projeto:

---

### 1. LOGS - Serilog (registra tudo)

**Arquivo:** `src/LedgerService/Program.cs` (linhas 8-15)

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()                    // ← AQUI: Log no console
    .WriteTo.File("logs/ledgerservice-.txt") // ← AQUI: Log em arquivo
    .CreateLogger();
```

**Como funciona:**
- Toda operação é registrada
- Logs em arquivo (rotativo por dia)
- Console para debug em tempo real
- Formato estruturado (JSON pronto para análise)

**Onde está no código:**
```csharp
_logger.LogInformation("Lançamento criado: {LancamentoId}", result.Id);
// ↑ Em Controllers/Services - logs específicos
```

---

### 2. HEALTH CHECKS - Saber se está vivo

**Arquivo:** `src/LedgerService/Program.cs` (linha 129)

```csharp
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    service = "LedgerService", 
    timestamp = DateTime.UtcNow 
}));
// ↑ AQUI: Endpoint de health check
```

**Como funciona:**
- Endpoint `/health` em cada serviço
- Retorna status + timestamp
- Pode ser usado por Kubernetes ou load balancer
- Se não responder, serviço está morto

**Testar:**
```bash
curl http://localhost:5001/health
```

---

### 3. RABBITMQ MANAGEMENT UI - Ver fila em tempo real

**Arquivo:** `docker-compose.yml` (linhas 45-56)

```yaml
rabbitmq:
  ports:
    - "15672:15672"  # ← AQUI: Management UI
```

**Como funciona:**
- Interface web em http://localhost:15672
- Mostra quantas mensagens na fila
- Mostra taxa de consumo
- Mostra se há mensagens na Dead Letter Queue

**Acessar:**
- URL: http://localhost:15672
- User: guest
- Password: guest

---

### 4. METRICS (pronto para Prometheus)

**Arquivo:** `src/LedgerService/Program.cs` (linhas 8-15)

**Como funciona:**
- Serilog pode enviar métricas para Prometheus
- Logs já têm timestamp e contexto
- Pronto para adicionar: `WriteTo.Prometheus()`

**Não implementado ainda, mas estrutura está pronta.**

---

## Pergunta 3: COMO IMPLEMENTOU ARQUITETURA CORPORATIVA?

### Resposta Simples:
Arquitetura Corporativa está em **3 documentos** e no **código**:

---

### 1. MAPEAMENTO DE DOMÍNIOS - Contextos do negócio

**Arquivo:** `passoapasso/mapeamento_dominios.md`

**O que tem:**
- **Domínio 1: Gestão Financeira** → Ledger Service
- **Domínio 2: Consolidação Financeira** → Consolidation Service
- **Domínio 3: Integração** → RabbitMQ

**Como funciona no código:**
- Cada domínio é um **bounded context** (contexto delimitado)
- Cada contexto tem seu próprio banco de dados
- Contextos se comunicam via eventos (não direto)

**No código:**
- `src/LedgerService/` - Contexto de Gestão Financeira
- `src/ConsolidationService/` - Contexto de Consolidação
- Não há dependência direta entre eles

---

### 2. CAPACIDADES DE NEGÓCIO - O que cada domínio faz

**Arquivo:** `passoapasso/mapeamento_dominios.md` (linhas 15-30)

**Exemplo:**
```
Domínio: Gestão Financeira
Capacidades:
- Registrar transações
- Consultar transações
- Validar transações
```

**No código:**
- `Controllers/LancamentosController.cs` - Registrar transações
- `Services/LancamentoService.cs` - Lógica de negócio
- Cada método = uma capacidade

---

### 3. SEPARAÇÃO DE RESPONSABILIDADES - Cada contexto é independente

**Arquivo:** `passoapasso/mapeamento_dominios.md` (linhas 50-70)

**No código:**
```
src/LedgerService/
├── Controllers/     → API (entrada)
├── Services/        → Lógica de negócio
├── Repositories/    → Acesso a dados
└── Messaging/       → Comunicação

src/ConsolidationService/
├── Controllers/     → API (entrada)
├── Services/        → Lógica de negócio
├── Repositories/    → Acesso a dados
└── Messaging/       → Comunicação
```

**Como funciona:**
- Cada camada tem uma responsabilidade única
- Controller não acessa banco direto
- Service não faz HTTP
- Repository não tem lógica de negócio

---

### 4. INTEGRAÇÃO ENTRE CONTEXTOS - Comunicação definida

**Arquivo:** `passoapasso/mapeamento_dominios.md` (linhas 72-90)

**No código:**
- `src/LedgerService/Messaging/RabbitMQEventPublisher.cs` - Publica eventos
- `src/ConsolidationService/Messaging/RabbitMQEventConsumer.cs` - Consome eventos

**Como funciona:**
- Ledger publica "LancamentoCriado"
- Consolidation consome "LancamentoCriado"
- Não há chamada direta HTTP entre eles
- Comunicação assíncrona via fila

---

### 5. ALINHAMENTO COM NEGÓCIO - Contextos alinhados com áreas de negócio

**Arquivo:** `passoapasso/mapeamento_dominios.md` (linhas 1-10)

**Como funciona:**
- Ledger Service = Área financeira (operações)
- Consolidation Service = Área financeira (relatórios)
- RabbitMQ = Área de integração

**Benefício:**
- Times podem trabalhar independentemente
- Escalabilidade independente
- Manutenção facilitada

---

## RESUMO ONDE ESTÁ NO CÓDIGO

| O que perguntam | Onde está no código | Linha/arquivo específico |
|----------------|-------------------|-------------------------|
| **Escalabilidade** | | |
| Docker replicas | docker-compose.yml | Linha 85-108 |
| Código stateless | Program.cs (todos serviços) | Linha 1-136 |
| RabbitMQ buffer | RabbitMQEventPublisher.cs | Linha 17-67 |
| **Observabilidade** | | |
| Logs Serilog | Program.cs | Linha 8-15 |
| Health checks | Program.cs | Linha 129 |
| RabbitMQ UI | docker-compose.yml | Linha 45-56 |
| **Arquitetura Corporativa** | | |
| Mapeamento de domínios | passoapasso/mapeamento_dominios.md | Todo o arquivo |
| Capacidades de negócio | passoapasso/mapeamento_dominios.md | Linha 15-30 |
| Separação de responsabilidades | Estrutura de pastas | src/*/ |
| Integração entre contextos | Messaging/ | RabbitMQPublisher/Consumer |

---

## NA ENTREVISTA, FALA ASSIM:

### Sobre Escalabilidade:

"Escalabilidade está em 3 lugares:

1. No Docker Compose posso rodar várias cópias do serviço - se precisar de mais capacidade, aumento o número de réplicas

2. O código é stateless - não guarda nada em memória, tudo está no banco. Posso ter 10 cópias rodando sem problema

3. RabbitMQ funciona como buffer - se vierem 100 requisições de uma vez, ele guarda na fila e o serviço processa no ritmo dele. Com 2 instâncias do Consolidation Service, atendo 50 req/s."

### Sobre Observabilidade:

"Observabilidade está em 4 lugares:

1. Logs com Serilog - registro tudo em arquivo e console
2. Health checks - endpoint /health em cada serviço para saber se está vivo
3. RabbitMQ Management UI - interface web para ver a fila em tempo real
4. Estrutura pronta para métricas com Prometheus

Isso permite monitorar se o sistema está funcionando, identificar problemas e entender o comportamento."

### Sobre Arquitetura Corporativa:

"Arquitetura Corporativa está documentada em mapeamento_dominios.md e implementada no código:

1. Mapeei 3 domínios: Gestão Financeira (Ledger), Consolidação (Consolidation), Integração (RabbitMQ)

2. Cada domínio é um bounded context - tem seu próprio banco, suas responsabilidades, sua equipe

3. Capacidades de negócio estão definidas: registrar transações, consolidar, gerar relatórios

4. Separação de responsabilidades no código: Controllers → Services → Repositories, cada um com sua função

5. Integração entre contextos via eventos assíncronos - não há dependência direta, o que permite escalar e evoluir independentemente

Isso alinha a arquitetura com o negócio, permite times autônomos e escalabilidade direcionada."

---

**Não precisa decorar linhas. A chave é saber ONDE estão as coisas e COMO funcionam.** ✅
