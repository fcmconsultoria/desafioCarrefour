namespace ConsolidationService.Messaging;

public interface ILancamentoEventConsumer
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
}
