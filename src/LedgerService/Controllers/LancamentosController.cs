using Microsoft.AspNetCore.Mvc;
using LedgerService.Models;
using LedgerService.Services;

namespace LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LancamentosController : ControllerBase
{
    private readonly ILancamentoService _lancamentoService;
    private readonly ILogger<LancamentosController> _logger;

    public LancamentosController(ILancamentoService lancamentoService, ILogger<LancamentosController> logger)
    {
        _lancamentoService = lancamentoService;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo lançamento financeiro
    /// </summary>
    /// <param name="request">Dados do lançamento</param>
    /// <param name="idempotencyKey">Chave de idempotência para evitar duplicações</param>
    /// <returns>Lançamento criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LancamentoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LancamentoResponse>> CreateLancamento(
        [FromBody] CreateLancamentoRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _lancamentoService.CreateLancamentoAsync(request, idempotencyKey);
            
            _logger.LogInformation("Lançamento criado: {LancamentoId}", result.Id);
            
            return CreatedAtAction(
                nameof(GetLancamentoById), 
                new { id = result.Id }, 
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar lançamento");
            return StatusCode(500, "Erro interno ao processar solicitação");
        }
    }

    /// <summary>
    /// Obtém um lançamento por ID
    /// </summary>
    /// <param name="id">ID do lançamento</param>
    /// <returns>Lançamento encontrado</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LancamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LancamentoResponse>> GetLancamentoById(Guid id)
    {
        try
        {
            var lancamento = await _lancamentoService.GetLancamentoByIdAsync(id);
            
            if (lancamento == null)
            {
                return NotFound();
            }

            return Ok(lancamento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar lançamento {LancamentoId}", id);
            return StatusCode(500, "Erro interno ao processar solicitação");
        }
    }

    /// <summary>
    /// Lista todos os lançamentos
    /// </summary>
    /// <returns>Lista de lançamentos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LancamentoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LancamentoResponse>>> GetAllLancamentos()
    {
        try
        {
            var lancamentos = await _lancamentoService.GetAllLancamentosAsync();
            return Ok(lancamentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar lançamentos");
            return StatusCode(500, "Erro interno ao processar solicitação");
        }
    }

    /// <summary>
    /// Lista lançamentos por período
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <returns>Lista de lançamentos no período</returns>
    [HttpGet("periodo")]
    [ProducesResponseType(typeof(IEnumerable<LancamentoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LancamentoResponse>>> GetLancamentosByPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var lancamentos = await _lancamentoService.GetLancamentosByDateRangeAsync(startDate, endDate);
            return Ok(lancamentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar lançamentos por período");
            return StatusCode(500, "Erro interno ao processar solicitação");
        }
    }

    /// <summary>
    /// Lista lançamentos por tipo
    /// </summary>
    /// <param name="tipo">Tipo do lançamento (debito ou credito)</param>
    /// <returns>Lista de lançamentos do tipo</returns>
    [HttpGet("tipo/{tipo}")]
    [ProducesResponseType(typeof(IEnumerable<LancamentoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<LancamentoResponse>>> GetLancamentosByTipo(string tipo)
    {
        try
        {
            if (tipo != "debito" && tipo != "credito")
            {
                return BadRequest("Tipo deve ser 'debito' ou 'credito'");
            }

            var lancamentos = await _lancamentoService.GetLancamentosByTipoAsync(tipo);
            return Ok(lancamentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar lançamentos por tipo");
            return StatusCode(500, "Erro interno ao processar solicitação");
        }
    }
}
