# Estimativa de Custos - AWS

## Cenário Base
- **Pico de carga:** 50 requisições/segundo no serviço de consolidação
- **Disponibilidade:** 99.5% (SLA)
- **Região:** us-east-1 (N. Virginia)
- **Moeda:** USD

---

## Serviços AWS Utilizados

### 1. Compute - AWS ECS Fargate

**Serviço de Lançamentos (Ledger Service)**
- 3 tarefas (pico) x 0.25 vCPU x 0.5 GB RAM
- Custo: $0.04048 per vCPU-hour + $0.00453 per GB-hour
- Custo hora: (3 × 0.25 × $0.04048) + (3 × 0.5 × $0.00453) = $0.03036 + $0.00680 = $0.03716/hora
- Custo mês (730h): $0.03716 × 730 = **$27.13/mês**

**Serviço de Consolidação (Consolidation Service)**
- 2 tarefas (pico) x 0.25 vCPU x 0.5 GB RAM
- Custo hora: (2 × 0.25 × $0.04048) + (2 × 0.5 × $0.00453) = $0.02024 + $0.00453 = $0.02477/hora
- Custo mês (730h): $0.02477 × 730 = **$18.08/mês**

**Total Compute:** **$45.21/mês**

---

### 2. Banco de Dados - Amazon RDS PostgreSQL

**Instância:** db.t3.micro (1 vCPU, 1 GB RAM)
- Multi-AZ: Não (para reduzir custo em PoC)
- Storage: 20 GB General Purpose SSD (gp3)
- IOPS: 3.000 (incluído no gp3)
- Custo instância: $0.017/hora × 730h = **$12.41/mês**
- Custo storage: $0.08/GB-mês × 20 GB = **$1.60/mês**
- Backup storage (7 dias): ~5 GB × $0.095 = **$0.48/mês**

**Total RDS:** **$14.49/mês**

*Nota: Em produção, usar Multi-AZ dobraria o custo*

---

### 3. Cache - Amazon ElastiCache Redis

**Instância:** cache.t3.micro (0.5 vCPU, 0.6 GB RAM)
- Cluster mode disabled
- Custo: $0.016/hora × 730h = **$11.68/mês**

**Total ElastiCache:** **$11.68/mês**

---

### 4. Message Broker - Amazon MQ (RabbitMQ)

**Tipo:** mq.t3.micro
- Instance type: mq.t3.micro
- Custo: $0.078/hora × 730h = **$56.94/mês**

*Alternativa: Self-hosted RabbitMQ em ECS*
- Usar ECS Fargate para RabbitMQ
- Custo similar a 1 tarefa: ~$12/mês
- *Recomendado para reduzir custos*

**Total Amazon MQ:** **$56.94/mês** (self-hosted: **$12.00/mês**)

---

### 5. Load Balancer - Application Load Balancer

**ALB:**
- LCU hours: 730 × $0.0225 = **$16.43/mês**
- Data processing: ~10 GB/mês × $0.008 = **$0.08/mês**

**Total ALB:** **$16.51/mês**

---

### 6. Monitoramento - CloudWatch

**Métricas Customizadas:**
- 10 métricas customizadas
- Custo: $0.30/métrica × 10 = **$3.00/mês**

**Logs:**
- ~5 GB logs/mês
- Custo: $0.50/GB × 5 = **$2.50/mês**

**Total CloudWatch:** **$5.50/mês**

---

### 7. VPC e Networking

**NAT Gateway:**
- $0.045/hora × 730h = **$32.85/mês**
- Data processing: ~5 GB × $0.045 = **$0.23/mês**

**Total VPC:** **$33.08/mês**

---

### 8. S3 (Artefatos de Deploy, Backups)

**Storage:**
- ~1 GB artefatos
- Custo: $0.023/GB × 1 = **$0.02/mês**

**Requests:**
- ~1.000 requests/mês
- Custo: $0.0004/1.000 requests = **$0.0004/mês**

**Total S3:** **$0.02/mês**

---

## Resumo de Custos Mensais

