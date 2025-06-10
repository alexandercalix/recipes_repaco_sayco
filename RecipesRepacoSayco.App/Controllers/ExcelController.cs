using Microsoft.AspNetCore.Mvc;
using RecipesRepacoSayco.App.Models;
using RecipesRepacoSayco.App.Services;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IExcelReportService _reportService;

    public ReportController(IExcelReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportToExcel([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? searchText = null)
    {
        Console.WriteLine($"Exporting batches from {startDate} to {endDate} with search text '{searchText}'");
        if (startDate == default || endDate == default)
            return BadRequest("Invalid date range.");

        var filter = new BatchFilterModel
        {
            StartDate = startDate,
            EndDate = endDate,
            SearchText = searchText
        };

        var result = await _reportService.ExportBatchesToExcel(filter);
        return result;
    }
}
