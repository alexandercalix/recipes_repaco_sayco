using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RecipesRepacoSayco.Core.Models;
using RecipesRepacoSayco.Core.Models.Definitions;
using RecipesRepacoSayco.Plc.Models;
using RecipesRepacoSayco.Plc.Services;
using S7.Net;
using Xunit;

namespace RecipesRepacoSayco.Plc.Tests
{
    public class PLCServiceTests
    {


        private PlcConnectionDefinition CreateTestConfig(string name = "PLC1")
        {
            return new PlcConnectionDefinition
            {
                Driver = "Siemens",
                IpAddress = "192.168.0.1",
                Rack = 0,
                Slot = 1,
                Tags = new List<PlcTagDefinition>
        {
            new PlcTagDefinition { Name = "Confirmar", Datatype = "Bool", Address = "DB1.DBX0.0", DefaultValue = false },
            new PlcTagDefinition { Name = "Finalizar", Datatype = "Bool", Address = "DB1.DBX0.1", DefaultValue = false },
            new PlcTagDefinition { Name = "Tanque 1", Datatype = "Real", Address = "DB1.DBD2", DefaultValue = 0.0f },
            new PlcTagDefinition { Name = "Presion", Datatype = "Int", Address = "DB1.DBW6", DefaultValue = 0 },
            new PlcTagDefinition { Name = "Nivel", Datatype = "Word", Address = "MW10", DefaultValue = (ushort)0 },
            new PlcTagDefinition { Name = "Sensor Entrada", Datatype = "Bool", Address = "I1.0", DefaultValue = false },
            new PlcTagDefinition { Name = "Motor Salida", Datatype = "Bool", Address = "Q2.1", DefaultValue = false }
        }
            };
        }

        [Fact]
        public void Constructor_ShouldInitializeTags()
        {
            var service = new SiemensPLCService(CreateTestConfig());

            service.Tags.Should().NotBeNull();
            service.Tags.Should().HaveCount(7);
        }

        [Fact]
        public void Tags_ShouldImplementITagInterface()
        {
            var service = new SiemensPLCService(CreateTestConfig());
            foreach (var tag in service.Tags)
            {
                tag.Should().BeAssignableTo<ITag>();
            }
        }

        [Fact]
        public async Task StartAsync_ShouldStartCycle()
        {
            var service = new SiemensPLCService(CreateTestConfig());
            await service.StartAsync();
            service.IsRunning.Should().BeTrue();
            await service.StopAsync();
        }

        [Fact]
        public async Task StopAsync_ShouldCancelCycle()
        {
            var service = new SiemensPLCService(CreateTestConfig());
            await service.StartAsync();
            await service.StopAsync();
            service.IsRunning.Should().BeFalse();
        }

        [Fact]
        public async Task StartAsync_ShouldNotStartIfAlreadyRunning()
        {
            var service = new SiemensPLCService(CreateTestConfig());
            await service.StartAsync();
            var taskBefore = service.IsRunning;
            await service.StartAsync();
            var taskAfter = service.IsRunning;
            taskBefore.Should().BeTrue();
            taskAfter.Should().BeTrue();
            await service.StopAsync();
        }

        [Fact]
        public void IsConnected_ShouldReflectClientConnection()
        {
            var service = new SiemensPLCService(CreateTestConfig());
            service.IsConnected.Should().BeFalse();
        }

        [Fact]
        public void Tags_ShouldContainSpecificNames()
        {
            var service = new SiemensPLCService(CreateTestConfig());
            var tagNames = service.Tags.Select(t => t.Name);
            tagNames.Should().Contain("Confirmar");
            tagNames.Should().Contain("Finalizar");
            tagNames.Should().Contain("Tanque 1");
        }

        [Fact]
        public void Tags_ShouldHaveCorrectDataTypes()
        {
            var service = new SiemensPLCService(CreateTestConfig());
            var tag = service.Tags.First(t => t.Name == "Tanque 1");
            tag.Datatype.Should().Be("Real");
        }

        [Fact]
        public void Tags_ShouldHaveCorrectAddresses()
        {
            var service = new SiemensPLCService(CreateTestConfig());
            var tag = service.Tags.First(t => t.Name == "Confirmar");
            tag.Address.Should().Be("DB1.DBX0.0");
        }

        [Fact]
        public void Tag_RealValue_ShouldBeFloat()
        {
            var tag = new SiemensTag("Test", "Real", "DB1.DBD4", 0f);
            tag.Value.Should().BeOfType<float>();
        }

        [Fact]
        public void Tag_SettingIncompatibleType_ShouldThrow()
        {
            var tag = new SiemensTag("Test", "Bool", "DB1.DBX0.0", false);
            Action act = () => tag.Value = 123; // not a bool
            act.Should().Throw<InvalidCastException>();
        }

        [Fact]
        public void Tag_BitOffset_ShouldBeParsedCorrectly()
        {
            var tag = new SiemensTag("Test", "Bool", "DB1.DBX0.7", false);
            tag.Item.BitAdr.Should().Be(7);
        }
    }
}
