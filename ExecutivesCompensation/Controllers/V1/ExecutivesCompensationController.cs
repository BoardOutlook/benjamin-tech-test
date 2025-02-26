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
    private readonly ILogger<ExecutivesCompensationController> _logger;

    public ExecutivesCompensationController(ILogger<ExecutivesCompensationController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Finds all executives on the ASX stock exchange which have a compensation that is at least 10% greater than the average compensation for that industry.
    /// </summary>
    /// <returns>The executive compensation information.</returns>
    [Route("")]
    [HttpGet]
    public IEnumerable<ExecutiveCompensation> Get()
    {
        var comp = new ExecutiveCompensation[] {
                new ExecutiveCompensation
                {
                    NameAndPosition = "John Doe",
                    Compensation = 150000.0,
                    AverageIndustryCompensation = 100000.0
                },
                new ExecutiveCompensation
                {
                    NameAndPosition = "Mary Sue",
                    Compensation = 200000.0,
                    AverageIndustryCompensation = 120000.0
                }};
        return comp;
    }
}
