using System;
using RecipesRepacoSayco.Core.Models;

namespace RecipesRepacoSayco.Core.Services;

public interface IPLCService
{
    string Name { get; }
    bool IsConnected { get; }
    bool IsRunning { get; }

    IEnumerable<ITag> Tags { get; }

    Task StartAsync();
    Task StopAsync();
    Task<bool> WriteTagAsync(string tagName, object value);
    // 🆕 Diagnóstico
    DateTime LastReadTime { get; }
    string LastError { get; }
    bool HasRecentReadFailure { get; }
}
