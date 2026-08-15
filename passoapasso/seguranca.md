# Critérios de Segurança

## Visão Geral

Este documento define os critérios de segurança para consumo e integração dos serviços, atendendo ao requisito RNF-005 e ao diferencial de segurança.

---

## Princípios de Segurança

1. **Defense in Depth:** Múltiplas camadas de segurança
2. **Zero Trust:** Nunca confiar, sempre verificar
3. **Least Privilege:** Mínimo acesso necessário
4. **Security by Design:** Segurança desde o design

---

## 1. Autenticação

### 1.1 Estratégia de Autenticação

**Mecanismo:** JWT (JSON Web Tokens)

**Flow:**
1. Cliente obtém token de autenticação
2. Token incluído no header `Authorization: Bearer <token>`
3. API valida token em cada requisição
4. Token contém claims (user_id, roles, expiration)

**Implementação:**
```csharp
// .NET 8 - JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "carrefour-challenge",
            ValidAudience = "carrefour-challenge-api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
        };
    });
```

**Rotas Públicas vs Protegidas:**
- `/health` - Pública (health check)
- `/api/lancamentos` - Protegida (requer autenticação)
- `/api/consolidado` - Protegida (requer autenticação)

---

### 1.2 Alternativa: API Keys

Para simplicidade no desafio, pode-se usar API Keys:

**Header:** `X-API-Key: <key>`

**Implementação:**
```csharp
// Custom API Key Middleware
app.UseMiddleware<ApiKeyMiddleware>();
```

**Armazenamento:** Environment variables ou AWS Secrets Manager

---

## 2. Autorização

### 2.1 RBAC (Role-Based Access Control)

**Roles definidas:**
- `admin`: Acesso total (leitura e escrita)
- `readonly`: Apenas leitura
- `cashier`: Lançamentos apenas
- `manager`: Consolidado e relatórios

**Implementação:**
```csharp
[Authorize(Roles = "admin,manager")]
[HttpPost("api/lancamentos")]
public async Task<IActionResult> CreateLancamento(...)
```

### 2.2 ABAC (Attribute-Based Access Control) - Evolução

Para implementação futura:
- Baseado em atributos do usuário
- Regras dinâmicas
- Mais granular

---

## 3. Criptografia

### 3.1 Criptografia em Trânsito

**Protocolo:** TLS 1.3

**Implementação:**
- API Gateway com SSL/TLS termination
- Certificados gerenciados pelo AWS Certificate Manager (ACM)
- Forced HTTPS (redirect HTTP → HTTPS)

```csharp
// .NET - Force HTTPS
app.UseHttpsRedirection();
app.UseHsts();
```

### 3.2 Criptografia em Repouso

**Banco de Dados (RDS):**
- Encryption at rest habilitado
- AWS KMS para gerenciamento de chaves
- TDE (Transparent Data Encryption)

**Cache (Redis):**
- Encryption in transit (TLS)
- Authentication token
- At-rest encryption (ElastiCache)

**Secrets:**
- AWS Secrets Manager para armazenar:
  - Connection strings
  - API keys
  - JWT secrets
  - RabbitMQ credentials

---

## 4. Validação de Input

### 4.1 Validação de API

**Campos obrigatórios:**
```csharp
public class CreateLancamentoRequest
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Valor { get; set; }
    
    [Required]
    [RegularExpression("^(debito|credito)$", ErrorMessage = "Tipo deve ser 'debito' ou 'credito'")]
    public string Tipo { get; set; }
    
    [MaxLength(500)]
    public string Descricao { get; set; }
}
```

### 4.2 Sanitização de Input

- Escape de SQL injection (EF Core já protege)
- Validação de tipos de dados
- Limitação de tamanho de strings
- Whitelist de valores aceitos

### 4.3 Proteção contra Mass Assignment

- Usar DTOs (Data Transfer Objects)
- Não expor entidades diretamente
- Bind explícito de propriedades

---

## 5. Rate Limiting

### 5.1 Estratégia de Rate Limiting

**Por IP/Cliente:**
- Ledger Service: 100 req/min
- Consolidation Service: 200 req/min

**Implementação:**
```csharp
// .NET - Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("LancamentoPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 10
            }));
});
```

### 5.2 Rate Limiting por Token

- Por API Key ou JWT token
- Prevenir abuso por cliente específico
- Configuração no API Gateway

---

## 6. Proteção contra Ataques Comuns

### 6.1 SQL Injection

**Proteção:**
- EF Core (parameterized queries automaticamente)
- Nunca concatenar strings SQL
- Usar Stored Procedures se necessário

### 6.2 XSS (Cross-Site Scripting)

**Proteção:**
- Validação de input
- Encoding de output
- Content Security Policy (CSP)
- Headers de segurança

```csharp
// Security Headers
app.UseSecurityHeaders(new SecurityHeadersPolicy
{
    ContentSecurityPolicy = "default-src 'self'",
    XContentTypeOptions = "nosniff",
    XFrameOptions = "DENY",
    XSSProtection = "1; mode=block"
});
```

### 6.3 CSRF (Cross-Site Request Forgery)

**Proteção:**
- Anti-forgery tokens em state-changing operations
- SameSite cookie attribute
- Verificação de Origin header

### 6.4 DoS/DDoS

**Proteção:**
- Rate limiting (já mencionado)
- AWS Shield (standard grátis, advanced pago)
- CloudFlare (opcional)
- Auto-scaling com alarmes

---

## 7. Segurança de Comunicação entre Serviços

### 7.1 Service-to-Service Authentication

**mTLS (Mutual TLS):**
- Certificados para cada serviço
- Comunicação segura entre microsserviços
- Implementação via AWS Certificate Manager

