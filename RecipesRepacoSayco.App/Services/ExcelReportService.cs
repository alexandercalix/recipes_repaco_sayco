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

            var batches = await batch.GetByDate(
                (DateTime)filter.StartDate,
                (DateTime)filter.EndDate);

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
                worksheet.Cell(currentRow, col++).Value = batch.Batch;
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

    public byte[] Generate(List<BatchProcess> processes)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Reporte");

        // Título principal
        worksheet.Cell("A1").Value = "Informe Batch por Nº Orden Producción";
        worksheet.Range("A1:Q1").Merge().Style
            .Font.SetBold()
            .Font.SetFontSize(16)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Fecha actual
        worksheet.Cell("A3").Value = $"Fecha de generación: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
        worksheet.Cell("A3").Style.Font.Italic = true;

        // Encabezados
        var headers = new[]
        {
        "Nº orden producción", "Fecha inicio", "Hora inicio",
        "Fecha fin", "Hora final", "Nº lote", "Nombre receta",
        "Kg bach teórico", "Kg bach real",
        "Cantidad agua real", "Cantidad melaza real", "Cantidad vinaza real",
        "Cantidad suero real", "Minerales adicionales" ,"Ventury"
    };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(5, i + 1);
            cell.Value = headers[i];
            cell.Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }

        // Datos
        int row = 6;
        foreach (var p in processes)
        {
            worksheet.Cell(row, 1).Value = p.Batch;
            worksheet.Cell(row, 2).Value = p.StartTime.ToString("yyyy/MM/dd");
            worksheet.Cell(row, 3).Value = p.StartTime.ToString("HH:mm:ss");
            worksheet.Cell(row, 4).Value = p.EndTime?.ToString("yyyy/MM/dd");
            worksheet.Cell(row, 5).Value = p.EndTime?.ToString("HH:mm:ss");
            worksheet.Cell(row, 6).Value = p.Batch;
            worksheet.Cell(row, 7).Value = p.RecipeName;
            worksheet.Cell(row, 8).Value = p.Setpoint1 + p.Setpoint2 + p.Setpoint3 + p.Setpoint4 + p.Setpoint5 + p.Setpoint6;
            worksheet.Cell(row, 9).Value = p.ActualValue1 + p.ActualValue2 + p.ActualValue3 + p.ActualValue4 + p.ActualValue5 + p.ActualValue6;
            worksheet.Cell(row, 10).Value = p.ActualValue3;
            worksheet.Cell(row, 11).Value = p.ActualValue1;
            worksheet.Cell(row, 12).Value = p.ActualValue2;
            worksheet.Cell(row, 13).Value = p.ActualValue4;
            worksheet.Cell(row, 14).Value = p.ActualValue5;
            worksheet.Cell(row, 15).Value = p.ActualValue6;

            // Bordes a cada celda
            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cell(row, col).Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }

            row++;
        }

        // Fila total
        worksheet.Cell(row, 1).Value = "Total";
        worksheet.Range(row, 1, row, 7).Merge();
        worksheet.Range(row, 1, row, 7).Style
            .Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.Gray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        for (int col = 10; col <= 15; col++)
        {
            worksheet.Cell(row, col).FormulaA1 = $"SUM({worksheet.Cell(6, col).Address}:{worksheet.Cell(row - 1, col).Address})";
            worksheet.Cell(row, col).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.Gray)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .NumberFormat.SetFormat("#,##0.000");
        }

        int startRow = 6;
        int endRow = startRow + processes.Count - 1;

        worksheet.Cell($"H{endRow + 1}").SetFormulaA1($"SUM(H{startRow}:H{endRow})").Style.Fill.SetBackgroundColor(XLColor.LightGray).Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        worksheet.Cell($"I{endRow + 1}").SetFormulaA1($"SUM(I{startRow}:I{endRow})").Style.Fill.SetBackgroundColor(XLColor.LightGray).Border.SetOutsideBorder(XLBorderStyleValues.Thin);


        // Ajustar columnas
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }





}
