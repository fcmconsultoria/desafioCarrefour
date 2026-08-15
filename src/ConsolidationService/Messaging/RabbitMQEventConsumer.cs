using ConsolidationService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ConsolidationService.Messaging;

public class RabbitMQEventConsumer : ILancamentoEventConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly IConsolidationService _consolidationService;
    private readonly ILogger<RabbitMQEventConsumer> _logger;

    public RabbitMQEventConsumer(
        string hostName,
        string queueName,
        IConsolidationService consolidationService,
        ILogger<RabbitMQEventConsumer> logger)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _queueName = queueName;
        _consolidationService = consolidationService;
        _logger = logger;

        // Declare queue
        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Configure QoS for fair dispatch
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando consumo de eventos da fila {QueueName}", _queueName);

        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation("Mensagem recebida: {Message}", message);

                var eventData = JsonSerializer.Deserialize<LancamentoEvent>(message);
                
                if (eventData != null && eventData.EventType == "LancamentoCriado")
                {
                    await _consolidationService.ProcessLancamentoAsync(
                        eventData.Data.LancamentoId,
                        eventData.Data.Valor,
                        eventData.Data.Tipo,
                        eventData.Data.DataHora
                    );

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Mensagem processada e ACK enviada");
                }
                else
                {
                    _logger.LogWarning("Mensagem inválida ou tipo desconhecido");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem");
                
                // Nack com requeue para tentar novamente
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer
        );

        // Keep running until cancelled
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
        }

        _logger.LogInformation("Consumo de eventos finalizado");
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }

    private class LancamentoEvent
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public LancamentoData Data { get; set; } = new();
    }

    private class LancamentoData
    {
        public Guid LancamentoId { get; set; }
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
    }
}
