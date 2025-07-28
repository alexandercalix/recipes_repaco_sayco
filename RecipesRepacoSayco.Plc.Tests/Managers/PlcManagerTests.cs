using System;

namespace RecipesRepacoSayco.Plc.Tests.Managers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RecipesRepacoSayco.Core.Models;
using RecipesRepacoSayco.Core.Notifiers;
using RecipesRepacoSayco.Core.Services;
using RecipesRepacoSayco.Plc.Managers;
using RecipesRepacoSayco.Plc.Services;
using Xunit;

public class PlcManagerTests
{
    [Fact]
    public void InitializePlcs_ShouldCreateExpectedNumberOfPlcs()
    {
        var manager = new PlcManager(new NullTagNotifier());
        manager.InitializePlcs();

        manager.PlcServices.Should().NotBeNull();
        manager.PlcServices.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void GetPlc_ShouldReturnCorrectInstance()
    {
        var manager = new PlcManager(new NullTagNotifier());
        manager.InitializePlcs();

        var plc = manager.GetPlc("PLC_Main");
        plc.Should().NotBeNull();
        plc.Name.Should().Be("PLC_Main");
    }

    [Fact]
    public async Task StartPlcAsync_ShouldStartPlc()
    {
        var mockPlc = new Mock<IPLCService>();
        mockPlc.SetupGet(p => p.Name).Returns("PLC_Main");
        mockPlc.SetupGet(p => p.IsRunning).Returns(false);
        mockPlc.Setup(p => p.StartAsync()).Returns(Task.CompletedTask).Verifiable();

        var manager = new PlcManager(new NullTagNotifier());
        typeof(PlcManager)
            .GetField("_plcServices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, new List<IPLCService> { mockPlc.Object });

        await manager.StartPlcAsync("PLC_Main");

        mockPlc.Verify(p => p.StartAsync(), Times.Once);
    }

    [Fact]
    public async Task StopPlcAsync_ShouldStopPlc()
    {
        var mockPlc = new Mock<IPLCService>();
        mockPlc.SetupGet(p => p.Name).Returns("PLC_Main");
        mockPlc.SetupGet(p => p.IsRunning).Returns(true);
        mockPlc.Setup(p => p.StopAsync()).Returns(Task.CompletedTask).Verifiable();

        var manager = new PlcManager(new NullTagNotifier());
        typeof(PlcManager)
            .GetField("_plcServices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, new List<IPLCService> { mockPlc.Object });

        await manager.StopPlcAsync("PLC_Main");

        mockPlc.Verify(p => p.StopAsync(), Times.Once);
    }

    [Fact]
    public void GetTags_ShouldReturnTagsForPlc()
    {
        var mockTags = new List<Mock<ITag>>
        {
            new Mock<ITag>(), new Mock<ITag>()
        };

        var mockPlc = new Mock<IPLCService>();
        mockPlc.SetupGet(p => p.Name).Returns("PLC_Main");
        mockPlc.SetupGet(p => p.Tags).Returns(mockTags.Select(m => m.Object));

        var manager = new PlcManager(new NullTagNotifier());
        typeof(PlcManager)
            .GetField("_plcServices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, new List<IPLCService> { mockPlc.Object });

        var tags = manager.GetTags("PLC_Main");

        tags.Should().NotBeNull();
        tags.Should().HaveCount(2);
    }

    // [Fact]
    // public async Task WriteTagAsync_ShouldCallUnderlyingService()
    // {
    //     var mockPlc = new Mock<SiemensPLCService>(null!, null!); // parámetro nulo por constructor fake
    //     mockPlc.SetupGet(p => p.Name).Returns("PLC_Main");
    //     mockPlc.Setup(p => p.WriteTagAsync("Confirmar", true)).ReturnsAsync(true).Verifiable();

    //     var manager = new PlcManager(new NullTagNotifier());
    //     typeof(PlcManager)
    //         .GetField("_plcServices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
    //         ?.SetValue(manager, new List<IPLCService> { mockPlc.Object });

    //     var result = await manager.WriteTagAsync("PLC_Main", "Confirmar", true);
    //     result.Should().BeTrue();
    //     mockPlc.Verify(p => p.WriteTagAsync("Confirmar", true), Times.Once);
    // }
}
