using System;
using System.Text.RegularExpressions;
using S7.Net;
using S7.Net.Types;

namespace RecipesRepacoSayco.Plc.Models;

public class Tag
{
    public string Name { get; set; }
    public string Datatype { get; set; } // Ej: Bool, Real, Int
    public string Address { get; set; }  // Ej: DB10.DBX0.0, M0.0, IW2, QW4
    private object _value;

    public object Value
    {
        get => Item?.Value ?? _value;
        set
        {
            if (IsCompatibleType(value))
            {
                _value = value;
                if (Item != null)
                    Item.Value = value;
            }
            else
            {
                throw new InvalidCastException($"Value type '{value?.GetType().Name}' is not compatible with PLC type '{Datatype}'.");
            }
        }
    }
    public DataItem Item;

    public Tag(string name, string datatype, string address, object value)
    {
        Name = name;
        Datatype = datatype;
        Address = address;
        _value = value;
        Build();
        Value = value; // aplica validación y sincroniza con DataItem
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
        return datatype switch
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
    }

    private static int ParseDbNumber(string address)
    {
        var match = Regex.Match(address, @"^DB(\d+)\.");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private static int ParseByteOffset(string address)
    {
        var match = Regex.Match(address, @"(?:DB\d+\.)?[DMIQ](B|W|D)?X?(\d+)(?:\.\d+)?");
        if (match.Success && int.TryParse(match.Groups[2].Value, out int byteAddr))
            return byteAddr;

        throw new ArgumentException($"Invalid address format for byte offset: {address}");
    }

    private static byte ParseBitOffset(string address)
    {
        var match = Regex.Match(address, @"\.(\d+)$");
        return match.Success ? byte.Parse(match.Groups[1].Value) : (byte)0;
    }
}
