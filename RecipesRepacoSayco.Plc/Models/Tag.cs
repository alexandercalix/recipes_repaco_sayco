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
    public int? Length { get; init; } = 1;

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

    public SiemensTag(string name, string datatype, string address, object value, int? length = null)
    {
        Name = name;
        Datatype = ValidateDatatype(datatype);
        Address = address;
        _value = value;
        Length = length;

        Build();
        Value = value; // triggers validation and sync
    }

    private void Build()
    {
        int count = 1;

        if (Datatype == "String")
        {
            count = Length ?? 128; // Default string length
        }


        Item = new DataItem
        {

            DataType = ParseAreaType(Address),
            VarType = ParseVarType(Datatype),
            DB = ParseDbNumber(Address),
            StartByteAdr = ParseByteOffset(Address),
            BitAdr = ParseBitOffset(Address),
            Count = count,
            Value = _value
        };


        Console.WriteLine($"SiemensTag {Name} {Item.DataType} {Item.VarType} {Item.StartByteAdr}");
    }

    private static string ValidateDatatype(string datatype)
    {
        string[] allowed = { "Bool", "Byte", "Word", "DWord", "Int", "DInt", "Real", "String" };
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
            "String" => value is string,

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
            "String" => VarType.String,
            _ => throw new ArgumentException($"Unsupported VarType: {datatype}")
        };
        return tagtype;
    }

    private static int ParseDbNumber(string address)
    {
        var match = Regex.Match(address, @"DB(\d+)", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int dbNumber))
        {
            return dbNumber;
        }

        throw new ArgumentException($"Invalid address format for DB number: {address}");
    }

    private static int ParseByteOffset(string address)
    {
        // Para DBX, DBB, DBW, DBD
        var match = Regex.Match(address, @"DB\d+\.DB[XBWD](\d+)(?:\.\d+)?", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int byteAddr))
        {
            Console.WriteLine($"Parsed byte offset: {byteAddr} from address: {address}");
            return byteAddr;
        }

        throw new ArgumentException($"Invalid address format for byte offset: {address}");
    }



    private static byte ParseBitOffset(string address)
    {
        var match = Regex.Match(address, @"\.(\d+)$");
        return match.Success ? byte.Parse(match.Groups[1].Value) : (byte)0;
    }
}