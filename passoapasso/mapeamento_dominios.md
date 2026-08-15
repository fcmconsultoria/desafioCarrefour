# Mapeamento de Domínios Funcionais e Capacidades de Negócio

## Domínios Funcionais Identificados

### 1. Domínio: Gestão Financeira
**Contexto:** Gerenciamento das transações financeiras do comerciante

**Capacidades de Negócio:**
- Registrar transações (débitos/créditos)
- Consultar transações
- Validar transações
- Calcular saldos

**Responsabilidades:**
- Manter registro de todas as movimentações financeiras
- Garantir integridade dos dados
- Fornecer consistência nos lançamentos

---

### 2. Domínio: Consolidação Financeira
**Contexto:** Processamento e geração de relatórios financeiros

**Capacidades de Negócio:**
- Consolidar transações por período
- Calcular saldo diário
- Gerar relatórios
- Processar em lote (batch)

**Responsabilidades:**
- Agregar dados de transações
- Processar cálculos de saldo
- Entregar relatórios consolidados
- Lidar com processamento assíncrono

---

### 3. Domínio: Integração e Eventos
**Contexto:** Coordenação entre sistemas e comunicação de eventos

**Capacidades de Negócio:**
- Publicar eventos de transações
- Consumir eventos para consolidação
- Garantir entrega de mensagens
- Retry e tratamento de falhas

**Responsabilidades:**
- Desacoplar sistemas através de eventos
- Garantir ordem de processamento
- Implementar padrões de resiliência

---

## Bounded Contexts (Contextos Delimitados)

### Contexto: Lançamentos Financeiros
**Responsabilidade:** Receber e persistir lançamentos
**Boundary:** API REST para entrada de lançamentos
**Comunicação:** Publica eventos na criação de lançamentos

### Contexto: Consolidação Diária
**Responsabilidade:** Processar lançamentos e gerar consolidados
**Boundary:** API para consulta de consolidados + Consumers de eventos
**Comunicação:** Consome eventos de lançamentos

### Contexto: Monitoramento e Observabilidade
**Responsabilidade:** Monitorar saúde e performance dos serviços
**Boundary:** APIs de health check e métricas
**Comunicação:** Coleta métricas de todos os contextos

---

## Capacidades Técnicas Mapeadas

| Capacidade de Negócio | Capacidade Técnica | Implementação Sugerida |
|----------------------|-------------------|------------------------|
| Registrar transações | API REST POST /lancamentos | Endpoint idempotente |
| Consultar transações | API REST GET /lancamentos | Paginação e filtros |
| Consolidar transações | Processamento assíncrono | Message Queue (RabbitMQ/SQS) |
| Calcular saldo diário | Batch job ou stream processing | Worker process |
| Gerar relatórios | API REST GET /consolidado/{data} | Cache de resultados |
| Monitorar sistema | Métricas e logs | Prometheus + Grafana |

---

## Relacionamentos entre Domínios

```
┌─────────────────────────────────────────────────────────────┐
│                    Gestão Financeira                         │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │  Lançamentos     │────────▶│  Eventos         │         │
│  │  (Core Domain)   │         │  (Integration)   │         │
│  └──────────────────┘         └──────────────────┘         │
│                                       │                     │
│                                       ▼                     │
│                          ┌──────────────────┐              │
│                          │  Consolidação    │              │
│                          │  (Supporting)    │              │
│                          └──────────────────┘              │
└─────────────────────────────────────────────────────────────┘
```

**Observação:** A flecha é unidirecional porque o requisito NÃO funcional estabelece que o serviço de lançamentos não pode depender do consolidado. Isso indica uma arquitetura baseada em eventos onde lançamentos publicam e consolidado consome.

---

## Decisões de Design

### 1. Separação de Responsabilidades
**Justificativa:** O requisito de resiliência exige que o serviço de lançamentos funcione independentemente do consolidado.

### 2. Comunicação Assíncrona
**Justificativa:** Desacoplamento entre serviços e capacidade de processar picos de carga (50 req/s) sem impactar o serviço de lançamentos.

### 3. Bounded Contexts Distintos
**Justificativa:** Facilita escalabilidade independente, manutenção e evolução de cada domínio.

---

## Próximos Passos
- [ ] Refinar requisitos funcionais e não-funcionais
- [ ] Definir contratos das APIs
- [ ] Escolher tecnologias específicas
- [ ] Criar diagramas de arquitetura

---

**Status:** Rascunho inicial - Sujeito a revisão após definição de tecnologias
