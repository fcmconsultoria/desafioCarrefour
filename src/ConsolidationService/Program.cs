using ConsolidationService.Data;
using ConsolidationService.Messaging;
using ConsolidationService.Repositories;
using ConsolidationService.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/consolidationservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Repositories
builder.Services.AddScoped<IConsolidadoRepository, ConsolidadoRepository>();

// Configurar Services
builder.Services.AddScoped<IConsolidationService, ConsolidationService>();
builder.Services.AddSingleton<ICacheService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
    return new RedisCacheService(connectionString);
});

// Configurar RabbitMQ Consumer
builder.Services.AddSingleton<ILancamentoEventConsumer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
    var queueName = configuration["RabbitMQ:QueueName"] ?? "lancamentos";
    var consolidationService = sp.GetRequiredService<IConsolidationService>();
    var logger = sp.GetRequiredService<ILogger<RabbitMQEventConsumer>>();
    
    return new RabbitMQEventConsumer(hostName, queueName, consolidationService, logger);
});

// Configurar Controllers
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Consolidation Service API",
        Version = "v1",
        Description = "API para consolidação diária de lançamentos financeiros"
    });
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Consolidation Service API v1");
    });
}

app.UseCors("AllowAll");

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ConsolidationService", timestamp = DateTime.UtcNow }));

// Start RabbitMQ consumer in background
using (var scope = app.Services.CreateScope())
{
    var consumer = scope.ServiceProvider.GetRequiredService<ILancamentoEventConsumer>();
    var cts = new CancellationTokenSource();
    
    _ = Task.Run(() => consumer.StartConsumingAsync(cts.Token), cts.Token);
    
    // Ensure consumer is disposed when app shuts down
    app.Lifetime.ApplicationStopping.Register(() =>
    {
        cts.Cancel();
        if (consumer is IDisposable disposable)
        {
            disposable.Dispose();
        }
    });
}

app.Run();
