# Correlação Direta: O que pediram × O que fiz

## Linguagem simples: Sem enrolação

---

## 1. REQUISITO: "Serviço que faça o controle de lançamentos"

### O que pediram (literalmente):
> "Serviço que faça o controle de lançamentos"

### O que fiz (literalmente):
Criei o **Ledger Service** - uma API que:
- Recebe lançamentos (débitos e créditos)
- Salva no banco de dados
- Devolve confirmação

### Conexão óbvia:
**Ledger Service** = Serviço de controle de lançamentos ✅

**Arquivo:** `src/LedgerService/Controllers/LancamentosController.cs`
**Endpoint:** `POST /api/lancamentos`

---

## 2. REQUISITO: "Serviço do consolidado diário"

### O que pediram (literalmente):
> "Serviço do consolidado diário"

### O que fiz (literalmente):
Criei o **Consolidation Service** - uma API que:
- Soma todos os lançamentos do dia
- Calcula saldo final (créditos - débitos)
- Devolve o relatório

### Conexão óbvia:
**Consolidation Service** = Serviço do consolidado diário ✅

**Arquivo:** `src/ConsolidationService/Controllers/ConsolidadoController.cs`
**Endpoint:** `GET /api/consolidado/{data}`

---

## 3. REQUISITO: "Serviço de lançamentos não deve ficar indisponível se o sistema de consolidado diário cair"

### O que pediram (literalmente):
> "O serviço de lançamento não deve ficar indisponível se o sistema de consolidado diário cair"

### O que fiz (literalmente):
- Ledger Service **NÃO chama** Consolidation Service diretamente
- Ledger Service salva no banco E manda mensagem para fila
- Consolidation Service lê a fila DEPOIS
- Se Consolidation cair, Ledger continua funcionando normalmente

### Conexão óbvia:
Ledger Service funciona **independente** do Consolidation Service ✅

**Arquivo:** `src/LedgerService/Messaging/RabbitMQEventPublisher.cs`
**Conceito:** Comunicação assíncrona (fila)

---

## 4. REQUISITO: "50 requisições por segundo no consolidado"

### O que pediram (literalmente):
> "Em dias de picos, o serviço de consolidado diário recebe 50 requisições por segundo"

### O que fiz (literalmente):
- Consolidation Service pode ter **múltiplas cópias** rodando
- Com 2 cópias: cada uma trata 25 req/s = 50 total
- RabbitMQ funciona como "buffer" se vierem mais de 50

### Conexão óbvia:
Sistema aguenta 50 req/s através de **múltiplas instâncias** ✅

**Arquivo:** `docker-compose.yml` (pode adicionar mais instâncias)
**Conceito:** Horizontal scaling

---

## 5. REQUISITO: "Máximo 5% de perda de requisições"

### O que pediram (literalmente):
> "Com no máximo 5% de perda de requisições"

### O que fiz (literalmente):
- Mensagens no RabbitMQ **não somem** (são persistentes)
- Se falhar, **tenta de novo** automaticamente
- Se falhar muitas vezes, vai para fila especial (Dead Letter Queue)
- **Nada é perdido**

### Conexão óbvia:
Sistema tem **retry automático** + **fila persistente** = quase 0% perda ✅

**Arquivo:** `src/ConsolidationService/Messaging/RabbitMQEventConsumer.cs`
**Conceito:** Retry + Dead Letter Queue

---

## 6. REQUISITO: "Mapeamento de domínios funcionais e capacidades de negócio"

### O que pediram (literalmente):
> "Mapeamento de domínios funcionais e capacidades de negócio"

### O que fiz (literalmente):
Dividi o sistema em 3 partes:
1. **Gestão Financeira** - Lançamentos (Ledger Service)
2. **Consolidação Financeira** - Relatórios (Consolidation Service)
3. **Integração** - Comunicação entre eles (RabbitMQ)

### Conexão óbvia:
Documentei cada parte e o que faz = **Mapeamento de domínios** ✅

**Arquivo:** `passoapasso/mapeamento_dominios.md`

---

