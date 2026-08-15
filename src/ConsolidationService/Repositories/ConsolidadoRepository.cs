using ConsolidationService.Data;
using ConsolidationService.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidationService.Repositories;

public class ConsolidadoRepository : IConsolidadoRepository
{
    private readonly AppDbContext _context;

    public ConsolidadoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConsolidadoDiario?> GetByDataAsync(DateOnly data)
    {
        return await _context.ConsolidadosDiarios.FindAsync(data);
    }

    public async Task<IEnumerable<ConsolidadoDiario>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.ConsolidadosDiarios
            .Where(c => c.Data >= startDate && c.Data <= endDate)
            .OrderBy(c => c.Data)
            .ToListAsync();
    }

    public async Task<ConsolidadoDiario> CreateOrUpdateAsync(ConsolidadoDiario consolidado)
    {
        var existing = await _context.ConsolidadosDiarios.FindAsync(consolidado.Data);
        
        if (existing == null)
        {
            consolidado.UpdatedAt = DateTime.UtcNow;
            _context.ConsolidadosDiarios.Add(consolidado);
        }
        else
        {
            existing.TotalCreditos = consolidado.TotalCreditos;
            existing.TotalDebitos = consolidado.TotalDebitos;
            existing.SaldoFinal = consolidado.SaldoFinal;
            existing.QuantidadeLancamentos = consolidado.QuantidadeLancamentos;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return existing ?? consolidado;
    }
}
