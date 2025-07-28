using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RecipesRepacoSayco.Core.Models;
using RecipesRepacoSayco.Core.Models.Definitions;
using RecipesRepacoSayco.Core.Services;
using RecipesRepacoSayco.Plc.Models;
using S7.Net;

namespace RecipesRepacoSayco.Plc.Services
{
    public class SiemensPLCService : IPLCService
    {
        public string Name { get; set; }
        private readonly string _plcIp;
        private readonly S7.Net.Plc _client;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private CancellationTokenSource _cts;
        private Task _cycleTask;

        private List<SiemensTag> _tags = new();

        public IEnumerable<ITag> Tags => _tags;
        public bool IsConnected => _client.IsConnected;
        public bool IsRunning => _cycleTask != null && !_cycleTask.IsCompleted;
        private readonly bool _isTest;

        // 🆕 Diagnóstico adicional
        public DateTime LastReadTime { get; private set; }
        public string LastError { get; private set; } = string.Empty;
        public bool HasRecentReadFailure => (DateTime.Now - LastReadTime).TotalSeconds > 5;

        public SiemensPLCService(PlcConnectionDefinition config)
        {
            Console.WriteLine($"Initializing Siemens PLC Service for {config.Name} at {config.IpAddress}");
            _client = new S7.Net.Plc(CpuType.S71200, config.IpAddress, (short)config.Rack, (short)config.Slot);
            Name = config.Name;
            _plcIp = config.IpAddress;

            _tags = config.Tags
                .Select(t => new SiemensTag(t.Name, t.Datatype, t.Address, ConvertToPlcType(t.Datatype, t.DefaultValue)))
                .ToList();
        }

        private object ConvertToPlcType(string datatype, object value)
        {
            return datatype switch
            {
                "Bool" => Convert.ToBoolean(value),
                "Byte" => Convert.ToByte(value),
                "Word" => Convert.ToUInt16(value),
                "DWord" => Convert.ToUInt32(value),
                "Int" => Convert.ToInt16(value),
                "DInt" => Convert.ToInt32(value),
                "Real" => Convert.ToSingle(value),
                _ => throw new ArgumentException($"Unsupported datatype: {datatype}")
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
            Console.WriteLine($"Connecting to PLC {_plcIp}...");
            if (!_client.IsConnected)
            {
                try
                {
                    await _client.OpenAsync();
                    Console.WriteLine($"PLC {_plcIp} connected.");
                    LastError = string.Empty;
                }
                catch (Exception ex)
                {
                    LastError = $"Connection failed: {ex.Message}";
                    Console.WriteLine(LastError);
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

                    if (_client.IsConnected)
                    {
                        var items = _tags.Select(t => t.Item).ToList();

                        for (int i = 0; i < items.Count; i += 19)
                        {
                            var batch = items.Skip(i).Take(19).ToList();
                            await _client.ReadMultipleVarsAsync(batch);
                        }

                        LastReadTime = DateTime.Now;
                        LastError = string.Empty;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Exit requested
                }
                catch (Exception ex)
                {
                    LastError = $"Cycle error: {ex.Message}";
                    Console.WriteLine(LastError);
                }
                finally
                {
                    if (_semaphore.CurrentCount == 0)
                        _semaphore.Release();
                }

                await Task.Delay(1000, token);
            }

            if (_client.IsConnected)
            {
                _client.Close();
                Console.WriteLine($"PLC {_plcIp} disconnected.");
            }
        }

        public async Task<bool> WriteTagAsync(string tagName, object value)
        {
            var tag = _tags.FirstOrDefault(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
            if (tag == null)
                throw new ArgumentException($"Tag not found: {tagName}");

            try
            {
                await _semaphore.WaitAsync();

                tag.Value = value;
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
}
