using System;

namespace RecipesRepacoSayco.Core.Models.Reports;

public class ReportMappingConfig
{
    public string TriggerTag { get; set; } = "Guardar";
    public string BatchTag { get; set; } = "Batch";
    public string BatchSizeTag { get; set; } = "BatchSize";
    public string RecipeTag { get; set; } = "RecipeNo";
    public List<string> Setpoints { get; set; } = new();
    public List<string> ActualValues { get; set; } = new();
}