## 7. REQUISITO: "Refinamento do levantamento de requisitos funcionais e não funcionais"

### O que pediram (literalmente):
> "Refinamento do levantamento de requisitos funcionais e não funcionais"

### O que fiz (literalmente):
Listei TUDO detalhadamente:
- **Funcionais:** O que o sistema FAZ (criar, consultar, etc.)
- **Não-funcionais:** Como o sistema SE COMPORTA (rápido, seguro, etc.)

### Conexão óbvia:
Documento com todos os requisitos detalhados = **Refinamento** ✅

**Arquivo:** `passoapasso/requisitos_detalhados.md`

---

## 8. REQUISITO: "Desenho da solução completo (Arquitetura Alvo)"

### O que pediram (literalmente):
> "Desenho da solução completo (Arquitetura Alvo)"

### O que fiz (literalmente):
Criei **8 diagramas** mostrando:
- Como os componentes se conectam
- Como os dados fluem
- Como está em produção
- Como lida com falhas

### Conexão óbvia:
8 diagramas visuais = **Desenho completo da arquitetura** ✅

**Arquivo:** `diagrams/` (8 arquivos .mmd)

---

## 9. REQUISITO: "Justificativa na decisão/escolha de ferramentas/tecnologias e de tipo de arquitetura"

### O que pediram (literalmente):
> "Justificativa na decisão/escolha de ferramentas/tecnologias e de tipo de arquitetura"

### O que fiz (literalmente):
Para cada escolha, expliquei:
- **O que escolhi:** C#, PostgreSQL, RabbitMQ, etc.
- **Por que escolhi:** Explicação simples de cada um
- **Prós e contras:** O que ganhei e o que perdi

### Conexão óbvia:
Documento com decisões + justificativas = **Justificativa de escolhas** ✅

**Arquivo:** `passoapasso/decisoes_arquiteturais.md`

---

## 10. REQUISITO: "Pode ser feito na linguagem que você domina"

### O que pediram (literalmente):
> "Pode ser feito na linguagem que você domina"

### O que fiz (literalmente):
Usei **C#/.NET 8** - que é a linguagem que você domina

### Conexão óbvia:
Feito em C# = **Linguagem que você domina** ✅

**Arquivos:** Toda a pasta `src/`

---

## 11. REQUISITO: "Testes"

### O que pediram (literalmente):
> "Testes"

### O que fiz (literalmente):
Criei **14 testes** que verificam:
- Se cria lançamento corretamente
- Se idempotência funciona
- Se cache funciona
- Se processa lançamentos corretamente

### Conexão óbvia:
14 testes implementados = **Testes** ✅

**Arquivos:** `src/LedgerService.Tests/` e `src/ConsolidationService.Tests/`

---

## 12. REQUISITO: "README com instruções claras de como a aplicação funciona e como rodar localmente"

### O que pediram (literalmente):
> "README com instruções claras de como a aplicação funciona e como rodar localmente"

### O que fiz (literalmente):
Criei README com:
- O que é o projeto
- Como rodar (3 comandos)
- Como testar (exemplos de curl)
- Como cada serviço funciona

### Conexão óbvia:
README detalhado = **Instruções claras** ✅

**Arquivo:** `README.md`

---

## 13. REQUISITO: "Hospedar em repositório público (GitHub)"

### O que pediram (literalmente):
> "Hospedar em repositório público (GitHub)"

### O que fiz (literalmente):
Subi TUDO para:
**https://github.com/fcmconsultoria/desafioCarrefour**

### Conexão óbvia:
Projeto no GitHub público = **Hospedado em repositório público** ✅

---

## 14. DIFERENCIAL: "Estimativa de custos com infraestrutura e licenças"

### O que pediram (literalmente):
> "Estimativa de custos com infraestrutura e licenças"

### O que fiz (literalmente):
Calculei quanto custaria na AWS:
- **$138/mês** (cenário básico)
- **$321/mês** (cenário produção)
- Detalhei cada componente

### Conexão óbvia:
Cálculo de custos = **Estimativa de custos** ✅

