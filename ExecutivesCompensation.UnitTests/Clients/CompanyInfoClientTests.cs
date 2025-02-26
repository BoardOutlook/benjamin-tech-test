using System.Net;
using ExecutivesCompensation.Clients.Model;
using FluentAssertions;
using Moq;
using Moq.Contrib.HttpClient;

namespace ExecutivesCompensation.Clients.UnitTests;

[TestClass]
public sealed class CompanyInfoClientTests
{
    private const string BaseUrl = "https://fake-url.com";

    #region GetExchangeCompaniesAsync

    [TestMethod]
    public async Task GetExchangeCompaniesAsync_WhenBackendSuccess_ShouldReturnStocks()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var stock1 = new StockInfo { Symbol = "symbol1", ExchangeShortName = "ASX", Type = "stock" };
        var stock2 = new StockInfo { Symbol = "symbol2", ExchangeShortName = "ASX", Type = "stock" };
        var stocks = new StockInfo[] { stock1, stock2 };
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/exchanges/ASX/companies?code=fakeApiKey")
            .ReturnsJsonResponse(stocks);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        var result = await sut.GetExchangeCompaniesAsync("ASX", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(stocks);
    }

    [TestMethod]
    public async Task GetExchangeCompaniesAsync_WhenBackendEmpty_ShouldReturnEmpty()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var stocks = new StockInfo[] { };
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/exchanges/ASX/companies?code=fakeApiKey")
            .ReturnsJsonResponse(stocks);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        var result = await sut.GetExchangeCompaniesAsync("ASX", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetExchangeCompaniesAsync_WhenExchangeSymbolMismatch_ShouldThrow()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var stock1 = new StockInfo { Symbol = "symbol1", ExchangeShortName = "ASX", Type = "stock" };
        var stock2 = new StockInfo { Symbol = "symbol2", ExchangeShortName = "NYSE", Type = "stock" };
        var stocks = new StockInfo[] { stock1, stock2 };
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/exchanges/ASX/companies?code=fakeApiKey")
            .ReturnsJsonResponse(stocks);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        try
        {
            var result = await sut.GetExchangeCompaniesAsync("ASX", CancellationToken.None);
            Assert.Fail();
        }
        catch (Exception e)
        {
            // Assert.
            e.Should().BeOfType(typeof(CompanyInfoClientException));
            e.Message.Should().Contain("Unexpected stock exchange symbol: NYSE");
        }
    }

