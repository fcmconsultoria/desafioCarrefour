using ConsolidationService.Models;
using ConsolidationService.Repositories;

namespace ConsolidationService.Services;

public class ConsolidationService : IConsolidationService
{
    private readonly IConsolidadoRepository _consolidadoRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ConsolidationService> _logger;

    public ConsolidationService(
        IConsolidadoRepository consolidadoRepository,
        ICacheService cacheService,
        ILogger<ConsolidationService> logger)
    {
        _consolidadoRepository = consolidadoRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ConsolidadoDiarioResponse?> GetConsolidadoByDataAsync(DateOnly data)
    {
        // Try cache first
        var cacheKey = $"consolidado:{data:yyyy-MM-dd}";
        var cached = await _cacheService.GetAsync<ConsolidadoDiarioResponse>(cacheKey);
        
        if (cached != null)
        {
            _logger.LogInformation("Cache hit para consolidado do dia {Data}", data);
            return cached;
        }

        // Cache miss - get from database
        var consolidado = await _consolidadoRepository.GetByDataAsync(data);
        
        if (consolidado == null)
        {
            return null;
        }

        var response = MapToResponse(consolidado);
        
        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
        
        return response;
    }

    public async Task<IEnumerable<ConsolidadoDiarioResponse>> GetConsolidadosByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        var consolidados = await _consolidadoRepository.GetByDateRangeAsync(startDate, endDate);
        return consolidados.Select(MapToResponse);
    }

    public async Task ProcessLancamentoAsync(Guid lancamentoId, decimal valor, string tipo, DateTime dataHora)
    {
        try
        {
            var data = DateOnly.FromDateTime(dataHora);
            
            _logger.LogInformation("Processando lançamento {LancamentoId} para data {Data}", lancamentoId, data);

            // Get current consolidado
            var consolidado = await _consolidadoRepository.GetByDataAsync(data) ?? new ConsolidadoDiario
            {
                Data = data,
                TotalCreditos = 0,
                TotalDebitos = 0,
                SaldoFinal = 0,
                QuantidadeLancamentos = 0
            };

            // Update based on tipo
            if (tipo == "credito")
            {
                consolidado.TotalCreditos += valor;
            }
            else if (tipo == "debito")
            {
                consolidado.TotalDebitos += valor;
            }

            consolidado.QuantidadeLancamentos++;
            consolidado.SaldoFinal = consolidado.TotalCreditos - consolidado.TotalDebitos;

            // Save to database
            await _consolidadoRepository.CreateOrUpdateAsync(consolidado);

            // Invalidate cache for this date
            var cacheKey = $"consolidado:{data:yyyy-MM-dd}";
            await _cacheService.RemoveAsync(cacheKey);

            _logger.LogInformation("Consolidado atualizado para data {Data}: SaldoFinal={Saldo}", data, consolidado.SaldoFinal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar lançamento {LancamentoId}", lancamentoId);
            throw;
        }
    }

    private static ConsolidadoDiarioResponse MapToResponse(ConsolidadoDiario consolidado)
    {
        return new ConsolidadoDiarioResponse
        {
            Data = consolidado.Data,
            TotalCreditos = consolidado.TotalCreditos,
            TotalDebitos = consolidado.TotalDebitos,
            SaldoFinal = consolidado.SaldoFinal,
            QuantidadeLancamentos = consolidado.QuantidadeLancamentos,
            UpdatedAt = consolidado.UpdatedAt
        };
    }
}
