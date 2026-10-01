# Diagramas C4 - Projeto Desafio Carrefour

## C4-1: Context Diagram

### Descrição:
Mostra o sistema e seu contexto (atores externos, sistemas externos)

```mermaid
graph TB
    subgraph "Context"
        Comerciante[Comerciante]
        SistemaFluxoCaixa["Sistema de Fluxo de Caixa Diário"]
        SistemaLegado["Sistema Legado (Para Migração)"]
        SistemaPagamentos["Sistema de Pagamentos (Integração Futura)"]
    end

    Comerciante -->|Registra lançamentos| SistemaFluxoCaixa
    Comerciante -->|Consulta consolidado| SistemaFluxoCaixa
    SistemaFluxoCaixa -->|Migra dados históricos| SistemaLegado
    SistemaFluxoCaixa -->|Integra pagamentos| SistemaPagamentos

    style SistemaFluxoCaixa fill:#e1f5ff,stroke:#01579b,stroke-width:3px
    style Comerciante fill:#fff4e1,stroke:#ff6f00,stroke-width:2px
    style SistemaLegado fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    style SistemaPagamentos fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
```

### Legenda:
- **Comerciante:** Usuário principal que usa o sistema
- **Sistema de Fluxo de Caixa Diário:** Sistema que você desenvolveu
- **Sistema Legado:** Sistema antigo (para migração futura)
- **Sistema de Pagamentos:** Sistema externo (integração futura)

---

## C4-2: Container Diagram

### Descrição:
Mostra os containers (aplicações, bancos, filas) e suas relações

```mermaid
graph TB
    subgraph "Sistema de Fluxo de Caixa Diário"
        subgraph "Cliente"
            ClienteWeb["Cliente Web<br/>(Swagger UI)"]
        end

        subgraph "Aplicações"
            LedgerService["Ledger Service<br/>(API de Lançamentos)"]
            ConsolidationService["Consolidation Service<br/>(API de Consolidado)"]
        end

        subgraph "Mensageria"
            RabbitMQ["RabbitMQ<br/>(Message Broker)"]
        end

        subgraph "Cache"
            Redis["Redis<br/>(Cache)"]
        end

        subgraph "Bancos de Dados"
            LedgerDB["PostgreSQL<br/>(ledgerdb)"]
            ConsolidationDB["PostgreSQL<br/>(consolidationdb)"]
        end
    end

    ClienteWeb -->|HTTP: localhost:5001| LedgerService
    ClienteWeb -->|HTTP: localhost:5002| ConsolidationService
    LedgerService -->|Publish| RabbitMQ
    RabbitMQ -->|Consume| ConsolidationService
    LedgerService -->|SQL| LedgerDB
    ConsolidationService -->|SQL| ConsolidationDB
    ConsolidationService -->|Cache| Redis

    style LedgerService fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    style ConsolidationService fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    style RabbitMQ fill:#fff3e0,stroke:#e65100,stroke-width:2px
    style Redis fill:#fff3e0,stroke:#e65100,stroke-width:2px
    style LedgerDB fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    style ConsolidationDB fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
```

### Legenda:
- **Cliente Web:** Swagger UI para testar as APIs
- **Ledger Service:** API que recebe lançamentos
- **Consolidation Service:** API que processa consolidado
- **RabbitMQ:** Message broker para comunicação assíncrona
- **Redis:** Cache para performance
- **PostgreSQL (ledgerdb):** Banco de dados de lançamentos
- **PostgreSQL (consolidationdb):** Banco de dados de consolidados

---

## C4-3: Component Diagram - Ledger Service

### Descrição:
Mostra os componentes dentro do Ledger Service

```mermaid
graph TB
    subgraph "Ledger Service (Container)"
        subgraph "API Layer"
            LancamentosController["LancamentosController<br/>(REST API)"]
        end

        subgraph "Service Layer"
            LancamentoService["LancamentoService<br/>(Business Logic)"]
        end

        subgraph "Data Layer"
            LancamentoRepository["LancamentoRepository<br/>(Data Access)"]
            IdempotencyRepository["IdempotencyRepository<br/>(Idempotency Keys)"]
        end

        subgraph "Messaging Layer"
            RabbitMQEventPublisher["RabbitMQEventPublisher<br/>(Event Publisher)"]
        end

        subgraph "External"
            LedgerDB["PostgreSQL<br/>(ledgerdb)"]
            RabbitMQ["RabbitMQ<br/>(lancamentos queue)"]
        end
    end

    LancamentosController -->|usa| LancamentoService
    LancamentoService -->|usa| LancamentoRepository
    LancamentoService -->|usa| IdempotencyRepository
    LancamentoService -->|usa| RabbitMQEventPublisher
    LancamentoRepository -->|SQL| LedgerDB
    RabbitMQEventPublisher -->|Publish| RabbitMQ

    style LancamentosController fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    style LancamentoService fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    style LancamentoRepository fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    style IdempotencyRepository fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    style RabbitMQEventPublisher fill:#fff3e0,stroke:#e65100,stroke-width:2px
```

