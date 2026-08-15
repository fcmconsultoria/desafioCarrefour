# Decisões Arquiteturais (ADR)

## Metodologia
Este documento segue o padrão **Architecture Decision Record (ADR)** para documentar decisões arquiteturais importantes. Cada decisão tem:
- **Contexto:** O problema que estamos resolvendo
- **Decisão:** O que foi decidido
- **Consequências:** Impactos positivos e negativos
- **Status:** Proposto, Aceito, Rejeitado, Superseded

---

## ADR-001: Arquitetura Baseada em Eventos (Event-Driven Architecture)

**Data:** 14/08/2026  
**Status:** Aceito

### Contexto
O requisito não-funcional RNF-001 estabelece que o serviço de lançamentos não pode ficar indisponível se o sistema de consolidado diário cair. Isso indica necessidade de desacoplamento entre os serviços.

### Decisão
Adotar arquitetura baseada em eventos usando um message broker para comunicação assíncrona entre serviços.

**Padrão:** Publisher-Subscriber com Message Queue

**Fluxo:**
1. Serviço de Lançamentos recebe POST
2. Persiste lançamento no banco
3. Publica evento "LancamentoCriado" na fila
4. Serviço de Consolidado consome evento
5. Processa e atualiza consolidado

### Consequências

**Positivos:**
- ✅ Desacoplamento total entre serviços
- ✅ Lançamentos funcionam mesmo se consolidado estiver down
- ✅ Capacidade de buffer para picos de carga (50 req/s)
- ✅ Retry automático via configuração da fila
- ✅ Dead Letter Queue para mensagens que falharam
- ✅ Escalabilidade independente de cada serviço

**Negativos:**
- ❌ Consistência eventual (consolidado pode ter delay)
- ❌ Complexidade adicional (message broker)
- ❌ Debugging mais complexo (mensagens assíncronas)
- ❌ Custo adicional de infraestrutura

**Mitigações:**
- Cache de consolidados para consultas frequentes
- Logs detalhados de mensagens
- Dashboard de monitoramento da fila

---

## ADR-002: Padrão de Arquitetura - Microsserviços

**Data:** 14/08/2026  
**Status:** Aceito

### Contexto
O desafio exige:
- Isolamento de serviços (lançamentos não pode depender de consolidado)
- Escalabilidade para 50 req/s
- Segregação de capacidades

### Decisão
Adotar arquitetura de microsserviços com dois serviços principais:
1. **Serviço de Lançamentos** (Financial Ledger Service)
2. **Serviço de Consolidação** (Daily Consolidation Service)

**Boundary:** Cada serviço com seu próprio banco de dados

### Consequências

**Positivos:**
- ✅ Isolamento de falhas (lançamentos não afetados por consolidado)
- ✅ Escalabilidade independente (escalar apenas o que precisa)
- ✅ Deploy independente (atualizar um serviço sem parar o outro)
- ✅ Escolha de tecnologia otimizada por serviço
- ✅ Alinhamento com bounded contexts identificados

**Negativos:**
- ❌ Complexidade operacional (dois serviços para gerenciar)
- ❌ Desafios de consistência distribuída
- ❌ Latência de rede entre serviços
- ❌ Maior esforço de monitoring

**Mitigações:**
- Contratos claros de APIs (OpenAPI/Swagger)
- Testes de contrato (Pact)
- Tracing distribuído
- Centralized logging

---

## ADR-003: Banco de Dados - PostgreSQL

**Data:** 14/08/2026  
**Status:** Aceito

### Contexto
Necessidade de persistir lançamentos financeiros com:
- Integridade de dados (ACID)
- Consultas complexas (filtros, agregações)
- Transações consistentes

### Decisão
Usar PostgreSQL como banco de dados relacional para ambos os serviços.

**Racional:**
- ACID compliance crítico para dados financeiros
- Suporte a JSONB para flexibilidade
- Excelente performance para consultas
- Open source e maduro
- Backup/restore robusto

### Consequências

**Positivos:**
- ✅ Integridade de dados garantida
- ✅ SQL poderoso para queries complexas
- ✅ Maturidade e estabilidade
- ✅ Comunidade ativa
- ✅ Suporte a transações

**Negativos:**
- ❌ Escalabilidade vertical (mais difícil horizontal)
- ❌ Schema menos flexível que NoSQL
- ❌ Custo em cloud comparado a alternativas

**Mitigações:**
- Read replicas para escalabilidade de leitura
- Connection pooling
- Índices otimizados

---

## ADR-004: Message Broker - RabbitMQ

