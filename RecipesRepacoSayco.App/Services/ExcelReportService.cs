using System;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RecipesRepacoSayco.App.ExtensionMethods;
using RecipesRepacoSayco.App.Models;
using RecipesRepacoSayco.Data.Models;
using RecipesRepacoSayco.Data.Repositories;

namespace RecipesRepacoSayco.App.Services;


public class ExcelReportService : IExcelReportService
{
    private readonly IBatchProcessRepository batch;

    private IOptions<MaterialLabelOptions> MaterialOptions;

    public ExcelReportService(IBatchProcessRepository batch, IOptions<MaterialLabelOptions> materialOptions)
    {
        this.batch = batch;
        MaterialOptions = materialOptions;
    }

    public async Task<FileContentResult> ExportBatchesToExcel(BatchFilterModel filter)
    {
        try
        {
            if (filter.StartDate == null || filter.EndDate == null)
                throw new ArgumentException("Start and End dates are required.");

            var batches = await batch.GetByDateAndTextAsync(
                (DateTime)filter.StartDate,
                (DateTime)filter.EndDate,
                filter.SearchText);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");

            int currentRow = 1;
            int totalColumns = 6 + MaterialOptions.Value.MaterialLabels.Count * 2 + 1;

            // 🔶 Title Row
            worksheet.Cell(currentRow, 1).Value = $"REPACO";
            worksheet.Range(currentRow, 1, currentRow, totalColumns).Merge().Style
                .Font.SetBold().Font.FontSize = 24;
            worksheet.Row(currentRow).Height = 30;
            currentRow++;
            // 🔶 Title Row
            worksheet.Cell(currentRow, 1).Value = $"Batch Report – {filter.StartDate:dd/MM/yyyy} to {filter.EndDate:dd/MM/yyyy}";
            worksheet.Range(currentRow, 1, currentRow, totalColumns).Merge().Style
                .Font.SetBold().Font.FontSize = 14;
            worksheet.Row(currentRow).Height = 20;
            currentRow++;

            // 🔢 Headers
            int col = 1;
            worksheet.Cell(currentRow, col++).Value = "#";
            worksheet.Cell(currentRow, col++).Value = "Producto";
            worksheet.Cell(currentRow, col++).Value = "Inicio";
            worksheet.Cell(currentRow, col++).Value = "Fin";
            worksheet.Cell(currentRow, col++).Value = "Estado";
            worksheet.Cell(currentRow, col++).Value = "Tamano Batch Teorico";
            worksheet.Cell(currentRow, col++).Value = "Tamano Batch Real";

            foreach (var label in MaterialOptions.Value.MaterialLabels)
            {
                worksheet.Cell(currentRow, col++).Value = $"{label} SP";
                worksheet.Cell(currentRow, col++).Value = $"{label} PV";
            }

            worksheet.Row(currentRow).Style.Font.SetBold();
            currentRow++;

            // 🔄 Data Rows
            int rowNumber = 1;
            foreach (var batch in batches)
            {
                col = 1;
                worksheet.Cell(currentRow, col++).Value = rowNumber++;
                worksheet.Cell(currentRow, col++).Value = batch.ProductName;
                worksheet.Cell(currentRow, col++).Value = batch.StartTime;
                worksheet.Cell(currentRow, col - 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                worksheet.Cell(currentRow, col++).Value = batch.EndTime;
                worksheet.Cell(currentRow, col - 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                worksheet.Cell(currentRow, col++).Value = batch.StartTime.Year > 2020 && batch.EndTime.HasValue ? "Finalizado" : "En Proceso";

                var batchSizeTheoretical = batch.Setpoint1 + batch.Setpoint2 + batch.Setpoint3 + batch.Setpoint4;
                var batchSizeReal = batch.ActualValue1 + batch.ActualValue2 + batch.ActualValue3 + batch.ActualValue4;

                worksheet.Cell(currentRow, col++).Value = batchSizeTheoretical.RoundTo();
                worksheet.Cell(currentRow, col++).Value = batchSizeReal.RoundTo();

                worksheet.Cell(currentRow, col++).Value = batch.Setpoint1.RoundTo();
                worksheet.Cell(currentRow, col++).Value = batch.ActualValue1.RoundTo();
                worksheet.Cell(currentRow, col++).Value = batch.Setpoint2.RoundTo();
                worksheet.Cell(currentRow, col++).Value = batch.ActualValue2.RoundTo();
                worksheet.Cell(currentRow, col++).Value = batch.Setpoint3.RoundTo();
                worksheet.Cell(currentRow, col++).Value = batch.ActualValue3.RoundTo();
                worksheet.Cell(currentRow, col++).Value = batch.Setpoint4.RoundTo();
                worksheet.Cell(currentRow, col++).Value = batch.ActualValue4.RoundTo();

                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return new FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"BatchReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            };
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException("Invalid date range provided.", ex);
        }
    }

}
