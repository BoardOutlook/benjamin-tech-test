using System.Diagnostics;
using ExecutivesCompensation.Clients.Model;
using Microsoft.Extensions.Caching.Memory;

namespace ExecutivesCompensation.Clients;

public class CachingCompanyInfoClient : ICompanyInfoClient
{
    private ICompanyInfoClient _baseClient;
    private IMemoryCache _cache;

    // TODO: Add jitter so that not all entries expire at the same time.
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(relative: TimeSpan.FromHours(1));

    private bool TryGetExchangeCompaniesFromCache(string exchangeSymbol, out IList<StockInfo>? companies)
    {
        return _cache.TryGetValue("exch_comp:" + exchangeSymbol, out companies);
    }

    private bool TryGetCompanyExecutivesFromCache(string companySymbol, out IList<Executive>? executives)
    {
        return _cache.TryGetValue("comp_exec:" + companySymbol, out executives);
    }

    private bool TryGetIndustryBenchmarkFromCache(string industryTitle, out IndustryBenchmark? benchmark)
    {
        return _cache.TryGetValue("ind_bench:" + industryTitle, out benchmark);
    }

    private void AddExchangeCompaniesToCache(string exchangeSymbol, IList<StockInfo> companies)
    {
        _cache.Set("exch_comp:" + exchangeSymbol, companies, _cacheEntryOptions);
    }

    private void AddCompanyExecutivesToCache(string companySymbol, IList<Executive> executives)
    {
        _cache.Set("comp_exec:" + companySymbol, executives, _cacheEntryOptions);
    }

    private void AddIndustryBenchmarkToCache(string industryTitle, IndustryBenchmark? benchmark)
    {
        _cache.Set("ind_bench:" + industryTitle, benchmark, _cacheEntryOptions);
    }
    public CachingCompanyInfoClient(ICompanyInfoClient baseClient, IMemoryCache cache)
    {
        _baseClient = baseClient;
        _cache = cache;
    }

    public async Task<IList<StockInfo>> GetExchangeCompaniesAsync(string exchangeSymbol, CancellationToken cancellationToken)
    {
        if (TryGetExchangeCompaniesFromCache(exchangeSymbol, out IList<StockInfo>? cachedResult))
        {
            Debug.WriteLine("Serving GetExchangeCompaniesAsync from cache for exchange " + exchangeSymbol);
            return cachedResult!;
        }
        var result = await _baseClient.GetExchangeCompaniesAsync(exchangeSymbol, cancellationToken);
        Debug.WriteLine("Inserting GetExchangeCompaniesAsync result into cache for exchange " + exchangeSymbol);
        AddExchangeCompaniesToCache(exchangeSymbol, result);
        return result;
    }

    public async Task<IList<Executive>> GetCompanyExecutivesAsync(string companySymbol, CancellationToken cancellationToken)
    {
        if (TryGetCompanyExecutivesFromCache(companySymbol, out IList<Executive>? cachedResult))
        {
            Debug.WriteLine("Serving GetCompanyExecutivesAsync from cache for company " + companySymbol);
            return cachedResult!;
        }
        var result = await _baseClient.GetCompanyExecutivesAsync(companySymbol, cancellationToken);
        Debug.WriteLine("Inserting GetCompanyExecutivesAsync result into cache for company " + companySymbol);
        AddCompanyExecutivesToCache(companySymbol, result);
        return result;
    }

    public async Task<IndustryBenchmark?> GetIndustryBenchmarkAsync(string industryTitle, CancellationToken cancellationToken)
    {
        if (TryGetIndustryBenchmarkFromCache(industryTitle, out IndustryBenchmark? cachedResult))
        {
            Debug.WriteLine("Serving GetIndustryBenchmarkAsync from cache for industry " + industryTitle);
            return cachedResult;
        }
        var result = await _baseClient.GetIndustryBenchmarkAsync(industryTitle, cancellationToken);
        Debug.WriteLine("Inserting GetIndustryBenchmarkAsync result into cache for industry " + industryTitle);
        AddIndustryBenchmarkToCache(industryTitle, result);
        return result;
    }

}
