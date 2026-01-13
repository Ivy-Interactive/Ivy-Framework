using System.Globalization;
using ExcelNumberFormat;

namespace Ivy.Helpers;

public static class Format
{
    public static string Number(string numberFormat, object value)
    {
        var currentCulture = CultureInfo.CurrentCulture;
        var format = new NumberFormat(numberFormat);
        return format.Format(value, currentCulture);
    }
}