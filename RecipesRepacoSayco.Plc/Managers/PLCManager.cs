using System;
using RecipesRepacoSayco.Core.Models;
using RecipesRepacoSayco.Core.Models.Definitions;
using RecipesRepacoSayco.Core.Services;
using RecipesRepacoSayco.Plc.Services;

namespace RecipesRepacoSayco.Plc.Managers;

public class PlcManager
{
    private readonly List<IPLCService> _plcServices = new();

    public IReadOnlyList<IPLCService> PlcServices => _plcServices;
    private readonly ITagNotifier _tagNotifier;

    private readonly IEnumerable<PlcConnectionDefinition> _plcsDefinitions;

    public PlcManager(ITagNotifier tagNotifier, IEnumerable<PlcConnectionDefinition> plcsDefinitions)
    {
        _plcsDefinitions = plcsDefinitions;
        _tagNotifier = tagNotifier;
    }

    public void InitializePlcs()
    {
        var definitions = _plcsDefinitions;

        foreach (var def in definitions)
        {
            IPLCService plcService = def.Driver switch
            {
                "Siemens" => new SiemensPLCService(def),
                _ => throw new NotSupportedException($"Driver '{def.Driver}' is not supported.")
            };
            plcService.StartAsync();
            _plcServices.Add(plcService);
        }
    }

    public IEnumerable<IPLCService>? GetPlcs()
    {
        return _plcServices.ToList();
    }

    public IPLCService? GetPlc(string name)
    {
        return _plcServices.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task StartPlcAsync(string name)
    {
        var plc = GetPlc(name);
        if (plc != null && !plc.IsRunning)
            await plc.StartAsync();
    }

    public async Task StopPlcAsync(string name)
    {
        var plc = GetPlc(name);
        if (plc != null && plc.IsRunning)
            await plc.StopAsync();
    }

    public IEnumerable<ITag>? GetTags(string plcName)
    {
        return GetPlc(plcName)?.Tags;
    }

    public async Task<bool> WriteTagAsync(string plcName, string tagName, object value)
    {
        var plc = GetPlc(plcName);
        if (plc is SiemensPLCService siemens) // puedes usar un método común si lo defines en IPlcService
            return await siemens.WriteTagAsync(tagName, value);

        return false;
    }
}
