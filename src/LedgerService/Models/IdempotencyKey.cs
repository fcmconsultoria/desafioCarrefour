using System.Text.Json;

namespace LedgerService.Models;

public class IdempotencyKey
{
    public string Key { get; set; } = string.Empty;
    public Guid LancamentoId { get; set; }
    public string? Response { get; set; } // JSON serializado
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public T? GetResponse<T>() where T : class
    {
        if (string.IsNullOrEmpty(Response))
            return null;

        return JsonSerializer.Deserialize<T>(Response);
    }

    public void SetResponse<T>(T response) where T : class
    {
        Response = JsonSerializer.Serialize(response);
    }
}
