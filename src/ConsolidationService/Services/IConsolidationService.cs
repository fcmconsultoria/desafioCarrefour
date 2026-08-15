using ConsolidationService.Models;

namespace ConsolidationService.Services;

public interface IConsolidationService
{
    Task<ConsolidadoDiarioResponse?> GetConsolidadoByDataAsync(DateOnly data);
    Task<IEnumerable<ConsolidadoDiarioResponse>> GetConsolidadosByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task ProcessLancamentoAsync(Guid lancamentoId, decimal valor, string tipo, DateTime dataHora);
}
