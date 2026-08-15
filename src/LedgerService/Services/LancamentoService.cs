using LedgerService.Messaging;
using LedgerService.Models;
using LedgerService.Repositories;

namespace LedgerService.Services;

public class LancamentoService : ILancamentoService
{
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ILancamentoEventPublisher _eventPublisher;

    public LancamentoService(
        ILancamentoRepository lancamentoRepository,
        IIdempotencyRepository idempotencyRepository,
        ILancamentoEventPublisher eventPublisher)
    {
        _lancamentoRepository = lancamentoRepository;
        _idempotencyRepository = idempotencyRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<LancamentoResponse> CreateLancamentoAsync(CreateLancamentoRequest request, string? idempotencyKey = null)
    {
        // Verificar idempotency
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existingKey = await _idempotencyRepository.GetByKeyAsync(idempotencyKey);
            if (existingKey != null)
            {
                var cachedResponse = existingKey.GetResponse<LancamentoResponse>();
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
            }
        }

        // Criar lançamento
        var lancamento = new Lancamento
        {
            Id = Guid.NewGuid(),
            Valor = request.Valor,
            Tipo = request.Tipo,
            Descricao = request.Descricao,
            DataHora = DateTime.UtcNow
        };

        var createdLancamento = await _lancamentoRepository.CreateAsync(lancamento);

        // Publicar evento (fire and forget - não bloqueia a resposta)
        _ = Task.Run(async () =>
        {
            try
            {
                await _eventPublisher.PublishLancamentoCriadoAsync(
                    createdLancamento.Id,
                    createdLancamento.Valor,
                    createdLancamento.Tipo,
                    createdLancamento.DataHora
                );
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                Console.WriteLine($"Error publishing event: {ex.Message}");
            }
        });

        // Cache response se idempotency key foi fornecido
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var response = MapToResponse(createdLancamento);
            
            var idempotencyKeyEntity = new IdempotencyKey
            {
                Key = idempotencyKey,
                LancamentoId = createdLancamento.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(24) // 24 horas TTL
            };
            idempotencyKeyEntity.SetResponse(response);
            
            await _idempotencyRepository.CreateAsync(idempotencyKeyEntity);
            
            return response;
        }

        return MapToResponse(createdLancamento);
    }

    public async Task<LancamentoResponse?> GetLancamentoByIdAsync(Guid id)
    {
        var lancamento = await _lancamentoRepository.GetByIdAsync(id);
        return lancamento != null ? MapToResponse(lancamento) : null;
    }

    public async Task<IEnumerable<LancamentoResponse>> GetAllLancamentosAsync()
    {
        var lancamentos = await _lancamentoRepository.GetAllAsync();
        return lancamentos.Select(MapToResponse);
    }

    public async Task<IEnumerable<LancamentoResponse>> GetLancamentosByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var lancamentos = await _lancamentoRepository.GetByDateRangeAsync(startDate, endDate);
        return lancamentos.Select(MapToResponse);
    }

    public async Task<IEnumerable<LancamentoResponse>> GetLancamentosByTipoAsync(string tipo)
    {
        var lancamentos = await _lancamentoRepository.GetByTipoAsync(tipo);
        return lancamentos.Select(MapToResponse);
    }

    private static LancamentoResponse MapToResponse(Lancamento lancamento)
    {
        return new LancamentoResponse
        {
            Id = lancamento.Id,
            Valor = lancamento.Valor,
            Tipo = lancamento.Tipo,
            Descricao = lancamento.Descricao,
            DataHora = lancamento.DataHora,
            CreatedAt = lancamento.CreatedAt
        };
    }
}
