using System;
using RecipesRepacoSayco.Core.Models;

namespace RecipesRepacoSayco.Core.Notifiers;

public class NullTagNotifier : ITagNotifier
{
    public Task NotifyTagChangedAsync(string plcName, string tagName, object value)
    {
        // No hace nada
        return Task.CompletedTask;
    }
}