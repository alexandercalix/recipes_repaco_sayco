using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using RecipesRepacoSayco.Plc.Models;
using S7.Net;
using S7.Net.Types;
using Xunit;

namespace RecipesRepacoSayco.Plc.Tests.Models;

public class TagTests
{
    [Theory]
    [InlineData("Bool", true)]
    [InlineData("Byte", (byte)10)]
    [InlineData("Word", (ushort)100)]
    [InlineData("DWord", (uint)1000)]
    [InlineData("Int", (short)-5)]
    [InlineData("DInt", -100)]
    [InlineData("Real", 10.5f)]
    [InlineData("Real", 99.9d)]
    public void Tag_Should_Store_Compatible_Values(string datatype, object value)
    {
        var tag = new SiemensTag("Test", datatype, "DB1.DBX0.0", value);
        tag.Value.Should().Be(value);
        tag.Item.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("Bool", 123)]
    [InlineData("Byte", "string")]
    [InlineData("Word", -1)]
    [InlineData("DWord", -1)]
    [InlineData("Int", 100000)]
    [InlineData("Real", "not a number")]
    public void Tag_Should_Throw_When_Incompatible_Type_Assigned(string datatype, object value)
    {
        var tag = new SiemensTag("Test", datatype, "DB1.DBX0.0", GetDefaultValueForDatatype(datatype));
        Action act = () => tag.Value = value;
        act.Should().Throw<InvalidCastException>();
    }

    [Theory]
    [InlineData("DB1.DBX0.0", DataType.DataBlock, 1, 0, 0)]
    [InlineData("M0.0", DataType.Memory, 0, 0, 0)]
    [InlineData("I2.0", DataType.Input, 0, 2, 0)]
    [InlineData("Q4.7", DataType.Output, 0, 4, 7)]
    public void Tag_Should_Parse_Address_Correctly(string address, DataType expectedArea, int expectedDb, int expectedByte, byte expectedBit)
    {
        var tag = new SiemensTag("Test", "Bool", address, true);
        tag.Item.DataType.Should().Be(expectedArea);
        tag.Item.DB.Should().Be(expectedDb);
        tag.Item.StartByteAdr.Should().Be(expectedByte);
        tag.Item.BitAdr.Should().Be(expectedBit);
    }

    [Fact]
    public void Tag_Should_Throw_When_Address_Is_Invalid()
    {
        Action act = () => new SiemensTag("Test", "Bool", "INVALID", true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Tag_Should_Throw_When_Datatype_Is_Unknown()
    {
        Action act = () => new SiemensTag("Test", "Unknown", "DB1.DBX0.0", 123);
        act.Should().Throw<ArgumentException>();
    }

    private object GetDefaultValueForDatatype(string datatype) => datatype switch
    {
        "Bool" => false,
        "Byte" => (byte)0,
        "Word" => (ushort)0,
        "DWord" => (uint)0,
        "Int" => (short)0,
        "DInt" => 0,
        "Real" => 0.0f,
        _ => throw new ArgumentException("Unknown datatype")
    };
}