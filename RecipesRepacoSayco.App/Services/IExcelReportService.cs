using Microsoft.AspNetCore.Mvc;
using RecipesRepacoSayco.App.Models;

public interface IExcelReportService
{
    Task<FileContentResult> ExportBatchesToExcel(BatchFilterModel filter);
}