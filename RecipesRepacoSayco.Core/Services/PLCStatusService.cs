using System;

namespace RecipesRepacoSayco.Core.Services;

public class PlcStatusService
{
    public event Action? OnStatusChanged;

    private bool _isConnected;
    private string _ipAddress = string.Empty;
    private string? _lastError;
    private readonly List<string> _eventLog = new();

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                AddEvent(value ? "PLC connected." : "PLC disconnected.");
                NotifyStateChanged();
            }
        }
    }

    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            if (_ipAddress != value)
            {
                _ipAddress = value;
                AddEvent($"IP updated to {value}");
                NotifyStateChanged();
            }
        }
    }

    public string? LastError
    {
        get => _lastError;
        set
        {
            if (_lastError != value)
            {
                _lastError = value;
                if (!string.IsNullOrWhiteSpace(value))
                    AddEvent($"Error: {value}");
                NotifyStateChanged();
            }
        }
    }

    public IReadOnlyList<string> EventLog => _eventLog.AsReadOnly();

    public void AddEvent(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _eventLog.Insert(0, $"[{timestamp}] {message}");
        if (_eventLog.Count > 20)
            _eventLog.RemoveAt(_eventLog.Count - 1);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStatusChanged?.Invoke();
}

