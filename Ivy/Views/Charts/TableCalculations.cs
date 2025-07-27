namespace Ivy.Views.Charts;

public static class TableCalculations
{
    private static double Convert(object value) => (double?)Core.Utils.BestGuessConvert(value, typeof(double)) ?? 0;

    public static TableCalculation PercentOfTotal<T>(string measureName, string name = "PercentOfTotal")
    {
        return new TableCalculation(name, [measureName], rows =>
        {
            var total = rows.Sum(Convert);
            foreach (var row in rows)
            {
                row[name] = Convert(row[measureName]) / total;
            }
        });
    }

    public static TableCalculation RunningTotal(string measureName, string name = "RunningTotal")
    {
        return new TableCalculation(name, [measureName], rows =>
        {
            double total = 0;
            foreach (var row in rows)
            {
                total += Convert(row[measureName]);
                row[name] = total;
            }
        });
    }

    public static TableCalculation DifferenceFromPrevious(string measureName, string name = "DifferenceFromPrevious")
    {
        return new TableCalculation(name, [measureName], rows =>
        {
            double? previous = null;
            foreach (var row in rows)
            {
                double current = Convert(row[measureName]);
                row[name] = previous.HasValue ? current - previous.Value : 0;
                previous = current;
            }
        });
    }

    public static TableCalculation PercentChangeFromPrevious(string measureName, string name = "")
    {
        return new TableCalculation(name, [measureName], rows =>
        {
            double? previous = null;
            foreach (var row in rows)
            {
                double current = Convert(row[measureName]);
                row[name] = previous.HasValue
                    ? (previous.Value == 0 ? 0 : (current - previous.Value) / previous.Value)
                    : 0;
                previous = current;
            }
        });
    }

    public static TableCalculation MovingAverage(string measureName, int window, string name = "MovingAverage")
    {
        return new TableCalculation(name, [measureName], rows =>
        {
            var list = rows.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                double sum = 0;
                int count = 0;
                for (int j = Math.Max(0, i - window + 1); j <= i; j++)
                {
                    sum += Convert(list[j][measureName]);
                    count++;
                }
                list[i][name] = count > 0 ? sum / count : 0;
            }
        });
    }

    public static TableCalculation CumulativeAverage(string measureName, string name = "CumulativeAverage")
    {
        return new TableCalculation(name, [measureName], rows =>
        {
            double total = 0;
            int count = 0;
            foreach (var row in rows)
            {
                count++;
                total += Convert(row[measureName]);
                row[name] = total / count;
            }
        });
    }

    public static TableCalculation Rank(string measureName, string name = "Rank")
    {
        return new TableCalculation(name, [measureName], rows =>
        {
            var sorted = rows.OrderBy(r => Convert(r[measureName])).ToList();
            int rank = 1;
            foreach (var row in sorted)
                row[name] = rank++;
        });
    }

    public static TableCalculation PercentRank(string measureName, string name = "PercentRank")
    {
        return new TableCalculation(name, [measureName], rows =>
        {
            var sorted = rows.OrderBy(r => Convert(r[measureName])).ToList();
            int n = sorted.Count;
            for (int i = 0; i < n; i++)
                sorted[i][name] = n <= 1 ? 0 : (double)i / (n - 1);
        });
    }
}