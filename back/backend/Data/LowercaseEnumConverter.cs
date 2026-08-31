using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace backend.Data;

public class LowercaseEnumConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : struct, Enum
{
    public LowercaseEnumConverter() : base(
        v => v.ToString().ToLowerInvariant(),
        v => Enum.Parse<TEnum>(v, true))
    {}
}
