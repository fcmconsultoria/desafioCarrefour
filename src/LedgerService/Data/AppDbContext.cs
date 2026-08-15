using LedgerService.Models;
using Microsoft.EntityFrameworkCore;

namespace LedgerService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Lancamento> Lancamentos { get; set; }
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de Lancamento
        modelBuilder.Entity<Lancamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).HasPrecision(18, 2);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.DataHora).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.DataHora);
            entity.HasIndex(e => e.Tipo);
        });

        // Configuração de IdempotencyKey
        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Response);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();

            entity.HasIndex(e => e.ExpiresAt);
        });
    }
}
