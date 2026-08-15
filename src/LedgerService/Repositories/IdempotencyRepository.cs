using LedgerService.Data;
using LedgerService.Models;
using Microsoft.EntityFrameworkCore;

namespace LedgerService.Repositories;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly AppDbContext _context;

    public IdempotencyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IdempotencyKey?> GetByKeyAsync(string key)
    {
        return await _context.IdempotencyKeys
            .FirstOrDefaultAsync(k => k.Key == key && k.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<IdempotencyKey> CreateAsync(IdempotencyKey idempotencyKey)
    {
        idempotencyKey.CreatedAt = DateTime.UtcNow;
        
        _context.IdempotencyKeys.Add(idempotencyKey);
        await _context.SaveChangesAsync();
        
        return idempotencyKey;
    }

    public async Task<bool> DeleteExpiredKeysAsync()
    {
        var expiredKeys = await _context.IdempotencyKeys
            .Where(k => k.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        if (expiredKeys.Any())
        {
            _context.IdempotencyKeys.RemoveRange(expiredKeys);
            await _context.SaveChangesAsync();
        }

        return true;
    }
}
