# Statistic

A content area to display primary and secondary number values with optional trend indicators, formatting, and prefix/suffix labels.

## Retool

```toolscript
statistic1.value = 12450
statistic1.label = "Revenue"
statistic1.prefix = "$"
statistic1.formattingStyle = "currency"
statistic1.currency = "USD"
statistic1.enableTrend = true
statistic1.signDisplay = "trendArrows"
statistic1.secondaryValue = 0.21
statistic1.secondaryFormattingStyle = "percent"
statistic1.secondaryEnableTrend = true
```

## Ivy

```csharp
new MetricView("Revenue", Icons.DollarSign, ctx =>
{
    var data = ctx.UseQuery(() => GetRevenueAsync());
    return data.Map(d => new MetricRecord(
        MetricFormatted: d.Current.ToString("C"),
        TrendComparedToPreviousPeriod: 0.21m,
        GoalAchieved: 0.85m,
        GoalFormatted: "$15,000"
    ));
});
```

## Parameters

| Parameter                    | Documentation                                          | Ivy                                                      |
|------------------------------|--------------------------------------------------------|----------------------------------------------------------|
| `value`                      | Primary numeric value                                  | `MetricRecord.MetricFormatted`                           |
| `label`                      | Text label to display                                  | `title` constructor parameter                            |
| `labelCaption`               | Additional text with label                             | Not supported                                            |
| `icon`                       | Icon to display                                        | `icon` constructor parameter (`Icons` enum)              |
| `prefix`                     | Prefix text (e.g., "$")                                | Include in `MetricFormatted` string                      |
| `suffix`                     | Suffix text (e.g., "USD")                              | Include in `MetricFormatted` string                      |
| `formattingStyle`            | Format: decimal, percent, currency                     | Format via .NET string formatting in `MetricFormatted`   |
| `currency`                   | Three-letter ISO currency code                         | Format via .NET `ToString("C")` with culture             |
| `decimalPlaces`              | Decimal places for primary value                       | Format via .NET format strings                           |
| `padDecimal`                 | Include trailing zeros                                 | Format via .NET format strings                           |
| `showSeparators`             | Show thousands separator                               | Format via .NET format strings (e.g., `"N2"`)            |
| `enableTrend`                | Color based on positive/negative value                 | `MetricRecord.TrendComparedToPreviousPeriod` (automatic) |
| `signDisplay`                | Sign display: trendArrows, exceptZero, auto            | Automatic trend arrows when trend value is set           |
| `secondaryValue`             | Secondary value display                                | Not supported (single metric only)                       |
| `secondaryPrefix`            | Secondary value prefix                                 | Not supported                                            |
| `secondarySuffix`            | Secondary value suffix                                 | Not supported                                            |
| `secondaryFormattingStyle`   | Secondary formatting                                   | Not supported                                            |
| `secondaryCurrency`          | Secondary currency code                                | Not supported                                            |
| `secondaryDecimalPlaces`     | Secondary decimal places                               | Not supported                                            |
| `secondaryPadDecimal`        | Secondary trailing zeros                               | Not supported                                            |
| `secondaryShowSeparators`    | Secondary thousands separator                          | Not supported                                            |
| `secondaryEnableTrend`       | Secondary trend coloring                               | Not supported                                            |
| `secondarySignDisplay`       | Secondary sign display                                 | Not supported                                            |
| `align`                      | Alignment: left, center, right                         | Not supported (use layout)                               |
| `clickable`                  | Enable click event handler                             | Not supported                                            |
| `hidden`                     | Hide component from view                               | `Visible` property                                       |
| `margin`                     | Margin spacing                                         | Not supported (use layout spacing)                       |
| N/A                          | N/A                                                    | `MetricRecord.GoalAchieved` (progress toward goal)       |
| N/A                          | N/A                                                    | `MetricRecord.GoalFormatted` (target goal text)          |
| N/A                          | N/A                                                    | Built-in loading/error states via `UseQuery`             |
