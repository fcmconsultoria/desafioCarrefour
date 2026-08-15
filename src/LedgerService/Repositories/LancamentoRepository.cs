using LedgerService.Data;
using LedgerService.Models;
using Microsoft.EntityFrameworkCore;

namespace LedgerService.Repositories;

public class LancamentoRepository : ILancamentoRepository
{
    private readonly AppDbContext _context;

    public LancamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Lancamento?> GetByIdAsync(Guid id)
    {
        return await _context.Lancamentos.FindAsync(id);
    }

    public async Task<IEnumerable<Lancamento>> GetAllAsync()
    {
        return await _context.Lancamentos
            .OrderByDescending(l => l.DataHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lancamento>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Lancamentos
            .Where(l => l.DataHora >= startDate && l.DataHora <= endDate)
            .OrderByDescending(l => l.DataHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<Lancamento>> GetByTipoAsync(string tipo)
    {
        return await _context.Lancamentos
            .Where(l => l.Tipo == tipo)
            .OrderByDescending(l => l.DataHora)
            .ToListAsync();
    }

    public async Task<Lancamento> CreateAsync(Lancamento lancamento)
    {
        lancamento.CreatedAt = DateTime.UtcNow;
        lancamento.UpdatedAt = DateTime.UtcNow;
        
        _context.Lancamentos.Add(lancamento);
        await _context.SaveChangesAsync();
        
        return lancamento;
    }

    public async Task<Lancamento?> UpdateAsync(Lancamento lancamento)
    {
        var existing = await _context.Lancamentos.FindAsync(lancamento.Id);
        if (existing == null)
            return null;

        existing.Valor = lancamento.Valor;
        existing.Tipo = lancamento.Tipo;
        existing.Descricao = lancamento.Descricao;
        existing.DataHora = lancamento.DataHora;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var lancamento = await _context.Lancamentos.FindAsync(id);
        if (lancamento == null)
            return false;

        _context.Lancamentos.Remove(lancamento);
        await _context.SaveChangesAsync();
        return true;
    }
}
