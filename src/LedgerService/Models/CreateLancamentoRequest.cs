using System.ComponentModel.DataAnnotations;

namespace LedgerService.Models;

public class CreateLancamentoRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [Required]
    [RegularExpression("^(debito|credito)$", ErrorMessage = "Tipo deve ser 'debito' ou 'credito'")]
    public string Tipo { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string? Descricao { get; set; }
}
