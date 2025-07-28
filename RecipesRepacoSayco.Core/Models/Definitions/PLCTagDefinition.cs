using System;

namespace RecipesRepacoSayco.Core.Models.Definitions;

public class PlcTagDefinition
{


    public string Name { get; set; }          // Ej: "Tanque 1"
    public string Datatype { get; set; }      // Ej: "Real"
    public string Address { get; set; }       // Ej: "DB1.DBD2"

    public object DefaultValue { get; set; }  // Ej: 0, false, 0.0f, etc.

    public PlcTagDefinition() { }

    public PlcTagDefinition(string name, string datatype, string address, object defaultValue)
    {
        Name = name;
        Datatype = datatype;
        Address = address;
        DefaultValue = defaultValue;
    }

}
