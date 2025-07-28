using Microsoft.AspNetCore.Mvc;
using RecipesRepacoSayco.App.Models;
using RecipesRepacoSayco.Data.Models;

public interface IExcelReportService
{
    Task<FileContentResult> ExportBatchesToExcel(BatchFilterModel filter);
    public byte[] Generate(List<BatchProcess> processes);
}