### Legenda:
- **LancamentosController:** Controller REST API
- **LancamentoService:** Lógica de negócio
- **LancamentoRepository:** Acesso a dados de lançamentos
- **IdempotencyRepository:** Acesso a dados de idempotência
- **RabbitMQEventPublisher:** Publicador de eventos no RabbitMQ

---

## C4-3: Component Diagram - Consolidation Service

### Descrição:
Mostra os componentes dentro do Consolidation Service

```mermaid
graph TB
    subgraph "Consolidation Service (Container)"
        subgraph "API Layer"
            ConsolidadoController["ConsolidadoController<br/>(REST API)"]
        end

        subgraph "Service Layer"
            ConsolidationService["ConsolidationService<br/>(Business Logic)"]
            RedisCacheService["RedisCacheService<br/>(Cache Logic)"]
        end

        subgraph "Data Layer"
            ConsolidadoRepository["ConsolidadoRepository<br/>(Data Access)"]
        end

        subgraph "Messaging Layer"
            RabbitMQEventConsumer["RabbitMQEventConsumer<br/>(Event Consumer)"]
        end

        subgraph "External"
            ConsolidationDB["PostgreSQL<br/>(consolidationdb)"]
            RabbitMQ["RabbitMQ<br/>(lancamentos queue)"]
            Redis["Redis<br/>(cache)"]
        end
    end

    ConsolidadoController -->|usa| ConsolidationService
    ConsolidationService -->|usa| RedisCacheService
    ConsolidationService -->|usa| ConsolidadoRepository
    RabbitMQEventConsumer -->|Consume| RabbitMQ
    RabbitMQEventConsumer -->|processa| ConsolidationService
    RedisCacheService -->|Cache| Redis
    ConsolidadoRepository -->|SQL| ConsolidationDB

    style ConsolidadoController fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    style ConsolidationService fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    style RedisCacheService fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    style ConsolidadoRepository fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    style RabbitMQEventConsumer fill:#fff3e0,stroke:#e65100,stroke-width:2px
```

### Legenda:
- **ConsolidadoController:** Controller REST API
- **ConsolidationService:** Lógica de negócio de consolidação
- **RedisCacheService:** Lógica de cache
- **ConsolidadoRepository:** Acesso a dados de consolidados
- **RabbitMQEventConsumer:** Consumidor de eventos do RabbitMQ

---

## 🎯 Como usar esses diagramas:

### 1. Adicionar ao projeto:
- Criar arquivo: `c:\desafio\diagrams\c4-context.mmd`
- Criar arquivo: `c:\desafio\diagrams\c4-container.mmd`
- Criar arquivo: `c:\desafio\diagrams\c4-component-ledger.mmd`
- Criar arquivo: `c:\desafio\diagrams\c4-component-consolidation.mmd`

### 2. Adicionar ao README:
```markdown
## Diagramas C4

### Context Diagram
Arquivo: diagrams/c4-context.mmd
Mostra o sistema e seu contexto (comerciante, sistema legado, sistema de pagamentos)

### Container Diagram
Arquivo: diagrams/c4-container.mmd
Mostra os containers (Ledger Service, Consolidation Service, RabbitMQ, PostgreSQL, Redis)

### Component Diagram - Ledger Service
Arquivo: diagrams/c4-component-ledger.mmd
Mostra os componentes dentro do Ledger Service (Controller, Service, Repository, Event Publisher)

### Component Diagram - Consolidation Service
Arquivo: diagrams/c4-component-consolidation.mmd
Mostra os componentes dentro do Consolidation Service (Controller, Service, Repository, Event Consumer)
```

### 3. Para a entrevista:
"Usei o modelo C4 para documentar a arquitetura. Criei:
- Context Diagram: mostra o sistema e seu contexto (comerciante, sistema legado)
- Container Diagram: mostra os containers (Ledger Service, Consolidation Service, RabbitMQ, PostgreSQL, Redis)
- Component Diagram: mostra os componentes dentro de cada container (controllers, services, repositories)
- Isso permite comunicação clara entre stakeholders e desenvolvedores."

---

## 💡 Diferença entre seus diagramas atuais e C4:

### Seus diagramas atuais:
- Focam em fluxo de dados
- São úteis para entender processo
- Mas não seguem hierarquia C4

### Diagramas C4:
- Hierarquia explícita: Context → Container → Component
- Linguagem padrão da indústria
- Mais estruturado e formal
- Focam em estrutura, não apenas fluxo

---

**Quer que eu crie esses arquivos .mmd no seu projeto?**