**Data:** 14/08/2026  
**Status:** Aceito

### Contexto
Necessidade de comunicação assíncrona confiável com:
- Garantia de entrega
- Retry automático
- Dead Letter Queue
- Suporte a 50 req/s em picos

### Decisão
Usar RabbitMQ como message broker.

**Racional:**
- Maduro e estável
- Suporte nativo a Dead Letter Queue
- Configuração flexível de retry
- Protocolo AMQP padrão
- Management UI útil
- Open source

### Consequências

**Positivos:**
- ✅ Garantia de entrega (persistent queues)
- ✅ Retry configurável
- ✅ DLQ nativo
- ✅ Alta performance para 50 req/s
- ✅ Visualização via Management UI

**Negativos:**
- ❌ Mais um componente para gerenciar
- ❌ Single point of failure (requer cluster)
- ❌ Custo de infraestrutura

**Mitigações:**
- Cluster RabbitMQ para HA
- Monitoramento da fila
- Backup de configurações

---

## ADR-005: Cache - Redis

**Data:** 14/08/2026  
**Status:** Aceito

### Contexto
Requisito de performance (RNF-004) exige:
- Consulta de consolidado < 200ms (P95)
- Picos de 50 req/s

### Decisão
Usar Redis como cache para consultas de consolidado diário.

**Estratégia:**
- Cache read-through
- TTL de 5 minutos
- Invalidação ao processar novo lançamento

### Consequências

**Positivos:**
- ✅ Latência muito baixa (< 10ms cache hit)
- ✅ Reduz carga no banco de dados
- ✅ Suporta alta concorrência
- ✅ Simples de implementar

**Negativos:**
- ❌ Consistência eventual (cache staleness)
- ❌ Mais um componente
- ❌ Memória limitada

**Mitigações:**
- TTL curto (5 min)
- Invalidação proativa
- Monitoramento de cache hit rate

---

## ADR-006: API Gateway

**Data:** 14/08/2026  
**Status:** Proposto

### Contexto
Dois serviços expõem APIs que precisam de:
- Autenticação/autorização centralizada
- Rate limiting
- Routing
- Monitoramento unificado

### Decisão
Usar API Gateway (Nginx/Kong ou cloud-native como AWS API Gateway)

**Responsabilidades:**
- Autenticação centralizada (JWT)
- Rate limiting por consumidor
- Routing para serviços
- SSL termination
- Request/response logging

### Consequências

**Positivos:**
- ✅ Segurança centralizada
- ✅ Single entry point
- ✅ Cross-cutting concerns no gateway
- ✅ Simplifica clientes

**Negativos:**
- ❌ Mais um componente
- ❌ Single point of failure
- ❌ Latência adicional

**Mitigações:**
- Gateway em HA
- Health checks
- Circuit breaker

---

## ADR-007: Idempotência em Lançamentos

**Data:** 14/08/2026  
**Status:** Aceito

### Contexto
RNF-003 exige < 5% perda de requisições, o que implica retry. Retries podem causar duplicações.

### Decisão
Implementar idempotência usando idempotency key.

**Implementação:**
- Cliente envolve header `Idempotency-Key`
- Serviço verifica se key já foi processada
- Se sim, retorna resultado cacheado
- Se não, processa e cacheia resultado

### Consequências

**Positivos:**
- ✅ Permite retry seguro
- ✅ Evita duplicações
- ✅ Melhora confiabilidade

**Negativos:**
- ❌ Complexidade adicional
- ❌ Storage para cache de respostas
- ❌ Expiração de keys

**Mitigações:**
- TTL para idempotency keys (24h)
- Redis para cache de respostas
- Documentação clara para clientes

---

## ADR-008: Observabilidade - Prometheus + Grafana

**Data:** 14/08/2026  
**Status:** Proposto (Diferencial)

### Contexto
RNF-006 exige observabilidade para:
- Monitorar saúde dos serviços
- Detectar anomalias
- Troubleshooting

### Decisão
Usar stack Prometheus + Grafana para monitoramento.

**Métricas a coletar:**
- Requests por segundo
- Latência (P50, P95, P99)
- Error rate
- Queue depth (RabbitMQ)
- Cache hit rate (Redis)
- Database connections

### Consequências

**Positivos:**
- ✅ Dashboard visual
- ✅ Alertas configuráveis
- ✅ Padrão de mercado
- ✅ Open source

**Negativos:**
- ❌ Curva de aprendizado
- ❌ Setup complexo
- ❌ Storage de métricas

**Mitigações:**
- Dashboards pré-configurados
- Documentação
- Retention policy otimizada

