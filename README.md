# Desafio Arquiteto de Soluções - Carrefour

## Sistema de Controle de Fluxo de Caixa Diário

Arquitetura de microsserviços para controle de lançamentos financeiros e consolidação diária, desenvolvida em C#/.NET 8 com foco em resiliência, escalabilidade e desacoplamento.

---

## 📋 Visão Geral

Este projeto implementa uma solução completa para o desafio do Carrefour, consistindo em:

- **Ledger Service**: Serviço responsável pelo registro de lançamentos (débitos e créditos)
- **Consolidation Service**: Serviço responsável pelo processamento e consolidação diária
- **Comunicação Assíncrona**: RabbitMQ para desacoplamento entre serviços
- **Cache**: Redis para performance em consultas de consolidado
- **Docker Compose**: Ambiente completo de desenvolvimento

### Requisitos Atendidos

✅ Serviço de controle de lançamentos  
✅ Serviço de consolidado diário  
✅ Mapeamento de domínios funcionais e capacidades de negócio  
✅ Refinamento de requisitos funcionais e não funcionais  
✅ Desenho da solução completo (Arquitetura Alvo)  
✅ Justificativa na decisão/escolha de ferramentas/tecnologias  
✅ Testes  
✅ README com instruções claras  
✅ Requisitos diferenciais (estimativa de custos, segurança, arquitetura de transição)  

---

## 🏗️ Arquitetura

### Stack Tecnológica

| Componente | Tecnologia | Versão |
|-----------|-----------|--------|
| Linguagem | C# / .NET | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Banco de Dados | PostgreSQL | 15 |
| Message Broker | RabbitMQ | 3.12 |
| Cache | Redis | 7 |
| Container | Docker | Latest |
| Logging | Serilog | 8.0 |

### Padrões Arquiteturais

- **Event-Driven Architecture**: Comunicação assíncrona via RabbitMQ
- **Microsserviços**: Serviços independentes e escaláveis
- **CQRS**: Separação de leitura e escrita
- **Idempotência**: Prevenção de duplicações via idempotency keys
- **Cache-Aside**: Redis para performance de leituras

### Diagrama de Arquitetura

```
┌─────────────┐
│   Cliente   │
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Load       │
│  Balancer   │
└──────┬──────┘
       │
       ├───▶ Ledger Service (Port 5001)
       │        │
       │        ├──▶ PostgreSQL (ledgerdb)
       │        └──▶ RabbitMQ (lancamentos queue)
       │
       └───▶ Consolidation Service (Port 5002)
                │
                ├──▶ PostgreSQL (consolidationdb)
                ├──▶ RabbitMQ (consumer)
                └──▶ Redis (cache)
```

---

## 🚀 Como Rodar Localmente

### Pré-requisitos

- Docker Desktop instalado e rodando
- Git
- (Opcional) .NET 8 SDK para desenvolvimento local

### Passo 1: Clonar o Repositório

```bash
git clone <seu-repositorio>
cd desafio
```

### Passo 2: Iniciar os Serviços com Docker Compose

```bash
docker-compose up -d
```

Isso iniciará:
- PostgreSQL para Ledger Service (porta 5432)
- PostgreSQL para Consolidation Service (porta 5433)
- RabbitMQ (portas 5672 e 15672 para Management UI)
- Redis (porta 6379)
- Ledger Service (porta 5001)
- Consolidation Service (porta 5002)

### Passo 3: Verificar Saúde dos Serviços

```bash
# Ledger Service
curl http://localhost:5001/health

# Consolidation Service
curl http://localhost:5002/health
```

### Passo 4: Acessar as APIs

#### Swagger UI

- **Ledger Service**: http://localhost:5001/swagger
- **Consolidation Service**: http://localhost:5002/swagger

#### RabbitMQ Management UI

- **URL**: http://localhost:15672
- **User**: guest
- **Password**: guest

---

## 📚 Uso da API

### Ledger Service (Port 5001)

#### Criar Lançamento

