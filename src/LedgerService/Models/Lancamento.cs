namespace LedgerService.Models;

public class Lancamento
{
    public Guid Id { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; } // "debito" ou "credito"
    public string? Descricao { get; set; }
    public DateTime DataHora { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
