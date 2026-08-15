using Microsoft.AspNetCore.Mvc;
using ConsolidationService.Models;
using ConsolidationService.Services;

namespace ConsolidationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsolidadoController : ControllerBase
{
    private readonly IConsolidationService _consolidationService;
    private readonly ILogger<ConsolidadoController> _logger;

    public ConsolidadoController(
        IConsolidationService consolidationService,
        ILogger<ConsolidadoController> logger)
    {
        _consolidationService = consolidationService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o consolidado diário por data
    /// </summary>
    /// <param name="data">Data no formato yyyy-MM-dd</param>
    /// <returns>Consolidado diário</returns>
    [HttpGet("{data}")]
    [ProducesResponseType(typeof(ConsolidadoDiarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConsolidadoDiarioResponse>> GetConsolidadoByData(string data)
    {
        try
        {
            if (!DateOnly.TryParseExact(data, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
            {
                return BadRequest("Data deve estar no formato yyyy-MM-dd");
            }

            var consolidado = await _consolidationService.GetConsolidadoByDataAsync(date);
            
            if (consolidado == null)
            {
                return NotFound();
            }

            return Ok(consolidado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar consolidado para data {Data}", data);
            return StatusCode(500, "Erro interno ao processar solicitação");
        }
    }

    /// <summary>
    /// Obtém consolidados em um período de datas
    /// </summary>
    /// <param name="startDate">Data inicial no formato yyyy-MM-dd</param>
    /// <param name="endDate">Data final no formato yyyy-MM-dd</param>
    /// <returns>Lista de consolidados</returns>
    [HttpGet("periodo")]
    [ProducesResponseType(typeof(IEnumerable<ConsolidadoDiarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ConsolidadoDiarioResponse>>> GetConsolidadosByPeriod(
        [FromQuery] string startDate,
        [FromQuery] string endDate)
    {
        try
        {
            if (!DateOnly.TryParseExact(startDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var start))
            {
                return BadRequest("Data inicial deve estar no formato yyyy-MM-dd");
            }

            if (!DateOnly.TryParseExact(endDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var end))
            {
                return BadRequest("Data final deve estar no formato yyyy-MM-dd");
            }

            if (start > end)
            {
                return BadRequest("Data inicial deve ser menor ou igual à data final");
            }

            var consolidados = await _consolidationService.GetConsolidadosByDateRangeAsync(start, end);
            return Ok(consolidados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar consolidados por período");
            return StatusCode(500, "Erro interno ao processar solicitação");
        }
    }
}
