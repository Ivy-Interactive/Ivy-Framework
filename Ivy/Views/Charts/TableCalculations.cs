namespace Ivy.Views.Charts;

/// <summary>
/// Provides pre-built table calculations for common post-aggregation computations in pivot tables.
/// </summary>
/// <remarks>
/// Table calculations operate on pivot table results after dimensions have been grouped and measures aggregated.
/// They enable advanced analytics like percentages, running totals, moving averages, rankings, and trend analysis.
/// All calculations automatically handle type conversion and null values for robust data processing.
/// </remarks>
public static class TableCalculations
{
    private static double Convert(object value) => (double?)Core.Utils.BestGuessConvert(value, typeof(double)) ?? 0;

    /// <summary>
    /// Creates a table calculation that computes each row's value as a percentage of the total sum.
    /// </summary>
    /// <typeparam name="T">The type parameter (not used but maintains API consistency).</typeparam>
    /// <param name="measureName">The name of the measure column to calculate percentages for.</param>
    /// <param name="name">The name of the calculated column to create. Defaults to "PercentOfTotal".</param>
    /// <returns>A TableCalculation that adds a percentage-of-total column to the pivot table results.</returns>
    /// <remarks>
    /// Calculates each value as a proportion of the total sum across all rows.
    /// Useful for understanding relative contribution of each category to the overall total.
    /// Result values are decimals (e.g., 0.25 for 25%).
    /// </remarks>
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

    /// <summary>
    /// Creates a table calculation that computes a running total (cumulative sum) of the specified measure.
    /// </summary>
    /// <param name="measureName">The name of the measure column to calculate running totals for.</param>
    /// <param name="name">The name of the calculated column to create. Defaults to "RunningTotal".</param>
    /// <returns>A TableCalculation that adds a running total column to the pivot table results.</returns>
    /// <remarks>
    /// Calculates the cumulative sum from the first row to the current row.
    /// Useful for tracking progressive accumulation over time or ordered categories.
    /// Depends on the pivot table's sort order for meaningful results.
    /// </remarks>
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

    /// <summary>
    /// Creates a table calculation that computes the difference between each row's value and the previous row's value.
    /// </summary>
    /// <param name="measureName">The name of the measure column to calculate differences for.</param>
    /// <param name="name">The name of the calculated column to create. Defaults to "DifferenceFromPrevious".</param>
    /// <returns>A TableCalculation that adds a difference-from-previous column to the pivot table results.</returns>
    /// <remarks>
    /// Calculates the absolute difference between consecutive values in the sorted data.
    /// The first row receives a difference of 0 since there is no previous value.
    /// Useful for identifying period-over-period changes and trends.
    /// </remarks>
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

    /// <summary>
    /// Creates a table calculation that computes the percentage change between each row's value and the previous row's value.
    /// </summary>
    /// <param name="measureName">The name of the measure column to calculate percentage changes for.</param>
    /// <param name="name">The name of the calculated column to create. Defaults to empty string.</param>
    /// <returns>A TableCalculation that adds a percentage-change-from-previous column to the pivot table results.</returns>
    /// <remarks>
    /// Calculates the percentage change as (current - previous) / previous.
    /// The first row and rows following zero values receive a change of 0.
    /// Result values are decimals (e.g., 0.25 for 25% increase, -0.1 for 10% decrease).
    /// Handles division by zero gracefully by returning 0.
    /// </remarks>
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

    /// <summary>
    /// Creates a table calculation that computes a moving average over a specified window of rows.
    /// </summary>
    /// <param name="measureName">The name of the measure column to calculate moving averages for.</param>
    /// <param name="window">The number of rows to include in the moving average window.</param>
    /// <param name="name">The name of the calculated column to create. Defaults to "MovingAverage".</param>
    /// <returns>A TableCalculation that adds a moving average column to the pivot table results.</returns>
    /// <remarks>
    /// Calculates the average of the current row and the specified number of preceding rows.
    /// For early rows with fewer preceding values, uses all available rows up to the current position.
    /// Useful for smoothing out short-term fluctuations and identifying trends.
    /// Window size of 1 returns the original values, larger windows provide more smoothing.
    /// </remarks>
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

    /// <summary>
    /// Creates a table calculation that computes the cumulative average from the first row to the current row.
    /// </summary>
    /// <param name="measureName">The name of the measure column to calculate cumulative averages for.</param>
    /// <param name="name">The name of the calculated column to create. Defaults to "CumulativeAverage".</param>
    /// <returns>A TableCalculation that adds a cumulative average column to the pivot table results.</returns>
    /// <remarks>
    /// Calculates the average of all values from the first row through the current row.
    /// Unlike moving averages, the window size increases with each row.
    /// Useful for understanding long-term trends and overall performance over time.
    /// The first row's cumulative average equals its original value.
    /// </remarks>
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

    /// <summary>
    /// Creates a table calculation that assigns integer ranks to rows based on the specified measure values.
    /// </summary>
    /// <param name="measureName">The name of the measure column to rank by.</param>
    /// <param name="name">The name of the calculated column to create. Defaults to "Rank".</param>
    /// <returns>A TableCalculation that adds a rank column to the pivot table results.</returns>
    /// <remarks>
    /// Ranks rows from 1 (lowest value) to N (highest value) based on measure values.
    /// Rows are sorted by the measure in ascending order for ranking.
    /// Tied values receive different ranks based on their position in the sorted order.
    /// Useful for identifying top/bottom performers and creating leaderboards.
    /// </remarks>
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

    /// <summary>
    /// Creates a table calculation that assigns percentage ranks to rows based on the specified measure values.
    /// </summary>
    /// <param name="measureName">The name of the measure column to rank by.</param>
    /// <param name="name">The name of the calculated column to create. Defaults to "PercentRank".</param>
    /// <returns>A TableCalculation that adds a percentage rank column to the pivot table results.</returns>
    /// <remarks>
    /// Calculates percentage ranks as (rank - 1) / (total_count - 1), resulting in values from 0 to 1.
    /// The lowest value receives a rank of 0, the highest receives a rank of 1.
    /// For single-row datasets, all rows receive a rank of 0.
    /// Useful for understanding relative position within the dataset as a percentage.
    /// Result values are decimals (e.g., 0.75 means the value is at the 75th percentile).
    /// </remarks>
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