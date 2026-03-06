using Ivy.Shared;
using Ivy.Views;

namespace Ivy.Views.Builders;

public class ProgressBuilder<TModel> : IBuilder<TModel>
{
    private double min = 0;
    private double max = 100;
    private Colors? color = null;
    private string? format = null;
    private bool autoColor = false;

    public ProgressBuilder<TModel> Min(double value)
    {
        min = value;
        return this;
    }

    public ProgressBuilder<TModel> Max(double value)
    {
        max = value;
        return this;
    }

    public ProgressBuilder<TModel> Color(Colors value)
    {
        color = value;
        return this;
    }

    public ProgressBuilder<TModel> Format(string value)
    {
        format = value;
        return this;
    }

    public ProgressBuilder<TModel> AutoColor(bool value = true)
    {
        autoColor = value;
        return this;
    }

    public object? Build(object? value, TModel record)
    {
        if (value == null)
        {
            return null;
        }

        double numericValue;
        try
        {
            numericValue = Convert.ToDouble(value);
        }
        catch
        {
            return null;
        }

        var percentage = CalculatePercentage(numericValue);
        var progressColor = DetermineColor(percentage);
        var progress = new Progress((int)Math.Round(percentage)).Color(progressColor);

        if (format != null)
        {
            var formattedValue = FormatValue(numericValue);
            return Layout.Horizontal().Gap(2).Align(Align.Center)
                   | progress.Width(Size.Full())
                   | Text.Muted(formattedValue);
        }

        return progress;
    }

    private double CalculatePercentage(double value)
    {
        if (max == min)
        {
            return value >= max ? 100 : 0;
        }

        var percentage = (value - min) / (max - min) * 100;
        return Math.Clamp(percentage, 0, 100);
    }

    private Colors? DetermineColor(double percentage)
    {
        if (color != null)
        {
            return color;
        }

        if (!autoColor)
        {
            return null;
        }

        return percentage switch
        {
            >= 75 => Colors.Success,
            >= 50 => Colors.Warning,
            >= 25 => Colors.Orange,
            _ => Colors.Destructive
        };
    }

    private string FormatValue(double value)
    {
        if (format == null)
        {
            return value.ToString();
        }

        if (format.Contains("%f"))
        {
            return format.Replace("%f", value.ToString("F2"));
        }

        if (format.Contains("%d"))
        {
            return format.Replace("%d", ((int)value).ToString());
        }

        return string.Format(format, value);
    }
}
