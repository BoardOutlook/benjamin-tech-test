using ExecutivesCompensation.Clients;
using ExecutivesCompensation.Clients.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExecutivesCompensation.Controllers.V1.UnitTests;

[TestClass]
public sealed class ExecutivesCompensationControllerTests
{
    void SetupGetExchangeCompaniesAsync(Mock<ICompanyInfoClient> client, string exchangeSymbol, StockInfo[] stocks)
    {
        client
            .Setup(m => m.GetExchangeCompaniesAsync(exchangeSymbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stocks);
    }

    void SetupGetCompanyExecutivesAsync(Mock<ICompanyInfoClient> client, string companySymbol, Executive[] executives)
    {
        client
            .Setup(m => m.GetCompanyExecutivesAsync(companySymbol, It.IsAny<CancellationToken>()))
            .ReturnsAsync(executives);
    }

    void SetupGetIndustryBenchmarkAsync(Mock<ICompanyInfoClient> client, string industryTitle, IndustryBenchmark? benchmark)
    {
        client
            .Setup(m => m.GetIndustryBenchmarkAsync(industryTitle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(benchmark);
    }

    [TestMethod]
    public async Task GetAsync_ShouldWork()
    {
        // Arrange.
        var client = new Mock<ICompanyInfoClient>(MockBehavior.Strict);
        var logger = new Mock<ILogger<ExecutivesCompensationController>>(MockBehavior.Strict);

        var stock1 = new StockInfo { Symbol = "BRDL", ExchangeShortName = "ASX", Type = "stock" };
        var stock2 = new StockInfo { Symbol = "GOOG", ExchangeShortName = "ASX", Type = "stock" };
        var stock3 = new StockInfo { Symbol = "MSFT", ExchangeShortName = "ASX", Type = "stock" };
        // Empty symbol. Should be ignored.
        var stock4 = new StockInfo { Symbol = "", ExchangeShortName = "ASX", Type = "stock" };  // Empty symbols are ignored.
        var stocks = new StockInfo[] { stock1, stock2, stock3, stock4 };
        SetupGetExchangeCompaniesAsync(client, "ASX", stocks);

        // Salary just above 110% of average.
        var brdlExecutive1 = new Executive { Symbol = "BRDL", IndustryTitle = "BOARD SERVICES", NameAndPosition = "Steve Pell CEO", Total = 110001.0 };
        // Salary 105% of average.
        var brdlExecutive2 = new Executive { Symbol = "BRDL", IndustryTitle = "BOARD SERVICES", NameAndPosition = "Allen Stephens CTO", Total = 105000.0 };
        // Intentional typo in the industry title.
        var brdlExecutive3 = new Executive { Symbol = "BRDL", IndustryTitle = "TYPO SERVICES", NameAndPosition = "Sarah Graff VP", Total = 150000.0 };
        var brdlExecutives = new Executive[] { brdlExecutive1, brdlExecutive2, brdlExecutive3 };
        // For some reason Steve Pell is also CEO of GOOG and in a different industry. Salary above 110% of average.
        var googExecutive1 = new Executive { Symbol = "GOOG", IndustryTitle = "WEB API DEVELOPMENT", NameAndPosition = "Steve Pell CEO", Total = 300000.0 };
        // Salary above 110% of average.
        var googExecutive2 = new Executive { Symbol = "GOOG", IndustryTitle = "INTERNET SERVICES", NameAndPosition = "Sundar Pichai CEO", Total = 200000.0 };
        // Empty industry title. Should be ignored.
        var googExecutive3 = new Executive { Symbol = "GOOG", IndustryTitle = "", NameAndPosition = "Scott Silver YouTube VP", Total = 200000.0 };
        var googExecutives = new Executive[] { googExecutive1, googExecutive2, googExecutive3 };
        SetupGetCompanyExecutivesAsync(client, "BRDL", brdlExecutives);
        SetupGetCompanyExecutivesAsync(client, "GOOG", googExecutives);
        // For some reason MSFT has no executives.
        SetupGetCompanyExecutivesAsync(client, "MSFT", []);

        SetupGetIndustryBenchmarkAsync(client, "BOARD SERVICES", new IndustryBenchmark { IndustryTitle = "BOARD SERVICES", AverageCompensation = 100000.0 });
        // Industry with no benchmark.
        SetupGetIndustryBenchmarkAsync(client, "TYPO SERVICES", null);
        SetupGetIndustryBenchmarkAsync(client, "INTERNET SERVICES", new IndustryBenchmark { IndustryTitle = "INTERNET SERVICES", AverageCompensation = 150000.0 });
        SetupGetIndustryBenchmarkAsync(client, "WEB API DEVELOPMENT", new IndustryBenchmark { IndustryTitle = "WEB API DEVELOPMENT", AverageCompensation = 42.0 });
        var sut = new ExecutivesCompensationController(client.Object, logger.Object);

        // Act.
        var result = await sut.GetAsync(CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult!.Value.Should().BeEquivalentTo(new ExecutiveCompensation[] {
            // As CEO of BRDL.
            new() { NameAndPosition = "Steve Pell CEO", Compensation = 110001.0, AverageIndustryCompensation = 100000.0 },
            // As CEO of GOOG.
            new() { NameAndPosition = "Steve Pell CEO", Compensation = 300000.0, AverageIndustryCompensation = 42.0 },
            new() { NameAndPosition = "Sundar Pichai CEO", Compensation = 200000.0, AverageIndustryCompensation = 150000.0 },
        });
        // Client methods should only be called once.
        client.Verify(x => x.GetExchangeCompaniesAsync("ASX", It.IsAny<CancellationToken>()), Times.Once());
        client.Verify(x => x.GetCompanyExecutivesAsync("BRDL", It.IsAny<CancellationToken>()), Times.Once());
        client.Verify(x => x.GetCompanyExecutivesAsync("GOOG", It.IsAny<CancellationToken>()), Times.Once());
        client.Verify(x => x.GetCompanyExecutivesAsync("MSFT", It.IsAny<CancellationToken>()), Times.Once());
        client.Verify(x => x.GetIndustryBenchmarkAsync("BOARD SERVICES", It.IsAny<CancellationToken>()), Times.Once());
        client.Verify(x => x.GetIndustryBenchmarkAsync("TYPO SERVICES", It.IsAny<CancellationToken>()), Times.Once());
        client.Verify(x => x.GetIndustryBenchmarkAsync("INTERNET SERVICES", It.IsAny<CancellationToken>()), Times.Once());
        client.Verify(x => x.GetIndustryBenchmarkAsync("WEB API DEVELOPMENT", It.IsAny<CancellationToken>()), Times.Once());
    }

     [TestMethod]
    public async Task GetAsync_WhenNoCompanies_ShouldReturnInternalServerError()
    {
        // Arrange.
        var client = new Mock<ICompanyInfoClient>(MockBehavior.Strict);
        var logger = new Mock<ILogger<ExecutivesCompensationController>>(MockBehavior.Strict);

        var stocks = new StockInfo[] {};
        SetupGetExchangeCompaniesAsync(client, "ASX", stocks);

        var sut = new ExecutivesCompensationController(client.Object, logger.Object);

        // Act.
        var result = await sut.GetAsync(CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        var problemResult = result.Result as ObjectResult;
        problemResult.Should().NotBeNull();
        problemResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problemDetails = problemResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Contain("Found 0 stocks on the ASX");
    }
}