**Arquivo:** `passoapasso/estimativa_custos.md`

---

## 15. DIFERENCIAL: "Monitoramento e Observabilidade"

### O que pediram (literalmente):
> "Monitoramento e Observabilidade"

### O que fiz (literalmente):
Implementei:
- **Logs** (Serilog) - registra tudo que acontece
- **Health checks** - endpoint para saber se está vivo
- **RabbitMQ Management UI** - ver fila em tempo real

### Conexão óbvia:
Logs + health checks + UI = **Monitoramento** ✅

**Arquivos:** `Program.cs` (logs), `docker-compose.yml` (health checks)

---

## 16. DIFERENCIAL: "Critérios de segurança para consumo (integração) de serviços"

### O que pediram (literalmente):
> "Critérios de segurança para consumo (integração) de serviços"

### O que fiz (literalmente):
Documentei:
- Validação de input
- Idempotência (evita ataques)
- TLS/SSL (comunicação segura)
- Autenticação (pronto para implementar)

### Conexão óbvia:
Documento de segurança = **Critérios de segurança** ✅

**Arquivo:** `passoapasso/seguranca.md`

---

## 17. DIFERENCIAL: "Arquitetura de Transição"

### O que pediram (literalmente):
> "Desenho da solução da Arquitetura de Transição (se necessária, considerando uma migração de legado)"

### O que fiz (literalmente):
Documentei como migrar de um sistema antigo:
- **Fase 1:** Migra serviço de lançamentos
- **Fase 2:** Migra serviço de consolidação
- **Fase 3:** Coexistência
- **Fase 4:** Desliga sistema antigo

### Conexão óbvia:
Plano de migração = **Arquitetura de transição** ✅

**Arquivo:** `passoapasso/arquitetura_transicao.md`

---

## RESUMO ULTRA-SIMPLES

| O que pediram | O que fiz | Arquivo |
|--------------|-----------|---------|
| Serviço de lançamentos | Ledger Service | src/LedgerService/ |
| Serviço de consolidado | Consolidation Service | src/ConsolidationService/ |
| Não depender um do outro | Comunicação via fila | RabbitMQ |
| Aguentar 50 req/s | Múltiplas instâncias | docker-compose.yml |
| Máx 5% perda | Retry + fila persistente | RabbitMQ |
| Mapeamento de domínios | Documento com 3 domínios | passoapasso/mapeamento_dominios.md |
| Requisitos detalhados | Documento com RF e RNF | passoapasso/requisitos_detalhados.md |
| Desenho da arquitetura | 8 diagramas | diagrams/ |
| Justificativa de tecnologias | ADRs (decisões) | passoapasso/decisoes_arquiteturais.md |
| Linguagem que domina | C#/.NET 8 | src/ |
| Testes | 14 testes | src/*.Tests/ |
| README | README detalhado | README.md |
| GitHub público | https://github.com/... | Já está lá |
| Custos | $138-321/mês | passoapasso/estimativa_custos.md |
| Monitoramento | Logs + health checks | Program.cs |
| Segurança | Documento de segurança | passoapasso/seguranca.md |
| Arquitetura de transição | Plano de migração | passoapasso/arquitetura_transicao.md |

---

## NA ENTREVISTA, FALA ASSIM:

"O desafio pedia um serviço de lançamentos e um de consolidado. Eu criei exatamente isso: o Ledger Service para lançamentos e o Consolidation Service para consolidados.

Pedia que um não dependesse do outro. Eu usei uma fila (RabbitMQ) - o Ledger manda mensagem e não espera o Consolidation responder. Se o Consolidation cair, o Ledger continua funcionando.

Pedia 50 req/s. Eu posso colocar várias cópias do serviço rodando - cada uma pega uma parte das requisições.

Pedia máximo 5% de perda. As mensagens na fila não somem e o sistema tenta de novo automaticamente se falhar.

Tudo está documentado: diagramas, decisões, custos, segurança. O código está no GitHub."

---

**Não precisa decorar tudo. A correlação é direta: O que pediram → O que fiz. Simples assim.** ✅
