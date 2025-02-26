using System.Text.Json;
using ExecutivesCompensation.Clients.Model;

namespace ExecutivesCompensation.Clients;

public interface ICompanyInfoClient
{
    public Task<IEnumerable<StockInfo>> GetExchangeCompaniesAsync(string exchangeSymbol, CancellationToken cancellationToken);

    public Task<IEnumerable<Executive>> GetCompanyExecutivesAsync(string companySymbol, CancellationToken cancellationToken);

    public Task<IndustryBenchmark> GetIndustryBenchmarkAsync(string industryTitle, CancellationToken cancellationToken);
}

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

    /// <summary>
    /// Gets all companies listed on the given exchange.
    /// </summary>
    /// <returns>The info for each company.</returns>
    /// <exception cref="CompanyInfoClientException">If the server returns a failure response or unexpected data.</exception>
    /// <exception cref="JsonException">If there is an error in parsing the JSON response.</exception>
    public async Task<IEnumerable<StockInfo>> GetExchangeCompaniesAsync(string exchangeSymbol, CancellationToken cancellationToken)
    {
        string url = $"api/exchanges/{exchangeSymbol}/companies?code={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new CompanyInfoClientException("CompanyInfo service returned failure HTTP status " + response.StatusCode);
        }

        using Stream contentStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        IEnumerable<StockInfo>? stocks = await JsonSerializer.DeserializeAsync
            <IEnumerable<StockInfo>>(contentStream, cancellationToken: cancellationToken);
        if (stocks == null)
        {
            throw new JsonException("Unexpected null IEnumerable<StockInfo> from JsonSerializer");
        }
        StockInfo[] stocksArray = stocks.ToArray();
        foreach (var stock in stocksArray)
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
        return stocksArray;
    }

    /// <summary>
    /// Gets all company executives for the given company.
    /// </summary>
    /// <returns>The info for each executive.</returns>
    /// <exception cref="CompanyInfoClientException">If the server returns a failure response or unexpected data.</exception>
    /// <exception cref="JsonException">If there is an error in parsing the JSON response.</exception>
    public async Task<IEnumerable<Executive>> GetCompanyExecutivesAsync(string companySymbol, CancellationToken cancellationToken)
    {
        string url = $"api/companies/{companySymbol}/executives?code={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new CompanyInfoClientException("CompanyInfo service returned failure HTTP status " + response.StatusCode);
        }

        using Stream? contentStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        IEnumerable<Executive>? executives = await JsonSerializer.DeserializeAsync
            <IEnumerable<Executive>>(contentStream, cancellationToken: cancellationToken);
        if (executives == null)
        {
            throw new JsonException("Unexpected null IEnumerable<Executive> from JsonSerializer");
        }
        Executive[] executivesArray = executives.ToArray();
        foreach (var executive in executivesArray)
        {
            if (executive.Symbol != companySymbol)
            {
                throw new CompanyInfoClientException("Unexpected executive company symbol: " + executive.Symbol);
            }
        }
        return executivesArray;
    }

    /// <summary>
    /// Gets the benchmark information for the given industry.
    /// </summary>
    /// <returns>The benchmark information.</returns>
    /// <exception cref="CompanyInfoClientException">If the server returns a failure response or unexpected data.</exception>
    /// <exception cref="JsonException">If there is an error in parsing the JSON response.</exception>
    public async Task<IndustryBenchmark> GetIndustryBenchmarkAsync(string industryTitle, CancellationToken cancellationToken)
    {
        string url = $"api/industries/{industryTitle}/benchmark?code={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new CompanyInfoClientException("CompanyInfo service returned failure HTTP status " + response.StatusCode);
        }

        using Stream? contentStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        IndustryBenchmark? benchmark = await JsonSerializer.DeserializeAsync
                    <IndustryBenchmark>(contentStream, cancellationToken: cancellationToken);
        if (benchmark == null)
        {
            throw new JsonException("Unexpected null IndustryBenchmark from JsonSerializer");
        }
        if (benchmark.IndustryTitle != industryTitle)
        {
            throw new CompanyInfoClientException("Unexpected benchmark industry title: " + benchmark.IndustryTitle);
        }
        return benchmark;
    }
}

