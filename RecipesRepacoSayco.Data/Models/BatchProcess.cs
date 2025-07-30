using System;
using System.ComponentModel.DataAnnotations;

namespace RecipesRepacoSayco.Data.Models;

public class BatchProcess
{
    public int Id { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public required int Batch { get; set; }
    [MaxLength(100)]
    public required string RecipeName { get; set; }

    public decimal? BatchSize { get; set; }

    public decimal? Setpoint1 { get; set; }
    public decimal? ActualValue1 { get; set; }
    public decimal? Setpoint2 { get; set; }
    public decimal? ActualValue2 { get; set; }
    public decimal? Setpoint3 { get; set; }
    public decimal? ActualValue3 { get; set; }
    public decimal? Setpoint4 { get; set; }
    public decimal? ActualValue4 { get; set; }
    public decimal? Setpoint5 { get; set; }
    public decimal? ActualValue5 { get; set; }
    public decimal? Setpoint6 { get; set; }
    public decimal? ActualValue6 { get; set; }
}