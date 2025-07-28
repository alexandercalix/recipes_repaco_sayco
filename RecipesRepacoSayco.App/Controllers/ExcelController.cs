using Microsoft.AspNetCore.Mvc;
using RecipesRepacoSayco.App.Models;
using RecipesRepacoSayco.App.Services;
using RecipesRepacoSayco.Data.Repositories;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IExcelReportService _reportService;
    private readonly IBatchProcessRepository _repository;

    public ReportController(IExcelReportService reportService, IBatchProcessRepository repository)
    {
        _reportService = reportService;
        _repository = repository;
    }

    [HttpGet("export1")]
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

    [HttpGet("export")]
    public async Task<IActionResult> ExportBatch([FromQuery] int batch)
    {
        var processes = await _repository.GetByBatch(batch);
        var excel = _reportService.Generate(processes);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Batch_{batch}.xlsx");
    }

}