**Alternativa simplificada:**
- Shared secret via AWS Secrets Manager
- HMAC signatures

### 7.2 Network Segmentation

**VPC Design:**
- Subnets privadas para serviços
- Subnets públicas apenas para ALB
- Security Groups restritivos
- NACLs (Network ACLs)

**Regras de Security Group:**
```yaml
# Ledger Service SG
Inbound:
  - From: ALB SG, Port: 443, Protocol: TCP
  - From: Consolidation SG, Port: 443, Protocol: TCP (se necessário)
Outbound:
  - To: RDS SG, Port: 5432, Protocol: TCP
  - To: RabbitMQ SG, Port: 5671, Protocol: TCP
  - To: Redis SG, Port: 6379, Protocol: TCP
```

---

## 8. Logging e Auditoria

### 8.1 Audit Log

**O que logar:**
- Quem fez a requisição (user_id, ip)
- O que foi feito (ação, recurso)
- Quando foi feito (timestamp)
- Resultado (sucesso/falha)

**Implementação:**
```csharp
// Audit Middleware
app.UseMiddleware<AuditLoggingMiddleware>();
```

**Estrutura do log:**
```json
{
  "timestamp": "2026-08-14T10:30:00Z",
  "user_id": "user-123",
  "ip": "192.168.1.1",
  "action": "CREATE_LANCAMENTO",
  "resource": "lancamento-456",
  "result": "success",
  "details": { "valor": 100.00, "tipo": "credito" }
}
```

### 8.2 Sensitive Data

**NÃO logar:**
- Senhas
- Tokens completos
- Dados PII (Personal Identifiable Information)
- Números de cartão

**Mascarar dados sensíveis:**
```csharp
public class AuditLog
{
    public string UserId { get; set; }
    public string Action { get; set; }
    public string SanitizedPayload => MaskSensitiveData(payload);
}
```

---

## 9. Secrets Management

### 9.1 AWS Secrets Manager

**Secrets armazenados:**
```yaml
# /prod/ledger-service/db
{
  "host": "ledger-db.xxxx.us-east-1.rds.amazonaws.com",
  "username": "ledger_user",
  "password": "encrypted_password"
}

# /prod/jwt
{
  "key": "jwt_secret_key",
  "issuer": "carrefour-challenge",
  "audience": "carrefour-challenge-api"
}

# /prod/rabbitmq
{
  "host": "rabbitmq.xxxx.us-east-1.amazonaws.com",
  "username": "rabbit_user",
  "password": "encrypted_password"
}
```

**Rotação automática:**
- Senhas de banco: 30 dias
- JWT keys: 90 dias
- API keys: 180 dias

### 9.2 Environment Variables (Desenvolvimento)

Para desenvolvimento local:
```bash
# .env file (não commitar)
DB_CONNECTION_STRING=...
JWT_KEY=...
RABBITMQ_HOST=...
```

---

## 10. Monitoramento de Segurança

### 10.1 Alertas de Segurança

**Eventos para alertar:**
- Múltiplas falhas de autenticação do mesmo IP
- Rate limit excedido
- Acesso de IP suspeito
- Erros de autorização inesperados
- Alterações em configurações críticas

**Implementação:**
```csharp
// CloudWatch Alarms
- AuthFailureRate > 10/min
- RateLimitExceeded > 5/min
- UnauthorizedAccess > 1/min
```

### 10.2 SIEM (Security Information and Event Management)

**Integração futura:**
- AWS Security Hub
- AWS GuardDuty
- Splunk ou ELK Stack

---

## 11. Compliance

### 11.1 LGPD (Brasil)

**Considerações:**
- Dados pessoais apenas se necessário
- Consentimento explícito
- Direito ao esquecimento
- Notificação de breaches

### 11.2 PCI DSS (se aplicável)

**Se processar pagamentos:**
- Never store full PAN
- Encryption in transit and at rest
- Regular security assessments
- Access control

---

## 12. Checklist de Segurança

### Antes do Deploy

- [ ] Secrets em AWS Secrets Manager (não em código)
- [ ] TLS/SSL habilitado em todos os endpoints
- [ ] Rate limiting configurado
- [ ] Security Groups restritivos
- [ ] Input validation implementado
- [ ] Dependencies atualizadas (sem CVEs conhecidos)
- [ ] CORS configurado corretamente
- [ ] Security headers implementados
- [ ] Audit logging habilitado
- [ ] Health checks sem autenticação

### Monitoramento Contínuo

- [ ] Monitorar tentativas de intrusão
- [ ] Revisar logs de acesso regularmente
- [ ] Atualizar dependências mensalmente
- [ ] Revisar permissões trimestralmente
- [ ] Testar backup e restore
- [ ] Realizar penetration testing anual

---

## 13. Segurança no Frontend (React)

### 13.1 Best Practices

- Nunca expor secrets no frontend
- Usar environment variables para configuração
- Validar input no frontend E backend
- Implementar CSRF protection
- Usar Content Security Policy
- Sanitizar user input (DOMPurify)

### 13.2 Autenticação no Frontend

```javascript
// Armazenar token em httpOnly cookie (não localStorage)
// Ou em memory com auto-refresh
// Nunca expor token em URL
```

---

## 14. Resumo

| Camada | Medida de Segurança |
|--------|-------------------|
| Network | VPC, Security Groups, TLS |
| Application | JWT, Rate Limiting, Input Validation |
| Data | Encryption at rest, KMS |
| Secrets | AWS Secrets Manager |
| Monitoring | CloudWatch, Audit Logs |
| Compliance | LGPD-ready |

---

**Status:** Documento base - Implementação deve seguir estas diretrizes
