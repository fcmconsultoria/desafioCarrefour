using ConsolidationService.Models;
using ConsolidationService.Repositories;
using ConsolidationService.Services;
using Moq;
using FluentAssertions;
using Xunit;

namespace ConsolidationService.Tests.Services;

public class ConsolidationServiceTests
{
    private readonly Mock<IConsolidadoRepository> _mockRepo;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<ILogger<ConsolidationService>> _mockLogger;
    private readonly ConsolidationService _service;

    public ConsolidationServiceTests()
    {
        _mockRepo = new Mock<IConsolidadoRepository>();
        _mockCache = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<ConsolidationService>>();
        _service = new ConsolidationService(_mockRepo.Object, _mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetConsolidadoByDataAsync_WhenCacheHit_ShouldReturnCachedValue()
    {
        // Arrange
        var data = DateOnly.FromDateTime(DateTime.UtcNow);
        var cachedResponse = new ConsolidadoDiarioResponse
        {
            Data = data,
            TotalCreditos = 1000m,
            TotalDebitos = 500m,
            SaldoFinal = 500m,
            QuantidadeLancamentos = 5
        };

        _mockCache.Setup(c => c.GetAsync<ConsolidadoDiarioResponse>($"consolidado:{data:yyyy-MM-dd}"))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _service.GetConsolidadoByDataAsync(data);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(cachedResponse);
        _mockRepo.Verify(r => r.GetByDataAsync(It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task GetConsolidadoByDataAsync_WhenCacheMiss_ShouldQueryDatabaseAndCache()
    {
        // Arrange
        var data = DateOnly.FromDateTime(DateTime.UtcNow);
        var dbConsolidado = new ConsolidadoDiario
        {
            Data = data,
            TotalCreditos = 1000m,
            TotalDebitos = 500m,
            SaldoFinal = 500m,
            QuantidadeLancamentos = 5,
            UpdatedAt = DateTime.UtcNow
        };

        _mockCache.Setup(c => c.GetAsync<ConsolidadoDiarioResponse>($"consolidado:{data:yyyy-MM-dd}"))
            .ReturnsAsync((ConsolidadoDiarioResponse?)null);
        
        _mockRepo.Setup(r => r.GetByDataAsync(data))
            .ReturnsAsync(dbConsolidado);

        // Act
        var result = await _service.GetConsolidadoByDataAsync(data);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().Be(data);
        result.SaldoFinal.Should().Be(500m);
        
        _mockRepo.Verify(r => r.GetByDataAsync(data), Times.Once);
        _mockCache.Verify(c => c.SetAsync(
            $"consolidado:{data:yyyy-MM-dd}", 
            It.IsAny<ConsolidadoDiarioResponse>(), 
            TimeSpan.FromMinutes(5)), Times.Once);
    }

    [Fact]
    public async Task GetConsolidadoByDataAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var data = DateOnly.FromDateTime(DateTime.UtcNow);
        
        _mockCache.Setup(c => c.GetAsync<ConsolidadoDiarioResponse>($"consolidado:{data:yyyy-MM-dd}"))
            .ReturnsAsync((ConsolidadoDiarioResponse?)null);
        
        _mockRepo.Setup(r => r.GetByDataAsync(data))
            .ReturnsAsync((ConsolidadoDiario?)null);

        // Act
        var result = await _service.GetConsolidadoByDataAsync(data);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ProcessLancamentoAsync_Credito_ShouldIncreaseTotalCreditos()
    {
        // Arrange
        var lancamentoId = Guid.NewGuid();
        var valor = 100m;
        var tipo = "credito";
        var dataHora = DateTime.UtcNow;
        var data = DateOnly.FromDateTime(dataHora);

        var existingConsolidado = new ConsolidadoDiario
        {
            Data = data,
            TotalCreditos = 500m,
            TotalDebitos = 200m,
            SaldoFinal = 300m,
            QuantidadeLancamentos = 3
        };

        _mockRepo.Setup(r => r.GetByDataAsync(data))
            .ReturnsAsync(existingConsolidado);
        
        _mockRepo.Setup(r => r.CreateOrUpdateAsync(It.IsAny<ConsolidadoDiario>()))
            .ReturnsAsync((ConsolidadoDiario c) => c);

        // Act
        await _service.ProcessLancamentoAsync(lancamentoId, valor, tipo, dataHora);

        // Assert
        _mockRepo.Verify(r => r.CreateOrUpdateAsync(It.Is<ConsolidadoDiario>(c => 
            c.TotalCreditos == 600m && // 500 + 100
            c.TotalDebitos == 200m &&
            c.SaldoFinal == 400m && // 600 - 200
            c.QuantidadeLancamentos == 4
        )), Times.Once);

        _mockCache.Verify(c => c.RemoveAsync($"consolidado:{data:yyyy-MM-dd}"), Times.Once);
    }

    [Fact]
    public async Task ProcessLancamentoAsync_Debito_ShouldIncreaseTotalDebitos()
    {
        // Arrange
        var lancamentoId = Guid.NewGuid();
        var valor = 50m;
        var tipo = "debito";
        var dataHora = DateTime.UtcNow;
        var data = DateOnly.FromDateTime(dataHora);

        var existingConsolidado = new ConsolidadoDiario
        {
            Data = data,
            TotalCreditos = 500m,
            TotalDebitos = 200m,
            SaldoFinal = 300m,
            QuantidadeLancamentos = 3
        };

        _mockRepo.Setup(r => r.GetByDataAsync(data))
            .ReturnsAsync(existingConsolidado);
        
        _mockRepo.Setup(r => r.CreateOrUpdateAsync(It.IsAny<ConsolidadoDiario>()))
            .ReturnsAsync((ConsolidadoDiario c) => c);

        // Act
        await _service.ProcessLancamentoAsync(lancamentoId, valor, tipo, dataHora);

        // Assert
        _mockRepo.Verify(r => r.CreateOrUpdateAsync(It.Is<ConsolidadoDiario>(c => 
            c.TotalCreditos == 500m &&
            c.TotalDebitos == 250m && // 200 + 50
            c.SaldoFinal == 250m && // 500 - 250
            c.QuantidadeLancamentos == 4
        )), Times.Once);
    }

    [Fact]
    public async Task ProcessLancamentoAsync_NewDate_ShouldCreateNewConsolidado()
    {
        // Arrange
        var lancamentoId = Guid.NewGuid();
        var valor = 100m;
        var tipo = "credito";
        var dataHora = DateTime.UtcNow;
        var data = DateOnly.FromDateTime(dataHora);

        _mockRepo.Setup(r => r.GetByDataAsync(data))
            .ReturnsAsync((ConsolidadoDiario?)null);
        
        _mockRepo.Setup(r => r.CreateOrUpdateAsync(It.IsAny<ConsolidadoDiario>()))
            .ReturnsAsync((ConsolidadoDiario c) => c);

        // Act
        await _service.ProcessLancamentoAsync(lancamentoId, valor, tipo, dataHora);

        // Assert
        _mockRepo.Verify(r => r.CreateOrUpdateAsync(It.Is<ConsolidadoDiario>(c => 
            c.Data == data &&
            c.TotalCreditos == 100m &&
            c.TotalDebitos == 0m &&
            c.SaldoFinal == 100m &&
            c.QuantidadeLancamentos == 1
        )), Times.Once);
    }

    [Fact]
    public async Task GetConsolidadosByDateRangeAsync_ShouldReturnConsolidadosInRange()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var expectedConsolidados = new List<ConsolidadoDiario>
        {
            new() { Data = startDate, TotalCreditos = 1000m, TotalDebitos = 500m, SaldoFinal = 500m, QuantidadeLancamentos = 5 },
            new() { Data = endDate, TotalCreditos = 2000m, TotalDebitos = 1000m, SaldoFinal = 1000m, QuantidadeLancamentos = 10 }
        };

        _mockRepo.Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(expectedConsolidados);

        // Act
        var result = await _service.GetConsolidadosByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
        result.First().Data.Should().Be(startDate);
        result.Last().Data.Should().Be(endDate);
    }
}
