using System.Text.Json;
using ExecutivesCompensation.Clients.Model;

namespace ExecutivesCompensation.Clients;

public interface ICompanyInfoClient
{
    /// <summary>
    /// Gets all companies listed on the given exchange.
    /// </summary>
    /// <returns>The info for each company.</returns>
    /// <exception cref="CompanyInfoClientException">If the server returns a failure response or unexpected data.</exception>
    /// <exception cref="JsonException">If there is an error in parsing the JSON response.</exception>
    public Task<IList<StockInfo>> GetExchangeCompaniesAsync(string exchangeSymbol, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all company executives for the given company.
    /// </summary>
    /// <returns>The info for each executive.</returns>
    /// <exception cref="CompanyInfoClientException">If the server returns a failure response or unexpected data.</exception>
    /// <exception cref="JsonException">If there is an error in parsing the JSON response.</exception>
    public Task<IList<Executive>> GetCompanyExecutivesAsync(string companySymbol, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the benchmark information for the given industry.
    /// </summary>
    /// <returns>The benchmark information, or null if not found.</returns>
    /// <exception cref="CompanyInfoClientException">If the server returns a failure response or unexpected data.</exception>
    /// <exception cref="JsonException">If there is an error in parsing the JSON response.</exception>
    public Task<IndustryBenchmark?> GetIndustryBenchmarkAsync(string industryTitle, CancellationToken cancellationToken);
}
