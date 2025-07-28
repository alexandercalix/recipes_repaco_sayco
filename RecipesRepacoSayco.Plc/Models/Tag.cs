using System;
using System.Text.RegularExpressions;
using RecipesRepacoSayco.Core.Models;
using S7.Net;
using S7.Net.Types;

namespace RecipesRepacoSayco.Plc.Models;

public class SiemensTag : ITag
{
    public string Name { get; set; }
    public string Datatype { get; init; } // Now immutable
    public string Address { get; init; }  // Now immutable
    public bool Quality { get; internal set; } // Quality is managed internally

    private object _value;

    public object Value
    {
        get => Item?.Value ?? _value;
        set
        {
            if (!IsCompatibleType(value))
                throw new InvalidCastException($"Value type '{value?.GetType().Name}' is not compatible with PLC type '{Datatype}'.");

            _value = value;
            if (Item != null)
                Item.Value = value;
        }
    }

    public DataItem Item { get; private set; }

    public SiemensTag(string name, string datatype, string address, object value)
    {
        Name = name;
        Datatype = ValidateDatatype(datatype);
        Address = address;
        _value = value;

        Build();
        Value = value; // triggers validation and sync
    }

    private void Build()
    {
        Item = new DataItem
        {
            Count = 1,
            DataType = ParseAreaType(Address),
            VarType = ParseVarType(Datatype),
            DB = ParseDbNumber(Address),
            StartByteAdr = ParseByteOffset(Address),
            BitAdr = ParseBitOffset(Address),
            Value = _value
        };

        Console.WriteLine($"SiemensTag {Name} {Item.DataType} {Item.VarType} {Item.StartByteAdr}");
    }

    private static string ValidateDatatype(string datatype)
    {
        string[] allowed = { "Bool", "Byte", "Word", "DWord", "Int", "DInt", "Real" };
        if (!Array.Exists(allowed, d => d.Equals(datatype, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Unsupported Datatype: '{datatype}'");

        return datatype;
    }

    private bool IsCompatibleType(object value)
    {
        if (value == null) return false;

        return Datatype switch
        {
            "Bool" => value is bool,
            "Byte" => value is byte,
            "Word" => value is ushort,
            "DWord" => value is uint,
            "Int" => value is short,
            "DInt" => value is int,
            "Real" => value is float or double,
            _ => false
        };
    }

    private static DataType ParseAreaType(string address)
    {
        if (address.StartsWith("DB"))
            return DataType.DataBlock;
        if (address.StartsWith("M"))
            return DataType.Memory;
        if (address.StartsWith("I"))
            return DataType.Input;
        if (address.StartsWith("Q"))
            return DataType.Output;

        throw new ArgumentException($"Unknown address area: {address}");
    }

    private static VarType ParseVarType(string datatype)
    {
        var tagtype = datatype switch
        {
            "Bool" => VarType.Bit,
            "Byte" => VarType.Byte,
            "Word" => VarType.Word,
            "DWord" => VarType.DWord,
            "Int" => VarType.Int,
            "DInt" => VarType.DInt,
            "Real" => VarType.Real,
            _ => throw new ArgumentException($"Unsupported VarType: {datatype}")
        };
        return tagtype;
    }

    private static int ParseDbNumber(string address)
    {
        var match = Regex.Match(address, @"^DB(\d+)\.");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private static int ParseByteOffset(string address)
    {
        // Ejemplos válidos: DB1.DBD2, DB1.DBX0.0, DB1.DBW4, M10.1, IW2, QW4
        var dbMatch = Regex.Match(address, @"^DB(\d+)\.DB[XDW](\d+)");
        if (dbMatch.Success)
        {
            var byteStr = dbMatch.Groups[2].Value;
            return int.Parse(byteStr);
        }

        var areaMatch = Regex.Match(address, @"^[MIQ](B|W|D)?X?(\d+)(?:\.\d+)?$");
        if (areaMatch.Success)
        {
            var byteStr = areaMatch.Groups[2].Value;
            return int.Parse(byteStr);
        }

        throw new ArgumentException($"Invalid address format for byte offset: {address}");
    }


    private static byte ParseBitOffset(string address)
    {
        var match = Regex.Match(address, @"\.(\d+)$");
        return match.Success ? byte.Parse(match.Groups[1].Value) : (byte)0;
    }
}