using ConsolidationService.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidationService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ConsolidadoDiario> ConsolidadosDiarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConsolidadoDiario>(entity =>
        {
            entity.HasKey(e => e.Data);
            entity.Property(e => e.TotalCreditos).HasPrecision(18, 2);
            entity.Property(e => e.TotalDebitos).HasPrecision(18, 2);
            entity.Property(e => e.SaldoFinal).HasPrecision(18, 2);
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }
}
