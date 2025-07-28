using System;

namespace RecipesRepacoSayco.Core.Models;

public interface ITagNotifier
{
    Task NotifyTagChangedAsync(string plcName, string tagName, object value);
}