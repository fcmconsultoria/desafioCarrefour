using LedgerService.Models;

namespace LedgerService.Services;

public interface ILancamentoService
{
    Task<LancamentoResponse> CreateLancamentoAsync(CreateLancamentoRequest request, string? idempotencyKey = null);
    Task<LancamentoResponse?> GetLancamentoByIdAsync(Guid id);
    Task<IEnumerable<LancamentoResponse>> GetAllLancamentosAsync();
    Task<IEnumerable<LancamentoResponse>> GetLancamentosByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<LancamentoResponse>> GetLancamentosByTipoAsync(string tipo);
}
