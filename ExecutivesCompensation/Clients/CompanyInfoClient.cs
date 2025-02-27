using System.Net;
using System.Text.Json;
using ExecutivesCompensation.Clients.Model;

namespace ExecutivesCompensation.Clients;

// Simple exception wrapper for CompanyInfoClient.
[Serializable]
public class CompanyInfoClientException : Exception
{
    public CompanyInfoClientException(string message) : base(message)
    {
    }
}

public class CompanyInfoClient : ICompanyInfoClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public CompanyInfoClient(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<IList<StockInfo>> GetExchangeCompaniesAsync(string exchangeSymbol, CancellationToken cancellationToken)
    {
        string url = $"api/exchanges/{exchangeSymbol}/companies?code={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new CompanyInfoClientException("CompanyInfo service returned failure HTTP status " + response.StatusCode + " for exchange " + exchangeSymbol);
        }

        using Stream contentStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        IEnumerable<StockInfo>? stocks = await JsonSerializer.DeserializeAsync
            <IEnumerable<StockInfo>>(contentStream, cancellationToken: cancellationToken);
        if (stocks == null)
        {
            throw new JsonException("Unexpected null IEnumerable<StockInfo> from JsonSerializer");
        }
        List<StockInfo> stocksList = stocks.ToList();
        foreach (var stock in stocksList)
        {
            if (stock.Type != "stock")
            {
                throw new CompanyInfoClientException("Unexpected stock type: " + stock.Type);
            }
            if (stock.ExchangeShortName != exchangeSymbol)
            {
                throw new CompanyInfoClientException("Unexpected stock exchange symbol: " + stock.ExchangeShortName);
            }
        }
        return stocksList;
    }

    public async Task<IList<Executive>> GetCompanyExecutivesAsync(string companySymbol, CancellationToken cancellationToken)
    {
        string url = $"api/companies/{companySymbol}/executives?code={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new CompanyInfoClientException("CompanyInfo service returned failure HTTP status " + response.StatusCode + " for company " + companySymbol);
        }

        using Stream? contentStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        IEnumerable<Executive>? executives = await JsonSerializer.DeserializeAsync
            <IEnumerable<Executive>>(contentStream, cancellationToken: cancellationToken);
        if (executives == null)
        {
            throw new JsonException("Unexpected null IEnumerable<Executive> from JsonSerializer");
        }
        List<Executive> executivesList = executives.ToList();
        foreach (var executive in executivesList)
        {
            if (executive.Symbol != companySymbol)
            {
                throw new CompanyInfoClientException("Unexpected executive company symbol: " + executive.Symbol);
            }
        }
        return executivesList;
    }

    public async Task<IndustryBenchmark?> GetIndustryBenchmarkAsync(string industryTitle, CancellationToken cancellationToken)
    {
        string url = $"api/industries/{industryTitle}/benchmark?code={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new CompanyInfoClientException("CompanyInfo service returned failure HTTP status " + response.StatusCode + " for industry " + industryTitle);
        }

        using Stream? contentStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        IndustryBenchmark? benchmark = await JsonSerializer.DeserializeAsync
                    <IndustryBenchmark>(contentStream, cancellationToken: cancellationToken);
        if (benchmark != null && benchmark.IndustryTitle != industryTitle)
        {
            throw new CompanyInfoClientException("Unexpected benchmark industry title: " + benchmark.IndustryTitle);
        }
        return benchmark;
    }
}
