using ExecutivesCompensation.Clients;
using ExecutivesCompensation.Clients.Model;
using Microsoft.AspNetCore.Mvc;

namespace ExecutivesCompensation.Controllers.V1;

/// <summary>
/// Executives compensation controller.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/v1/companies/executives/compensation")]
public class ExecutivesCompensationController : ControllerBase
{
    private readonly ICompanyInfoClient _companyInfoClient;
    private readonly ILogger<ExecutivesCompensationController> _logger;
    private const string AsxExchangeSymbol = "ASX";
    // Constant used for filtering executives by total compensation.
    private const double CompensationMultiple = 1.1;
    private const int MaxConcurrentOutgoingRequests = 20;

    public ExecutivesCompensationController(ICompanyInfoClient companyInfoClient, ILogger<ExecutivesCompensationController> logger)
    {
        _companyInfoClient = companyInfoClient;
        _logger = logger;
    }

    /// <summary>
    /// Finds all executives on the ASX stock exchange which have a compensation that is at least 10% greater than the average compensation for that industry.
    /// </summary>
    /// <returns>The executive compensation information.</returns>
    [Route("")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExecutiveCompensation>>> GetAsync(CancellationToken cancellationToken)
    {
        IList<StockInfo> stockInfos = await _companyInfoClient.GetExchangeCompaniesAsync(AsxExchangeSymbol, cancellationToken);

        if (stockInfos.Count == 0)
        {
            // This probably indicates something wrong with the backend.
            return Problem("Found 0 stocks on the ASX", statusCode: StatusCodes.Status500InternalServerError);
        }

        IEnumerable<string> distinctSymbols = stockInfos
            .Where(s => s.Symbol != null && s.Symbol.Length != 0) // ignore eventual empty company symbols
            .Select(s => s.Symbol!)
            .Distinct(); // merge eventual duplicate companies

        // Make requests for each symbol in parallel. Use mutex to limit concurrent outgoing requests.
        var mutex = new SemaphoreSlim(MaxConcurrentOutgoingRequests);
        var executivesTasks = distinctSymbols.Select(async companySymbol =>
            {
                try
                {
                    await mutex.WaitAsync();
                    IList<Executive> executives = await _companyInfoClient.GetCompanyExecutivesAsync(companySymbol, cancellationToken);
                    // Note: It's OK here if a company has no executives.
                    return executives;
                }
                finally { mutex.Release(); }
            });

        IList<Executive>[] executivesList = await Task.WhenAll(executivesTasks);

        IEnumerable<Executive> allExecutives = executivesList
            .SelectMany(l => l);

        IEnumerable<string> distinctIndustries = allExecutives
                    .Where(e => e.IndustryTitle != null && e.IndustryTitle.Length != 0) // ignore eventual empty industry titles
                    .Select(e => e.IndustryTitle!)
                    .Distinct(); // merge eventual duplicate industry titles

        // Make requests for each industry in parallel. Use mutex to limit concurrent outgoing requests.
        var benchmarkTasks = distinctIndustries.Select(async industryTitle =>
            {
                try
                {
                    await mutex.WaitAsync();
                    IndustryBenchmark? benchmark = await _companyInfoClient.GetIndustryBenchmarkAsync(industryTitle, cancellationToken);
                    return benchmark;
                }
                finally { mutex.Release(); }
            });

        IList<IndustryBenchmark?> allBenchmarks = await Task.WhenAll(benchmarkTasks);

        // Populate dictionary of average salaries by industry.
        var averageSalaryByIndustry = new Dictionary<string, double>();
        foreach (IndustryBenchmark? benchmark in allBenchmarks)
        {
            // The client does return null for some benchmarks, so we need to ignore nulls.
            string? industryTitle = benchmark?.IndustryTitle;
            if (industryTitle != null && industryTitle.Length != 0)
            {
                averageSalaryByIndustry[industryTitle] = benchmark!.AverageCompensation;
            }
        }

        // Finally, filter executives to those 10% or more above average salary.
        // Ignore executives from industries for which we couldn't find a benchmark.
        // Note: If the same executive is listed at multiple companies, they will be returned once per company.
        IEnumerable<ExecutiveCompensation> executivesFilteredByCompensation = allExecutives
            .Where(e => e.IndustryTitle != null
                     && e.IndustryTitle.Length != 0
                     && averageSalaryByIndustry.ContainsKey(e.IndustryTitle)
                     && e.Total >= CompensationMultiple * averageSalaryByIndustry[e.IndustryTitle])
            .Select(e => new ExecutiveCompensation
            {
                NameAndPosition = e.NameAndPosition,
                Compensation = e.Total,
                AverageIndustryCompensation = averageSalaryByIndustry[e.IndustryTitle!]
            });
        return Ok(executivesFilteredByCompensation);
    }
}