```bash
curl -X POST http://localhost:5001/api/lancamentos \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: unique-key-123" \
  -d '{
    "valor": 100.50,
    "tipo": "credito",
    "descricao": "Venda de produtos"
  }'
```

#### Listar Todos os Lançamentos

```bash
curl http://localhost:5001/api/lancamentos
```

#### Buscar Lançamento por ID

```bash
curl http://localhost:5001/api/lancamentos/{id}
```

#### Listar por Período

```bash
curl "http://localhost:5001/api/lancamentos/periodo?startDate=2026-08-01&endDate=2026-08-14"
```

#### Listar por Tipo

```bash
curl http://localhost:5001/api/lancamentos/tipo/credito
```

### Consolidation Service (Port 5002)

#### Buscar Consolidado por Data

```bash
curl http://localhost:5002/api/consolidado/2026-08-14
```

**Resposta:**
```json
{
  "data": "2026-08-14",
  "totalCreditos": 1500.00,
  "totalDebitos": 500.00,
  "saldoFinal": 1000.00,
  "quantidadeLancamentos": 5,
  "updatedAt": "2026-08-14T10:30:00Z"
}
```

#### Buscar Consolidados por Período

```bash
curl "http://localhost:5002/api/consolidado/periodo?startDate=2026-08-01&endDate=2026-08-14"
```

---

## 🧪 Testes

### Executar Testes

```bash
# Navegar para o projeto de testes
cd src/LedgerService.Tests
dotnet test

cd ../ConsolidationService.Tests
dotnet test
```

**Testes Implementados:**
- ✅ LedgerService.Tests (7 testes unitários)
  - Criação de lançamento com/sem idempotency key
  - Cache de resposta por idempotency
  - Busca por ID
  - Listagem todos
  - Filtro por tipo
  
- ✅ ConsolidationService.Tests (7 testes unitários)
  - Cache hit/miss
  - Processamento de créditos
  - Processamento de débitos
  - Criação de novo consolidado
  - Consulta por período

### Testes Manuais

1. Criar alguns lançamentos
2. Aguardar alguns segundos (processamento assíncrono)
3. Consultar o consolidado do dia
4. Verificar se o saldo está correto

**Exemplo:**

```bash
# Criar lançamento de crédito
curl -X POST http://localhost:5001/api/lancamentos \
  -H "Content-Type: application/json" \
  -d '{"valor": 100.00, "tipo": "credito", "descricao": "Teste"}'

# Criar lançamento de débito
curl -X POST http://localhost:5001/api/lancamentos \
  -H "Content-Type: application/json" \
  -d '{"valor": 50.00, "tipo": "debito", "descricao": "Teste"}'

# Aguardar 2-3 segundos
sleep 3

# Consultar consolidado de hoje
curl http://localhost:5002/api/consolidado/$(date +%Y-%m-%d)

# Deve mostrar: saldoFinal = 50.00 (100 - 50)
```

---

## 📊 Monitoramento

### Logs

Logs são salvos em `logs/` dentro de cada container:

```bash
# Ver logs do Ledger Service
docker logs ledger-service

# Ver logs do Consolidation Service
docker logs consolidation-service

# Ver logs do RabbitMQ
docker logs rabbitmq
```

### Métricas

- **RabbitMQ Management UI**: http://localhost:15672
  - Monitorar fila de mensagens
  - Verificar mensagens enfileiradas
  - Taxa de consumo

- **Health Checks**:
  - http://localhost:5001/health
  - http://localhost:5002/health

---

## 🔧 Configuração

### Variáveis de Ambiente

#### Ledger Service
- `ConnectionStrings__DefaultConnection`: String de conexão PostgreSQL
- `RabbitMQ__HostName`: Host do RabbitMQ
- `RabbitMQ__QueueName`: Nome da fila

#### Consolidation Service
- `ConnectionStrings__DefaultConnection`: String de conexão PostgreSQL
- `RabbitMQ__HostName`: Host do RabbitMQ
- `RabbitMQ__QueueName`: Nome da fila
- `Redis__ConnectionString`: String de conexão Redis

