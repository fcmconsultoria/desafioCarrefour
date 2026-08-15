using LedgerService.Models;

namespace LedgerService.Repositories;

public interface ILancamentoRepository
{
    Task<Lancamento?> GetByIdAsync(Guid id);
    Task<IEnumerable<Lancamento>> GetAllAsync();
    Task<IEnumerable<Lancamento>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Lancamento>> GetByTipoAsync(string tipo);
    Task<Lancamento> CreateAsync(Lancamento lancamento);
    Task<Lancamento?> UpdateAsync(Lancamento lancamento);
    Task<bool> DeleteAsync(Guid id);
}
