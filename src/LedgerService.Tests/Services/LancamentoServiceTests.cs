using LedgerService.Models;
using LedgerService.Repositories;
using LedgerService.Services;
using Moq;
using FluentAssertions;
using Xunit;

namespace LedgerService.Tests.Services;

public class LancamentoServiceTests
{
    private readonly Mock<ILancamentoRepository> _mockRepo;
    private readonly Mock<IIdempotencyRepository> _mockIdempotencyRepo;
    private readonly Mock<ILancamentoEventPublisher> _mockPublisher;
    private readonly LancamentoService _service;

    public LancamentoServiceTests()
    {
        _mockRepo = new Mock<ILancamentoRepository>();
        _mockIdempotencyRepo = new Mock<IIdempotencyRepository>();
        _mockPublisher = new Mock<ILancamentoEventPublisher>();
        _service = new LancamentoService(_mockRepo.Object, _mockIdempotencyRepo.Object, _mockPublisher.Object);
    }

    [Fact]
    public async Task CreateLancamentoAsync_WithoutIdempotencyKey_ShouldCreateLancamento()
    {
        // Arrange
        var request = new CreateLancamentoRequest
        {
            Valor = 100.50m,
            Tipo = "credito",
            Descricao = "Teste"
        };

        var expectedLancamento = new Lancamento
        {
            Id = Guid.NewGuid(),
            Valor = request.Valor,
            Tipo = request.Tipo,
            Descricao = request.Descricao,
            DataHora = DateTime.UtcNow
        };

        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Lancamento>()))
            .ReturnsAsync(expectedLancamento);

        // Act
        var result = await _service.CreateLancamentoAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Valor.Should().Be(request.Valor);
        result.Tipo.Should().Be(request.Tipo);
        result.Descricao.Should().Be(request.Descricao);
        
        _mockRepo.Verify(r => r.CreateAsync(It.IsAny<Lancamento>()), Times.Once);
        _mockPublisher.Verify(p => p.PublishLancamentoCriadoAsync(
            It.IsAny<Guid>(), 
            It.IsAny<decimal>(), 
            It.IsAny<string>(), 
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task CreateLancamentoAsync_WithExistingIdempotencyKey_ShouldReturnCachedResponse()
    {
        // Arrange
        var request = new CreateLancamentoRequest
        {
            Valor = 100.50m,
            Tipo = "credito"
        };

        var idempotencyKey = "test-key-123";
        var cachedResponse = new LancamentoResponse
        {
            Id = Guid.NewGuid(),
            Valor = 50.00m,
            Tipo = "credito"
        };

        var cachedKey = new IdempotencyKey
        {
            Key = idempotencyKey,
            LancamentoId = Guid.NewGuid()
        };
        cachedKey.SetResponse(cachedResponse);

        _mockIdempotencyRepo.Setup(r => r.GetByKeyAsync(idempotencyKey))
            .ReturnsAsync(cachedKey);

        // Act
        var result = await _service.CreateLancamentoAsync(request, idempotencyKey);

        // Assert
        result.Should().NotBeNull();
        result.Valor.Should().Be(cachedResponse.Valor);
        result.Id.Should().Be(cachedResponse.Id);
        
        _mockRepo.Verify(r => r.CreateAsync(It.IsAny<Lancamento>()), Times.Never);
        _mockPublisher.Verify(p => p.PublishLancamentoCriadoAsync(
            It.IsAny<Guid>(), 
            It.IsAny<decimal>(), 
            It.IsAny<string>(), 
            It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task CreateLancamentoAsync_WithNewIdempotencyKey_ShouldCacheResponse()
    {
        // Arrange
        var request = new CreateLancamentoRequest
        {
            Valor = 100.50m,
            Tipo = "debito"
        };

        var idempotencyKey = "new-key-456";
        var expectedLancamento = new Lancamento
        {
            Id = Guid.NewGuid(),
            Valor = request.Valor,
            Tipo = request.Tipo
        };

        _mockIdempotencyRepo.Setup(r => r.GetByKeyAsync(idempotencyKey))
            .ReturnsAsync((IdempotencyKey?)null);
        
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Lancamento>()))
            .ReturnsAsync(expectedLancamento);

        // Act
        var result = await _service.CreateLancamentoAsync(request, idempotencyKey);

        // Assert
        result.Should().NotBeNull();
        
        _mockIdempotencyRepo.Verify(r => r.CreateAsync(It.Is<IdempotencyKey>(k => k.Key == idempotencyKey)), Times.Once);
    }

    [Fact]
    public async Task GetLancamentoByIdAsync_WhenExists_ShouldReturnLancamento()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedLancamento = new Lancamento
        {
            Id = id,
            Valor = 100m,
            Tipo = "credito"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(expectedLancamento);

        // Act
        var result = await _service.GetLancamentoByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Valor.Should().Be(expectedLancamento.Valor);
    }

    [Fact]
    public async Task GetLancamentoByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Lancamento?)null);

        // Act
        var result = await _service.GetLancamentoByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllLancamentosAsync_ShouldReturnAllLancamentos()
    {
        // Arrange
        var expectedLancamentos = new List<Lancamento>
        {
            new() { Id = Guid.NewGuid(), Valor = 100m, Tipo = "credito" },
            new() { Id = Guid.NewGuid(), Valor = 50m, Tipo = "debito" }
        };

        _mockRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(expectedLancamentos);

        // Act
        var result = await _service.GetAllLancamentosAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLancamentosByTipoAsync_ShouldFilterByTipo()
    {
        // Arrange
        var tipo = "credito";
        var expectedLancamentos = new List<Lancamento>
        {
            new() { Id = Guid.NewGuid(), Valor = 100m, Tipo = tipo }
        };

        _mockRepo.Setup(r => r.GetByTipoAsync(tipo))
            .ReturnsAsync(expectedLancamentos);

        // Act
        var result = await _service.GetLancamentosByTipoAsync(tipo);

        // Assert
        result.Should().HaveCount(1);
        result.First().Tipo.Should().Be(tipo);
    }
}
