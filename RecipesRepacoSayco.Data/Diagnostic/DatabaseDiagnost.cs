using System;
using System.Collections.Concurrent;
using RecipesRepacoSayco.Data.Interfaces;

namespace RecipesRepacoSayco.Data.Diagnostic;

public class DatabaseDiagnostics : IDatabaseDiagnostics
{
    private readonly object _lock = new();

    public bool IsConnected { get; private set; } = false;
    public DateTime LastCheck { get; private set; } = DateTime.UtcNow;

    private readonly ConcurrentQueue<DatabaseLogEntry> _logs = new();
    private readonly TimeSpan _retention = TimeSpan.FromHours(24);
    private readonly int _maxCount = 1000; // Protección extra opcional

    public IReadOnlyList<DatabaseLogEntry> Logs
    {
        get
        {
            lock (_lock)
                return _logs.ToList(); // snapshot
        }
    }

    public void LogCommand(string command, bool success, string? message, string source)
    {
        var entry = new DatabaseLogEntry
        {
            Timestamp = DateTime.UtcNow,
            Command = command,
            Success = success,
            Message = message,
            Source = source
        };

        _logs.Enqueue(entry);
        Cleanup();
    }

    public List<DatabaseLogEntry> GetRecentLogs()
    {
        Cleanup();
        return _logs.Where(log => log.Timestamp >= DateTime.UtcNow - _retention).ToList();
    }

    private void Cleanup()
    {
        while (_logs.TryPeek(out var oldest))
        {
            bool expired = oldest.Timestamp < DateTime.UtcNow - _retention;
            bool overLimit = _logs.Count > _maxCount;

            if (expired || overLimit)
                _logs.TryDequeue(out _);
            else
                break;
        }
    }


    public void SetConnectionStatus(bool isConnected)
    {
        IsConnected = isConnected;
        LastCheck = DateTime.UtcNow;
    }
}
