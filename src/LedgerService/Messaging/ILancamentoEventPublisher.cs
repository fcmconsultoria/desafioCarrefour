namespace LedgerService.Messaging;

public interface ILancamentoEventPublisher
{
    Task PublishLancamentoCriadoAsync(Guid lancamentoId, decimal valor, string tipo, DateTime dataHora);
}
