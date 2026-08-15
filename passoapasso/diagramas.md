# Diagramas da Arquitetura

Este documento contém os diagramas da arquitetura da solução, usando Mermaid para visualização.

---

## Diagrama 1: Contexto da Arquitetura

```mermaid
graph TB
    subgraph "External Systems"
        Client[Cliente API]
    end
    
    subgraph "API Gateway"
        Gateway[API Gateway]
    end
    
    subgraph "Financial Ledger Service"
        API1[REST API]
        DB1[(PostgreSQL)]
        Publisher[Event Publisher]
    end
    
    subgraph "Message Broker"
        Queue[RabbitMQ<br/>Message Queue]
        DLQ[Dead Letter Queue]
    end
    
    subgraph "Daily Consolidation Service"
        Consumer[Event Consumer]
        Worker[Consolidation Worker]
        API2[REST API]
        DB2[(PostgreSQL)]
        Cache[(Redis Cache)]
    end
    
    subgraph "Monitoring"
        Metrics[Prometheus]
        Dash[Grafana Dashboard]
    end
    
    Client -->|HTTPS| Gateway
    Gateway -->|Route| API1
    Gateway -->|Route| API2
    
    API1 -->|Persist| DB1
    API1 -->|Publish Event| Queue
    Queue -->|Consume| Consumer
    Queue -.->|Failed Messages| DLQ
    
    Consumer --> Worker
    Worker -->|Process| DB2
    Worker -->|Update| Cache
    
    API2 -->|Read| Cache
    API2 -.->|Cache Miss| DB2
    
    API1 -.->|Metrics| Metrics
    API2 -.->|Metrics| Metrics
    Queue -.->|Metrics| Metrics
    Metrics --> Dash
    
    style Client fill:#e1f5ff
    style Gateway fill:#fff4e1
    style API1 fill:#e8f5e9
    style API2 fill:#e8f5e9
    style Queue fill:#f3e5f5
    style Cache fill:#fff9c4
    style DB1 fill:#fce4ec
    style DB2 fill:#fce4ec
```

---

## Diagrama 2: Fluxo de Lançamento

```mermaid
sequenceDiagram
    participant C as Cliente
    participant GW as API Gateway
    participant LS as Ledger Service
    participant DB as PostgreSQL
    participant MQ as RabbitMQ
    participant CS as Consolidation Service
    participant R as Redis
    
    C->>GW: POST /lancamentos (com idempotency-key)
    GW->>LS: POST /lancamentos
    LS->>LS: Verifica idempotency-key
    alt Key já existe
        LS-->>GW: 200 OK (resultado cacheado)
        GW-->>C: 200 OK
    else Key não existe
        LS->>DB: INSERT lancamento
        DB-->>LS: Sucesso
        LS->>MQ: Publish "LancamentoCriado"
        MQ-->>LS: Ack
        LS->>LS: Cacheia resultado
        LS-->>GW: 201 Created
        GW-->>C: 201 Created
    end
    
    Note over MQ,CS: Processamento Assíncrono
    
    MQ->>CS: Consume "LancamentoCriado"
    CS->>CS: Processa consolidação
    CS->>DB: Atualiza consolidado
    DB-->>CS: Sucesso
    CS->>R: Invalida cache do dia
    CS->>MQ: Ack
```

---

## Diagrama 3: Fluxo de Consulta de Consolidado

```mermaid
sequenceDiagram
    participant C as Cliente
    participant GW as API Gateway
    participant CS as Consolidation Service
    participant R as Redis
    participant DB as PostgreSQL
    
    C->>GW: GET /consolidado/{data}
    GW->>CS: GET /consolidado/{data}
    CS->>R: GET cache:{data}
    
    alt Cache Hit
        R-->>CS: Dados do consolidado
        CS-->>GW: 200 OK
        GW-->>C: 200 OK
    else Cache Miss
        CS->>DB: SELECT consolidado WHERE data = ?
        DB-->>CS: Dados do consolidado
        CS->>R: SET cache:{data} (TTL 5min)
        CS-->>GW: 200 OK
        GW-->>C: 200 OK
    end
```

---

## Diagrama 4: Arquitetura de Componentes

```mermaid
graph LR
    subgraph "Ledger Service"
        subgraph "API Layer"
            Controller[LancamentoController]
            Validator[RequestValidator]
        end
        
        subgraph "Service Layer"
            LedgerService[LedgerService]
            IdempotencyService[IdempotencyService]
        end
        
        subgraph "Infrastructure Layer"
            Repository[LancamentoRepository]
            EventPublisher[RabbitMQPublisher]
        end
        
        Controller --> Validator
        Controller --> LedgerService
        LedgerService --> IdempotencyService
        LedgerService --> Repository
        LedgerService --> EventPublisher
    end
    
    subgraph "Consolidation Service"
        subgraph "API Layer"
            ConsolidadoController[ConsolidadoController]
        end
        
        subgraph "Service Layer"
            ConsolidadoService[ConsolidadoService]
            CacheService[CacheService]
        end
        
        subgraph "Worker Layer"
            EventConsumer[RabbitMQConsumer]
            ConsolidationWorker[ConsolidationWorker]
        end
        
        subgraph "Infrastructure Layer"
            ConsolidadoRepository[ConsolidadoRepository]
        end
        
        ConsolidadoController --> ConsolidadoService
        ConsolidadoService --> CacheService
        ConsolidadoService --> ConsolidadoRepository
        EventConsumer --> ConsolidationWorker
        ConsolidationWorker --> ConsolidadoService
    end
```

