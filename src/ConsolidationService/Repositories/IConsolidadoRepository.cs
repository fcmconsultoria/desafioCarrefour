using ConsolidationService.Models;

namespace ConsolidationService.Repositories;

public interface IConsolidadoRepository
{
    Task<ConsolidadoDiario?> GetByDataAsync(DateOnly data);
    Task<IEnumerable<ConsolidadoDiario>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<ConsolidadoDiario> CreateOrUpdateAsync(ConsolidadoDiario consolidado);
}
