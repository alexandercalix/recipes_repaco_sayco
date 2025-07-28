using System;

using Microsoft.AspNetCore.SignalR;

namespace RecipesRepacoSayco.App.Hubs;

public class PlcHub : Hub
{
    // ✅ Llamado automáticamente cuando un cliente se conecta
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Cliente conectado: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    // ✅ Llamado automáticamente cuando un cliente se desconecta
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Cliente desconectado: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    // ✅ (Opcional) Cliente puede pedir unirse a un "grupo" por PLC
    public async Task SubscribeToPlc(string plcName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, plcName);
        Console.WriteLine($"Cliente {Context.ConnectionId} suscrito a grupo {plcName}");
    }

    // ✅ (Opcional) Salirse del grupo
    public async Task UnsubscribeFromPlc(string plcName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, plcName);
        Console.WriteLine($"Cliente {Context.ConnectionId} se salió del grupo {plcName}");
    }
}