| Serviço | Custo (USD) | % do Total |
|---------|-------------|------------|
| ECS Fargate (Compute) | $45.21 | 23% |
| RDS PostgreSQL | $14.49 | 7% |
| ElastiCache Redis | $11.68 | 6% |
| Amazon MQ (RabbitMQ) | $56.94 | 29% |
| ALB | $16.51 | 8% |
| CloudWatch | $5.50 | 3% |
| VPC/NAT Gateway | $33.08 | 17% |
| S3 | $0.02 | 0% |
| **TOTAL** | **$183.43** | **100%** |

---

## Cenário Otimizado (Self-hosted RabbitMQ)

Se substituirmos Amazon MQ por RabbitMQ self-hosted em ECS:

| Serviço | Custo Original | Custo Otimizado | Economia |
|---------|---------------|-----------------|----------|
| Amazon MQ | $56.94 | $12.00 (ECS) | $44.94 |
| **TOTAL** | **$183.43** | **$138.49** | **$44.94 (24%)** |

---

## Cenário Produção (Alta Disponibilidade)

Para produção com HA:

| Adição | Custo Adicional |
|--------|-----------------|
| RDS Multi-AZ | +$12.41 (dobro) |
| ElastiCache Replication | +$11.68 (2 nós) |
| ECS mais instâncias | +$20.00 |
| **TOTAL Produção** | **~$182.58** |
| **TOTAL FINAL** | **$321.07/mês** |

---

## Custos Anuais

| Cenário | Mensal | Anual (12x) |
|---------|--------|-------------|
| Desenvolvimento (básico) | $183.43 | $2,201.16 |
| Otimizado (self-hosted) | $138.49 | $1,661.88 |
| Produção (HA) | $321.07 | $3,852.84 |

---

## Economia com Reserved Instances

Se comprometer 1 ano:

- **RDS:** 20-30% de desconto
- **ElastiCache:** 15-25% de desconto
- **Economia estimada:** ~15-20% no total

**Custo anual com RI:** ~$2,800-$3,100 (produção)

---

## Custos com Free Tier (Primeiros 12 meses)

AWS Free Tier oferece:
- **ECS Fargate:** 750 horas/mês (suficiente para 1 tarefa)
- **RDS:** 750 horas/mês db.t2.micro
- **ElastiCache:** Não incluído
- **ALB:** 750 horas/mês
- **CloudWatch:** 10 métricas customizadas grátis
- **S3:** 5 GB storage grátis

**Custo Free Tier:** ~$50-70/mês (sem ElastiCache e RabbitMQ completo)

---

## Alternativas para Reduzir Custos

### 1. Usar SQS ao invés de RabbitMQ
- SQS é mais barato: $0.40 por milhão de requests
- Para 50 req/s = 4.32M requests/dia = 129.6M/mês
- Custo: 129.6M × $0.40/M = **$51.84/mês**
- *Mas RabbitMQ tem mais features (DLQ nativo, exchanges)*

### 2. Usar t2/t3 micro em vez de t3.small
- Reduz compute em ~50%
- Pode impactar performance

### 3. Eliminar NAT Gateway
- Usar VPC endpoints
- Economia: ~$33/mês

### 4. Usar Docker Compose local para desenvolvimento
- Custo: $0 (apenas desenvolvimento)

---

## Recomendação

**Para o desafio:**
- Usar Docker Compose local (custo $0)
- Documentar custos de produção em AWS
- Custo estimado produção: **$138-183/mês** (sem HA)

**Para produção real:**
- Otimizar com self-hosted RabbitMQ
- Considerar Reserved Instances
- Implementar HA crítico (RDS Multi-AZ)
- Custo estimado: **$250-350/mês**

---

## Observações Importantes

1. **Estimativas conservadoras:** Cenário real pode ser menor ou maior
2. **Escalabilidade automática:** Custos aumentam com escala
3. **Data transfer:** Não incluído (geralmente pequeno para APIs REST)
4. **Suporte:** Não incluído (Business Support: $100/mês)
5. **Licenças:** Não aplicável (tudo open-source/.NET)

---

**Status:** Estimativa inicial - Sujeita a ajustes conforme implementação real
