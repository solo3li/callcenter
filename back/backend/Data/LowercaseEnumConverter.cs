using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.RegularExpressions;

namespace backend.Data;

public class LowercaseEnumConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : struct, Enum
{
    public LowercaseEnumConverter() : base(
        v => ToSnakeCase(v.ToString()),
        v => (TEnum)Enum.Parse(typeof(TEnum), ToPascalCase(v), true))
    {}

    private static string ToSnakeCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return Regex.Replace(str, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }

    private static string ToPascalCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        var parts = str.Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
                parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1).ToLowerInvariant();
        }
        return string.Join("", parts);
    }
}
