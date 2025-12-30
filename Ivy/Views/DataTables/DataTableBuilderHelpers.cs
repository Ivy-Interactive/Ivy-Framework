using Ivy.Shared;

namespace Ivy.Views.DataTables;

public static class DataTableBuilderHelpers
{
    public static ColType GetDataTypeHint(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(Icons))
            return ColType.Icon;

        if (underlyingType == typeof(string) || underlyingType == typeof(char))
            return ColType.Text;

        if (underlyingType == typeof(int) || underlyingType == typeof(long) ||
            underlyingType == typeof(short) || underlyingType == typeof(byte) ||
            underlyingType == typeof(uint) || underlyingType == typeof(ulong) ||
            underlyingType == typeof(ushort) || underlyingType == typeof(sbyte) ||
            underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
            underlyingType == typeof(float))
            return ColType.Number;

        if (underlyingType == typeof(bool))
            return ColType.Boolean;

        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
            return ColType.DateTime;

        if (underlyingType == typeof(DateOnly))
            return ColType.Date;

        if (underlyingType == typeof(TimeSpan) || underlyingType == typeof(TimeOnly) ||
            underlyingType == typeof(Guid) || underlyingType.IsEnum)
            return ColType.Text;

        // Handle string arrays as Labels type
        if (underlyingType.IsArray && underlyingType.GetElementType() == typeof(string))
            return ColType.Labels;

        // Handle other arrays and collections as Text
        if (underlyingType.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(underlyingType))
            return ColType.Text;

        return ColType.Text;
    }
}