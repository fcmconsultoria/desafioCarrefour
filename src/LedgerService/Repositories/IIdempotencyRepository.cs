using LedgerService.Models;

namespace LedgerService.Repositories;

public interface IIdempotencyRepository
{
    Task<IdempotencyKey?> GetByKeyAsync(string key);
    Task<IdempotencyKey> CreateAsync(IdempotencyKey idempotencyKey);
    Task<bool> DeleteExpiredKeysAsync();
}