---

## Resumo das Decisões

| ADR | Decisão | Status | Impacto |
|-----|---------|--------|---------|
| ADR-001 | Event-Driven Architecture | Aceito | Alto - Core da solução |
| ADR-002 | Microsserviços | Aceito | Alto - Core da solução |
| ADR-003 | PostgreSQL | Aceito | Médio - Infraestrutura |
| ADR-004 | RabbitMQ | Aceito | Alto - Core da solução |
| ADR-005 | Redis | Aceito | Médio - Performance |
| ADR-006 | API Gateway | Proposto | Médio - Segurança |
| ADR-007 | Idempotência | Aceito | Médio - Confiabilidade |
| ADR-008 | Prometheus+Grafana | Proposto | Baixo - Diferencial |

---

## Decisões Pendentes

### TBD-001: Linguagem de Programação ✅ RESOLVIDO
**Contexto:** Aguardando definição do candidato

**Opções:**
- Node.js/TypeScript (ecossistema rico, bom para I/O)
- Python (simplicidade, bom para data processing)
- Java (robusto, empresarial)
- Go (performance, concorrência)

**Decisão:** C# / .NET 8

**Racional:**
- Candidato domina C#
- .NET 8 é moderno, performático e open-source
- Excelente suporte a microsserviços
- Entity Framework Core é maduro e robusto
- Boa integração com PostgreSQL
- Comunidade ativa e suporte empresarial

**Data decisão:** 14/08/2026

---

### TBD-002: Cloud Provider ✅ RESOLVIDO
**Contexto:** Para estimativa de custos

**Opções:**
- AWS (mais maduro, mais opções)
- Azure (integração Microsoft)
- GCP (bom preço-performance)

**Decisão:** AWS

**Racional:**
- Escolha do candidato
- Serviços mais maduros (ECS, RDS, ElastiCache, SQS/RabbitMQ)
- Ampla documentação e comunidade
- Boa integração com .NET
- Serviços gerenciados reduzem operacional

**Data decisão:** 14/08/2026

---

### TBD-003: Deploy Strategy ✅ RESOLVIDO
**Contexto:** Como implantar os serviços

**Opções:**
- Docker Compose (local/teste)
- Kubernetes (produção)
- Serverless (lambda functions)
- PaaS (Heroku, Render)

**Decisão:** Docker Compose (local) + AWS ECS (produção)

**Racional:**
- Docker Compose para desenvolvimento e testes locais
- AWS ECS para produção (gerenciado, escalável)
- Containerização garante consistência entre ambientes
- ECS Fargate elimina necessidade de gerenciar clusters

**Data decisão:** 14/08/2026

---

## ADR-009: ORM - Entity Framework Core

**Data:** 14/08/2026  
**Status:** Aceito

### Contexto
Necessidade de acesso a dados em C# com PostgreSQL, de forma produtiva e type-safe.

### Decisão
Usar Entity Framework Core como ORM.

**Racional:**
- Candidato domina EF
- Integração nativa com .NET
- Migrations automática
- LINQ para queries type-safe
- Suporte a PostgreSQL via Npgsql
- Change tracking automático

### Consequências

**Positivos:**
- ✅ Alta produtividade
- ✅ Type-safe queries
- ✅ Migrations estruturadas
- ✅ Suporte oficial da Microsoft

**Negativos:**
- ❌ Overhead de performance vs ADO.NET puro
- ❌ Complexidade em queries muito complexas

**Mitigações:**
- Usar Dapper para queries críticas se necessário
- Otimizar queries com Include, AsNoTracking
- Monitoring de performance

---

## ADR-010: Frontend - React (Diferencial)

**Data:** 14/08/2026  
**Status:** Aceito

### Contexto
Candidato domina React e isso pode ser um diferencial no desafio, mostrando visão full-stack.

### Decisão
Incluir frontend em React para consumo das APIs.

**Funcionalidades:**
- Formulário de lançamentos
- Dashboard de consolidado diário
- Gráficos de evolução de saldo
- Lista de lançamentos

### Consequências

**Positivos:**
- ✅ Diferencial no desafio (visão completa)
- ✅ Candidato domina React
- ✅ Experiência de usuário completa
- ✅ Demonstração de integração frontend-backend

**Negativos:**
- ❌ Escopo adicional
- ❌ Mais código para manter

**Mitigações:**
- Keep it simple (MVP funcional)
- Usar componentes UI prontos (Material UI, Ant Design)
- Documentar claramente no README

---

**Status:** Documento ativo - Decisões podem ser atualizadas conforme o projeto evolui
