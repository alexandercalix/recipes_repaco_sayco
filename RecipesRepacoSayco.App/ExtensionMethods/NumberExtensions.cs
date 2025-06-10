using System;

namespace RecipesRepacoSayco.App.ExtensionMethods;

public static class NumberExtensions
{
    // Round any number to N decimal places (default 2)
    public static string RoundTo(this object value, int digits = 2)
    {
        if (value == null)
            return string.Empty;

        try
        {
            var dec = Convert.ToDecimal(value);
            return Math.Round(dec, digits).ToString($"F{digits}");
        }
        catch
        {
            return value.ToString();
        }
    }

    // Add prefix
    public static string WithPrefix(this string value, string prefix)
    {
        return string.IsNullOrEmpty(value) ? value : $"{prefix}{value}";
    }

    // Add suffix
    public static string WithSuffix(this string value, string suffix)
    {
        return string.IsNullOrEmpty(value) ? value : $"{value}{suffix}";
    }
}

