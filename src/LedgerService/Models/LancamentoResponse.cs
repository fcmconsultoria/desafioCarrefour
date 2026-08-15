namespace LedgerService.Models;

public class LancamentoResponse
{
    public Guid Id { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataHora { get; set; }
    public DateTime CreatedAt { get; set; }
}
