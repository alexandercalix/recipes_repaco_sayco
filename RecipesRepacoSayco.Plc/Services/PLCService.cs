using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RecipesRepacoSayco.Plc.Models;
using S7.Net;

namespace RecipesRepacoSayco.Plc.Services;

public class PLCService
{
    private readonly string _plcIp;
    private readonly S7.Net.Plc _client;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private CancellationTokenSource _cts;
    private Task _cycleTask;

    public IEnumerable<Tag> Tags { get; private set; }
    public bool IsConnected => _client.IsConnected;
    public bool IsRunning => _cycleTask != null && !_cycleTask.IsCompleted;

    public PLCService(string plcIp)
    {
        _plcIp = plcIp;
        _client = new S7.Net.Plc(CpuType.S71200, plcIp, 0, 1);
        FillTags();
    }

    private void FillTags()
    {
        Tags = new List<Tag>
        {
            new Tag("Confirmar", "Bool", "DB1.DBX0.0", false),
            new Tag("Finalizar", "Bool", "DB1.DBX0.1", false),
            new Tag("Tanque 1", "Real", "DB1.DBD2", 0f),
            new Tag("Tanque 2", "Real", "DB1.DBD6", 0f),
            new Tag("Tanque 3", "Real", "DB1.DBD10", 0f),
            new Tag("Tanque 4", "Real", "DB1.DBD14", 0f)
        };
    }

    public async Task StartAsync()
    {
        if (IsRunning)
            return;

        _cts = new CancellationTokenSource();
        _cycleTask = Task.Run(() => Cycle(_cts.Token));
    }

    public async Task StopAsync()
    {
        if (!IsRunning)
            return;

        _cts.Cancel();
        await _cycleTask;
    }

    private async Task ConnectAsync()
    {
        if (!_client.IsConnected)
        {
            try
            {
                await _client.OpenAsync();
                Console.WriteLine("PLC connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
        }
    }

    private async Task Cycle(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _semaphore.WaitAsync(token);

                if (!_client.IsConnected)
                    await ConnectAsync();

                var items = Tags.Select(t => t.Item).ToList();

                for (int i = 0; i < items.Count; i += 19)
                {
                    var batch = items.Skip(i).Take(19).ToList();
                    await _client.ReadMultipleVarsAsync(batch);
                }

                await Task.Delay(100, token); // ajustable
            }
            catch (OperationCanceledException)
            {
                // Exit requested
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PLC cycle error: {ex.Message}");
            }
            finally
            {
                if (_semaphore.CurrentCount == 0)
                    _semaphore.Release();
            }
        }

        if (_client.IsConnected)
            _client.Close();
    }

    public async Task<bool> WriteTagAsync(string tagName, object value)
    {
        var tag = Tags.FirstOrDefault(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        if (tag == null)
            throw new ArgumentException($"Tag not found: {tagName}");

        try
        {
            await _semaphore.WaitAsync();

            tag.Value = value; // valida tipo internamente
            await _client.WriteAsync(tag.Item);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing tag '{tagName}': {ex.Message}");
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
