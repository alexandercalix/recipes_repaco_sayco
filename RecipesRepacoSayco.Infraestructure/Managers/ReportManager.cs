using RecipesRepacoSayco.Core.Models;
using RecipesRepacoSayco.Core.Models.Definitions;
using RecipesRepacoSayco.Core.Models.Reports;
using RecipesRepacoSayco.Core.Services;
using RecipesRepacoSayco.Data.Models;
using RecipesRepacoSayco.Data.Repositories;
using RecipesRepacoSayco.Plc.Managers;

namespace RecipesRepacoSayco.Infraestructure.Managers;

public class ReportManager
{
    private readonly IBatchProcessRepository batchProcessRepository;
    private readonly PlcManager plcManager;
    private readonly ReportMappingConfig config;

    private IPLCService plc;
    private bool _lastGuardar = false;
    private BatchProcess? _currentBatch = null;

    public ReportManager(
        IBatchProcessRepository batchProcessRepository,
        PlcManager plcManager,
        ReportMappingConfig config)
    {
        this.batchProcessRepository = batchProcessRepository;
        this.plcManager = plcManager;
        this.config = config;

        plc = plcManager.GetPlc("Process")!;
        Console.WriteLine("ReportManager initialized");
    }

    public async Task StartCycleAsync()
    {
        plc = plcManager.GetPlc("Process");

        if (plc == null)
        {
            Console.WriteLine("PLC 'Process' not found.");
            return;
        }

        Console.WriteLine("ReportManager started");
        _currentBatch = await batchProcessRepository.GetLastOpenBatchAsync();

        await Cycle();
    }

    public async Task Cycle()
    {
        while (true)
        {
            try
            {
                var tags = plc.Tags;
                bool guardar = tags.GetBool(config.TriggerTag);

                if (guardar && !_lastGuardar)
                    await OnGuardarAsync(tags);

                _lastGuardar = guardar;

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReportManager Cycle: {ex.Message}");
            }
        }
    }

    private async Task OnGuardarAsync(IEnumerable<ITag> tags)
    {
        Console.WriteLine("📥 Guardando proceso...");

        var now = DateTime.Now;

        var batch = new BatchProcess
        {
            StartTime = now,
            EndTime = now,
            Batch = tags.GetInt(config.BatchTag),
            RecipeName = tags.GetString(config.RecipeTag).Trim(),
        };

        var setpoints = config.Setpoints.Select(tags.GetFloat).ToList(); // ahora es List<decimal?>

        var actuals = config.ActualValues.Select(tags.GetFloat).ToList();

        for (int i = 0; i < setpoints.Count; i++)
        {
            var prop = typeof(BatchProcess).GetProperty($"Setpoint{i + 1}");
            if (prop == null)
                Console.WriteLine($"⚠️ Property 'Setpoint{i + 1}' not found in BatchProcess.");
            else if (prop.CanWrite)
                prop.SetValue(batch, setpoints[i]);
        }

        for (int i = 0; i < actuals.Count; i++)
        {
            var prop = typeof(BatchProcess).GetProperty($"ActualValue{i + 1}");
            if (prop == null)
                Console.WriteLine($"⚠️ Property 'ActualValue{i + 1}' not found in BatchProcess.");
            else if (prop.CanWrite)
                prop.SetValue(batch, actuals[i]);
        }

        Console.WriteLine($"Batch: {batch.Batch}, Recipe: {batch.RecipeName}");

        await batchProcessRepository.CreateAsync(batch);
    }
}
public static class TagExtensions
{
    public static int GetInt(this IEnumerable<ITag> tags, string name)
    {
        var val = tags.FirstOrDefault(t => t.Name == name)?.Value;

        if (val == null) return -1;
        if (val is int i) return i;
        if (val is short s) return s;
        if (val is float f) return (int)f;
        if (val is double d) return (int)d;
        if (int.TryParse(val.ToString(), out int result)) return result;

        return -1;
    }

    public static decimal? GetFloat(this IEnumerable<ITag> tags, string name)
    {
        var val = tags.FirstOrDefault(t => t.Name == name)?.Value;
        Console.WriteLine($"[DEBUG] {name} => {val} ({val?.GetType().Name})");

        return val switch
        {
            decimal d => d,
            double d => Convert.ToDecimal(d),
            float f => Convert.ToDecimal(f),
            int i => i,
            long l => l,
            short s => s,
            string str when decimal.TryParse(str, out var result) => result,
            _ => null
        };
    }



    public static bool GetBool(this IEnumerable<ITag> tags, string name) =>
        tags.FirstOrDefault(t => t.Name == name)?.Value as bool? ?? false;

    public static string GetString(this IEnumerable<ITag> tags, string name) =>
        tags.FirstOrDefault(t => t.Name == name)?.Value?.ToString() ?? "Unknown";
}
