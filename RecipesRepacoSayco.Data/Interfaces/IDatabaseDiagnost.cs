using System;

namespace RecipesRepacoSayco.Data.Interfaces;

public interface IDatabaseDiagnostics
{
    bool IsConnected { get; }
    DateTime LastCheck { get; }
    IReadOnlyList<DatabaseLogEntry> Logs { get; }
    List<DatabaseLogEntry> GetRecentLogs();

    void LogCommand(string command, bool success, string? message = null, string source = "General");
    void SetConnectionStatus(bool isConnected);
}

public class DatabaseLogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Command { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string Source { get; init; } = "Unknown"; // <- Agregado

}