---

## Diagrama 5: Modelo de Dados

```mermaid
erDiagram
    LANCAMENTOS ||--o{ LANCAMENTOS : "registra"
    
    LANCAMENTOS {
        uuid id PK
        decimal valor "valor positivo"
        string tipo "debito|credito"
        datetime data_hora
        string descricao
        datetime created_at
        datetime updated_at
    }
    
    CONSOLIDADO_DIARIO {
        date data PK
        decimal total_creditos
        decimal total_debitos
        decimal saldo_final
        integer qtd_lancamentos
        datetime updated_at
    }
    
    IDEMPOTENCY_KEYS {
        string key PK
        uuid lancamento_id FK
        json response
        datetime created_at
        datetime expires_at
    }
```

---

## Diagrama 6: Arquitetura de Deploy

```mermaid
graph TB
    subgraph "Production Environment"
        subgraph "Load Balancer"
            LB[Load Balancer / API Gateway]
        end
        
        subgraph "Ledger Service Cluster"
            LS1[Ledger Service Pod 1]
            LS2[Ledger Service Pod 2]
            LS3[Ledger Service Pod 3]
        end
        
        subgraph "Consolidation Service Cluster"
            CS1[Consolidation Service Pod 1]
            CS2[Consolidation Service Pod 2]
        end
        
        subgraph "Data Layer"
            PG_primary[(PostgreSQL Primary)]
            PG_replica[(PostgreSQL Replica)]
            R_master[(Redis Master)]
            R_replica[(Redis Replica)]
        end
        
        subgraph "Message Layer"
            RMQ1[RabbitMQ Node 1]
            RMQ2[RabbitMQ Node 2]
            RMQ3[RabbitMQ Node 3]
        end
        
        subgraph "Monitoring"
            PROM[Prometheus]
            GRAF[Grafana]
        end
        
        LB --> LS1
        LB --> LS2
        LB --> LS3
        LB --> CS1
        LB --> CS2
        
        LS1 --> PG_primary
        LS2 --> PG_primary
        LS3 --> PG_primary
        CS1 --> PG_replica
        CS2 --> PG_replica
        
        LS1 --> RMQ1
        LS2 --> RMQ2
        LS3 --> RMQ3
        CS1 --> RMQ1
        CS2 --> RMQ2
        
        CS1 --> R_master
        CS2 --> R_master
        
        LS1 -.-> PROM
        LS2 -.-> PROM
        LS3 -.-> PROM
        CS1 -.-> PROM
        CS2 -.-> PROM
        RMQ1 -.-> PROM
        RMQ2 -.-> PROM
        RMQ3 -.-> PROM
        
        PROM --> GRAF
    end
```

---

## Diagrama 7: Escalabilidade e Performance

```mermaid
graph LR
    subgraph "Pico de Carga (50 req/s)"
        Load["50 req/s"]
    end
    
    subgraph "Camada 1: API Gateway"
        GW1[Rate Limiting]
        GW2[Load Balancing]
    end
    
    subgraph "Camada 2: Ledger Service"
        LS1[Scale out<br/>3 instâncias]
        LS2[~17 req/s cada]
    end
    
    subgraph "Camada 3: Message Queue"
        MQ1[Buffer<br/>RabbitMQ]
        MQ2[Spikes absorvidos]
    end
    
    subgraph "Camada 4: Consolidation Service"
        CS1[Scale out<br/>2 instâncias]
        CS2[~25 req/s cada]
    end
    
    subgraph "Camada 5: Cache"
        R1[Redis]
        R2[Cache hit<br/>~90%]
    end
    
    Load --> GW1
    GW1 --> GW2
    GW2 --> LS1
    LS1 --> LS2
    LS2 --> MQ1
    MQ1 --> MQ2
    MQ2 --> CS1
    CS1 --> CS2
    CS2 --> R1
    R1 --> R2
```

---

## Diagrama 8: Tratamento de Falhas e Resiliência

```mermaid
graph TB
    subgraph "Cenário: Consolidation Service Down"
        LS[Ledger Service]
        MQ[RabbitMQ]
        CS_down[Consolidation Service ❌]
        MQ_full[Queue Building Up]
        CS_up[Consolidation Service ✅]
        DLQ[Dead Letter Queue]
    end
    
    LS -->|Publish| MQ
    MQ -.->|Consume| CS_down
    MQ -->|Messages Queued| MQ_full
    
    Note over MQ_full: Messages persistem<br/>não são perdidos
    
    CS_up -->|Resume Consuming| MQ
    MQ -->|Process Backlog| CS_up
    
    MQ -.->|Max Retries| DLQ
    
    style CS_down fill:#ffcdd2
    style CS_up fill:#c8e6c9
```

---

## Como Visualizar

### Opção 1: VS Code com Mermaid Preview
1. Instale a extensão "Markdown Preview Mermaid Support"
2. Abra este arquivo no VS Code
3. Use o preview para visualizar os diagramas

### Opção 2: Mermaid Live Editor
1. Acesse https://mermaid.live
2. Copie o código de cada diagrama
3. Cole no editor para visualizar

### Opção 3: GitHub/GitLab
1. Faça commit deste arquivo
2. GitHub e GitLab renderizam diagramas Mermaid automaticamente

---

**Status:** Diagramas base - Podem ser refinados conforme implementação