    [TestMethod]
    public async Task GetExchangeCompaniesAsync_WhenStockInfoTypeMismatch_ShouldThrow()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var stock1 = new StockInfo { Symbol = "symbol1", ExchangeShortName = "ASX", Type = "stock" };
        var stock2 = new StockInfo { Symbol = "symbol2", ExchangeShortName = "ASX", Type = "mutual_fund" };
        var stocks = new StockInfo[] { stock1, stock2 };
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/exchanges/ASX/companies?code=fakeApiKey")
            .ReturnsJsonResponse(stocks);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        try
        {
            var result = await sut.GetExchangeCompaniesAsync("ASX", CancellationToken.None);
            Assert.Fail();
        }
        catch (Exception e)
        {
            // Assert.
            e.Should().BeOfType(typeof(CompanyInfoClientException));
            e.Message.Should().Contain("Unexpected stock type: mutual_fund");
        }
    }

    [TestMethod]
    public async Task GetExchangeCompaniesAsync_WhenBackendFailure_ShouldThrow()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/exchanges/ASX/companies?code=fakeApiKey")
            .ReturnsResponse(HttpStatusCode.Forbidden);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        try
        {
            var result = await sut.GetExchangeCompaniesAsync("ASX", CancellationToken.None);
            Assert.Fail();
        }
        catch (Exception e)
        {
            // Assert.
            e.Should().BeOfType(typeof(CompanyInfoClientException));
            e.Message.Should().Contain("returned failure HTTP status Forbidden");
        }
    }

    #endregion

    #region GetCompanyExecutivesAsync

    [TestMethod]
    public async Task GetCompanyExecutivesAsync_WhenBackendSuccess_ShouldReturnStocks()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var executive1 = new Executive { Symbol = "BRDL", IndustryTitle = "BOARD SERVICES", NameAndPosition = "Steve Pell CEO", Total = 200000.0 };
        var executive2 = new Executive { Symbol = "BRDL", IndustryTitle = "BOARD SERVICES", NameAndPosition = "Allen Stephens CTO", Total = 150000.0 };
        var executives = new Executive[] { executive1, executive2 };
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/companies/BRDL/executives?code=fakeApiKey")
            .ReturnsJsonResponse(executives);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        var result = await sut.GetCompanyExecutivesAsync("BRDL", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(executives);
    }

    [TestMethod]
    public async Task GetCompanyExecutivesAsync_WhenBackendEmpty_ShouldReturnEmpty()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var executives = new Executive[] { };
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
             .SetupRequest(HttpMethod.Get, BaseUrl + "/api/companies/BRDL/executives?code=fakeApiKey")
            .ReturnsJsonResponse(executives);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        var result = await sut.GetCompanyExecutivesAsync("BRDL", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetCompanyExecutivesAsync_WhenCompanySymbolMismatch_ShouldThrow()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var executive1 = new Executive { Symbol = "BRDL", IndustryTitle = "BOARD SERVICES", NameAndPosition = "Steve Pell CEO", Total = 200000.0 };
        var executive2 = new Executive { Symbol = "GOOG", IndustryTitle = "INTERNET SERVICES", NameAndPosition = "Sundar Pichai CEO", Total = 150000.0 };
        var executives = new Executive[] { executive1, executive2 };
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/companies/BRDL/executives?code=fakeApiKey")
            .ReturnsJsonResponse(executives);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        try
        {
            var result = await sut.GetCompanyExecutivesAsync("BRDL", CancellationToken.None);
            Assert.Fail();
        }
        catch (Exception e)
        {
            // Assert.
            e.Should().BeOfType(typeof(CompanyInfoClientException));
            e.Message.Should().Contain("Unexpected executive company symbol: GOOG");
        }
    }

    [TestMethod]
    public async Task GetCompanyExecutivesAsync_WhenBackendFailure_ShouldThrow()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/companies/BRDL/executives?code=fakeApiKey")
            .ReturnsResponse(HttpStatusCode.Unauthorized);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        try
        {
            var result = await sut.GetCompanyExecutivesAsync("BRDL", CancellationToken.None);
            Assert.Fail();
        }
        catch (Exception e)
        {
            // Assert.
            e.Should().BeOfType(typeof(CompanyInfoClientException));
            e.Message.Should().Contain("returned failure HTTP status Unauthorized");
        }
    }

    #endregion

    #region GetIndustryBenchmarkAsync

    [TestMethod]
    public async Task GetIndustryBenchmarkAsync_WhenBackendSuccess_ShouldReturnBenchmark()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var benchmark = new IndustryBenchmark { IndustryTitle = "BOARD SERVICES", AverageCompensation = 123456.78 };
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/industries/BOARD SERVICES/benchmark?code=fakeApiKey")
            .ReturnsJsonResponse(benchmark);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        var result = await sut.GetIndustryBenchmarkAsync("BOARD SERVICES", CancellationToken.None);

        // Assert.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(benchmark);
    }

    [TestMethod]
    public async Task GetIndustryBenchmarkAsync_WhenIndustryTitleMismatch_ShouldThrow()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var benchmark = new IndustryBenchmark { IndustryTitle = "INTERNET SERVICES", AverageCompensation = 123456.78 };
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/industries/BOARD SERVICES/benchmark?code=fakeApiKey")
            .ReturnsJsonResponse(benchmark);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        try
        {
            var result = await sut.GetIndustryBenchmarkAsync("BOARD SERVICES", CancellationToken.None);
            Assert.Fail();
        }
        catch (Exception e)
        {
            // Assert.
            e.Should().BeOfType(typeof(CompanyInfoClientException));
            e.Message.Should().Contain("Unexpected benchmark industry title: INTERNET SERVICES");
        }
    }

    [TestMethod]
    public async Task GetIndustryBenchmarkAsync_WhenBackendFailure_ShouldThrow()
    {
        // Arrange.
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        handler
            .SetupRequest(HttpMethod.Get, BaseUrl + "/api/industries/BOARD SERVICES/benchmark?code=fakeApiKey")
            .ReturnsResponse(HttpStatusCode.InternalServerError);

        HttpClient httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var sut = new CompanyInfoClient(
            httpClient,
            "fakeApiKey");

        // Act.
        try
        {
            var result = await sut.GetIndustryBenchmarkAsync("BOARD SERVICES", CancellationToken.None);
            Assert.Fail();
        }
        catch (Exception e)
        {
            // Assert.
            e.Should().BeOfType(typeof(CompanyInfoClientException));
            e.Message.Should().Contain("returned failure HTTP status InternalServerError");
        }
    }

    #endregion
}