### Database Migrations

As migrations são aplicadas automaticamente na primeira execução via Entity Framework Core.

---

## 📖 Documentação de Arquitetura

Documentação detalhada disponível em `/passoapasso/`:

- [README do Processo](passoapasso/README.md) - Visão geral do processo de elaboração
- [Mapeamento de Domínios](passoapasso/mapeamento_dominios.md) - Domínios funcionais e capacidades
- [Requisitos Detalhados](passoapasso/requisitos_detalhados.md) - Requisitos funcionais e não-funcionais
- [Decisões Arquiteturais](passoapasso/decisoes_arquiteturais.md) - ADRs e justificativas
- [Diagramas](passoapasso/diagramas.md) - Diagramas da arquitetura
- [Estimativa de Custos](passoapasso/estimativa_custos.md) - Custos estimados em AWS
- [Segurança](passoapasso/seguranca.md) - Critérios de segurança
- [Arquitetura de Transição](passoapasso/arquitetura_transicao.md) - Estratégia de migração

---

## 💰 Estimativa de Custos (AWS)

Custo mensal estimado para produção: **$138 - $183 USD**

### Breakdown
- ECS Fargate (Compute): $45/mês
- RDS PostgreSQL: $14/mês
- ElastiCache Redis: $12/mês
- RabbitMQ (self-hosted): $12/mês
- ALB: $17/mês
- CloudWatch: $6/mês
- VPC/NAT Gateway: $33/mês

Veja detalhes completos em [estimativa_custos.md](passoapasso/estimativa_custos.md)

---

## 🔒 Segurança

Implementações de segurança:

- ✅ Idempotência para prevenção de duplicações
- ✅ Validação de input
- ✅ Rate limiting (configurável)
- ✅ Logs estruturados (Serilog)
- ✅ Health checks
- ✅ TLS/SSL (em produção)
- ✅ Network segmentation (VPC em produção)

Critérios detalhados em [seguranca.md](passoapasso/seguranca.md)

---

## 🚀 Evoluções Futuras

Funcionalidades que poderiam ser implementadas:

1. **Frontend React**: Interface web para usuários
2. **Autenticação JWT**: Integração com Identity Server
3. **Monitoramento Avançado**: Prometheus + Grafana
4. **Testes E2E**: Playwright ou Cypress
5. **CI/CD**: GitHub Actions ou Azure DevOps
6. **API Gateway**: Kong ou AWS API Gateway
7. **Distributed Tracing**: Jaeger ou Zipkin
8. **Dead Letter Queue UI**: Interface para mensagens falhas
9. **Backfill Jobs**: Processamento de dados históricos
10. **Multi-tenancy**: Suporte a múltiplos comerciantes

---

## 🛑 Parar os Serviços

```bash
docker-compose down
```

Para remover volumes (dados):

```bash
docker-compose down -v
```

---

## 🐛 Troubleshooting

### Serviços não iniciam

Verifique se as portas estão disponíveis:
- 5001, 5002 (APIs)
- 5432, 5433 (PostgreSQL)
- 5672, 15672 (RabbitMQ)
- 6379 (Redis)

### Erro de conexão com banco

Aguarde a inicialização completa dos bancos de dados (health checks)

### Mensagens não são processadas

Verifique o RabbitMQ Management UI para ver se há mensagens na fila

### Cache não está funcionando

Verifique se o Redis está rodando e acessível

---

## 👤 Autor

Desenvolvido para o desafio de Arquiteto de Soluções do Carrefour.

---

## 📄 Licença

Este projeto foi desenvolvido para fins de demonstração em processo seletivo.

---

**Status**: ✅ 100% Completo e funcional para demonstração

---

## 📖 Documentação Completa

Documentação detalhada disponível em:
- [GUIA_FINAL.md](GUIA_FINAL.md) - Guia completo para submissão
- [RESUMO_IMPLEMENTACAO.md](RESUMO_IMPLEMENTACAO.md) - Resumo técnico
- [passoapasso/](passoapasso/) - Documentação de processo de elaboração
