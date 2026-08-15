using LedgerService.Data;
using LedgerService.Messaging;
using LedgerService.Repositories;
using LedgerService.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ledgerservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Repositories
builder.Services.AddScoped<ILancamentoRepository, LancamentoRepository>();
builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

// Configurar Services
builder.Services.AddScoped<ILancamentoService, LancamentoService>();

// Configurar RabbitMQ
builder.Services.AddSingleton<ILancamentoEventPublisher>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
    var queueName = configuration["RabbitMQ:QueueName"] ?? "lancamentos";
    return new RabbitMQEventPublisher(hostName, queueName);
});

// Configurar Controllers
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ledger Service API",
        Version = "v1",
        Description = "API para gerenciamento de lançamentos financeiros"
    });
    
    // Documentar Idempotency-Key header
    c.OperationFilter<IdempotencyKeyOperationFilter>();
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ledger Service API v1");
    });
}

app.UseCors("AllowAll");

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "LedgerService", timestamp = DateTime.UtcNow }));

// Limpar chaves de idempotency expiradas (background job)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var idempotencyRepo = services.GetRequiredService<IIdempotencyRepository>();
    
    // Executar limpeza a cada hora
    Task.Run(async () =>
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromHours(1));
            try
            {
                await idempotencyRepo.DeleteExpiredKeysAsync();
                Log.Information("Chaves de idempotency expiradas limpas");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao limpar chaves de idempotency expiradas");
            }
        }
    });
}

app.Run();

// Custom Operation Filter para Swagger
public class IdempotencyKeyOperationFilter : Microsoft.OpenApi.Models.IOperationFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, Microsoft.OpenApi.Models.OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<Microsoft.OpenApi.Models.OpenApiParameter>();

        operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
        {
            Name = "Idempotency-Key",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Chave de idempotência para evitar duplicações em retries",
            Required = false,
            Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
        });
    }
}
