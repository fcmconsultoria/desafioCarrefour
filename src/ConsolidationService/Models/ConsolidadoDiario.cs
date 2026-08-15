namespace ConsolidationService.Models;

public class ConsolidadoDiario
{
    public DateOnly Data { get; set; }
    public decimal TotalCreditos { get; set; }
    public decimal TotalDebitos { get; set; }
    public decimal SaldoFinal { get; set; }
    public int QuantidadeLancamentos { get; set; }
    public DateTime UpdatedAt { get; set; }
}
