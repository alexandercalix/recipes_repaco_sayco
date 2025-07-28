namespace RecipesRepacoSayco.Core.Models.Definitions;

public class PlcConnectionDefinition
{
    public string Name { get; set; }          // Ej: "PLC_Main", "PLC_Remoto"
    public string Driver { get; set; }        // Ej: "Siemens", "Modbus", "AllenBradley"
    public string IpAddress { get; set; }     // Ej: "192.168.0.10"
    public int Rack { get; set; } = 0;
    public int Slot { get; set; } = 1;
    public List<PlcTagDefinition> Tags { get; set; } = new();
}
