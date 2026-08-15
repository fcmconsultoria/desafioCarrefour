using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace LedgerService.Messaging;

public class RabbitMQEventPublisher : ILancamentoEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    public RabbitMQEventPublisher(string hostName, string queueName)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _queueName = queueName;

        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public async Task PublishLancamentoCriadoAsync(Guid lancamentoId, decimal valor, string tipo, DateTime dataHora)
    {
        var eventData = new
        {
            EventId = Guid.NewGuid(),
            EventType = "LancamentoCriado",
            Timestamp = DateTime.UtcNow,
            Data = new
            {
                LancamentoId = lancamentoId,
                Valor = valor,
                Tipo = tipo,
                DataHora = dataHora
            }
        };

        var message = JsonSerializer.Serialize(eventData);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: string.Empty,
            routingKey: _queueName,
            basicProperties: properties,
            body: body
        );

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
