using System;

namespace RecipesRepacoSayco.App.Models;

public class BatchFilterModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchText { get; set; }
}