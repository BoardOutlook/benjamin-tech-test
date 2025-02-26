using ExecutivesCompensation.Clients.Model;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace ExecutivesCompensation.Clients.UnitTests;

[TestClass]
public sealed class CachingCompanyInfoClientTests
{
    [TestMethod]
    public async Task GetExchangeCompaniesAsync_WhenExecutedTwice_ShouldHitCache()
    {
        // Arrange.
        var baseClient = new Mock<ICompanyInfoClient>(MockBehavior.Strict);
        var cache = new MemoryCache(new MemoryCacheOptions());

        var stock1 = new StockInfo { Symbol = "symbol1", ExchangeShortName = "ASX", Type = "stock" };
        var stock2 = new StockInfo { Symbol = "symbol2", ExchangeShortName = "ASX", Type = "stock" };
        var stocks = new StockInfo[] { stock1, stock2 };
        baseClient
            .Setup(m => m.GetExchangeCompaniesAsync("ASX", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stocks);

        var sut = new CachingCompanyInfoClient(
            baseClient.Object, cache);

        // Act.
        var result = await sut.GetExchangeCompaniesAsync("ASX", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(stocks);
        baseClient.Verify(x => x.GetExchangeCompaniesAsync("ASX", It.IsAny<CancellationToken>()), Times.Once());

        // Act.
        result = await sut.GetExchangeCompaniesAsync("ASX", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(stocks);
        // Base client should not be called again.
        baseClient.Verify(x => x.GetExchangeCompaniesAsync("ASX", It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod]
    public async Task GetCompanyExecutivesAsync_WhenExecutedTwice_ShouldHitCache()
    {
        // Arrange.
        var baseClient = new Mock<ICompanyInfoClient>(MockBehavior.Strict);
        var cache = new MemoryCache(new MemoryCacheOptions());

        var executive1 = new Executive { Symbol = "BRDL", IndustryTitle = "BOARD SERVICES", NameAndPosition = "Steve Pell CEO", Total = 200000.0 };
        var executive2 = new Executive { Symbol = "BRDL", IndustryTitle = "BOARD SERVICES", NameAndPosition = "Allen Stephens CTO", Total = 150000.0 };
        var executives = new Executive[] { executive1, executive2 };
        baseClient
            .Setup(m => m.GetCompanyExecutivesAsync("BRDL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(executives);

        var sut = new CachingCompanyInfoClient(
            baseClient.Object, cache);

        // Act.
        var result = await sut.GetCompanyExecutivesAsync("BRDL", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(executives);
        baseClient.Verify(x => x.GetCompanyExecutivesAsync("BRDL", It.IsAny<CancellationToken>()), Times.Once());

        // Act.
        result = await sut.GetCompanyExecutivesAsync("BRDL", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(executives);
        // Base client should not be called again.
        baseClient.Verify(x => x.GetCompanyExecutivesAsync("BRDL", It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod]
    public async Task GetIndustryBenchmarkAsync_WhenExecutedTwice_ShouldHitCache()
    {
        // Arrange.
        var baseClient = new Mock<ICompanyInfoClient>(MockBehavior.Strict);
        var cache = new MemoryCache(new MemoryCacheOptions());

        var benchmark = new IndustryBenchmark { IndustryTitle = "BOARD SERVICES", AverageCompensation = 123456.78 };
        baseClient
            .Setup(m => m.GetIndustryBenchmarkAsync("BOARD SERVICES", It.IsAny<CancellationToken>()))
            .ReturnsAsync(benchmark);

        var sut = new CachingCompanyInfoClient(
            baseClient.Object, cache);

        // Act.
        var result = await sut.GetIndustryBenchmarkAsync("BOARD SERVICES", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(benchmark);
        baseClient.Verify(x => x.GetIndustryBenchmarkAsync("BOARD SERVICES", It.IsAny<CancellationToken>()), Times.Once());

        // Act.
        result = await sut.GetIndustryBenchmarkAsync("BOARD SERVICES", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(benchmark);
        // Base client should not be called again.
        baseClient.Verify(x => x.GetIndustryBenchmarkAsync("BOARD SERVICES", It.IsAny<CancellationToken>()), Times.Once());
    }
}
