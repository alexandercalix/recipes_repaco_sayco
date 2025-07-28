using System;
using Microsoft.AspNetCore.SignalR;
using RecipesRepacoSayco.Core.Models;

namespace RecipesRepacoSayco.App.Hubs;

public class SignalRTagNotifier : ITagNotifier
{
    private readonly IHubContext<PlcHub> _hubContext;

    public SignalRTagNotifier(IHubContext<PlcHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyTagChangedAsync(string plcName, string tagName, object value)
    {
        await NotifyTagsChangedAsync(plcName, new Dictionary<string, object>
        {
            [tagName] = value
        });
    }

    public async Task NotifyTagsChangedAsync(string plcName, Dictionary<string, object> changedTags)
    {
        // Enviar solo a clientes suscritos a ese PLC
        await _hubContext.Clients.Group(plcName).SendAsync("TagsChanged", new
        {
            Plc = plcName,
            Tags = changedTags
        });
    }
}
