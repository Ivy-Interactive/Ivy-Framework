using System.Collections;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Ivy.Views.Charts;

/// <summary>
/// Represents a dimension (grouping column) in a pivot table with a name and selector expression.
/// </summary>
/// <typeparam name="T">The type of the source data objects.</typeparam>
/// <param name="Name">The display name for the dimension column.</param>
/// <param name="Selector">Expression to select the dimension value from source objects.</param>
public record Dimension<T>(string Name, Expression<Func<T, object>> Selector);

/// <summary>
/// Represents a measure (aggregated value) in a pivot table with a name and aggregation expression.
/// </summary>
/// <typeparam name="T">The type of the source data objects.</typeparam>
/// <param name="Name">The display name for the measure column.</param>
/// <param name="Aggregator">Expression to aggregate values from grouped data (e.g., Sum, Count, Average).</param>
public record Measure<T>(string Name, Expression<Func<IQueryable<T>, object>> Aggregator);

/// <summary>
/// Represents a table calculation that performs post-aggregation computations on pivot table results.
/// </summary>
/// <param name="Name">The name of the calculated column.</param>
/// <param name="MeasureNames">Array of measure names that this calculation depends on.</param>
/// <param name="Calculation">Action that performs the calculation on the pivot table data.</param>
public record TableCalculation(string Name, string[] MeasureNames, Action<List<Dictionary<string, object>>> Calculation);

/// <summary>
/// Core pivot table engine that transforms data by grouping dimensions and aggregating measures.
/// </summary>
/// <typeparam name="T">The type of the source data objects.</typeparam>
/// <remarks>
/// Supports 1-2 dimensions with multiple measures and table calculations. Groups data by dimensions,
/// applies measure aggregations to each group, executes table calculations, and sorts results by the first dimension.
/// </remarks>
public class PivotTable<T>
{
    /// <summary>Gets the collection of dimensions (grouping columns) for the pivot table.</summary>
    /// <value>A list of dimension configurations that define how data is grouped.</value>
    public IList<Dimension<T>> Dimensions { get; } = new List<Dimension<T>>();

    /// <summary>Gets the collection of measures (aggregated values) for the pivot table.</summary>
    /// <value>A list of measure configurations that define how grouped data is aggregated.</value>
    public IList<Measure<T>> Measures { get; } = new List<Measure<T>>();

    /// <summary>Gets the collection of table calculations for post-aggregation computations.</summary>
    /// <value>A list of table calculations that operate on the aggregated pivot table results.</value>
    public IList<TableCalculation> TableCalculations { get; } = new List<TableCalculation>();

    /// <summary>
    /// Initializes a new pivot table with the specified dimensions, measures, and optional table calculations.
    /// </summary>
    /// <param name="dimensions">The dimensions to group data by.</param>
    /// <param name="measures">The measures to aggregate for each group.</param>
    /// <param name="calculations">Optional table calculations for post-aggregation computations.</param>
    public PivotTable(IEnumerable<Dimension<T>> dimensions, IEnumerable<Measure<T>> measures, IEnumerable<TableCalculation>? calculations = null)
    {
        foreach (var d in dimensions)
            Dimensions.Add(d);
        foreach (var m in measures)
            Measures.Add(m);
        if (calculations != null)
        {
            foreach (var c in calculations)
                TableCalculations.Add(c);
        }
    }

    /// <summary>
    /// Executes the pivot table transformation asynchronously, grouping data by dimensions and aggregating measures.
    /// </summary>
    /// <param name="data">The source data to transform.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An array of dictionaries representing the pivot table rows with dimension and measure columns.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no dimensions are configured.</exception>
    /// <exception cref="NotSupportedException">Thrown when more than 2 dimensions are configured.</exception>
    public async Task<Dictionary<string, object>[]> ExecuteAsync(
        IQueryable<T> data,
        CancellationToken cancellationToken = default)
    {
        if (Dimensions.Count == 0)
            throw new InvalidOperationException("At least one dimension is required.");

        var result = new List<Dictionary<string, object>>();

        if (Dimensions.Count == 1)
        {
            Expression<Func<T, object>> keySelector = Dimensions[0].Selector;
            var grouped = data.GroupBy(keySelector);
            // Convert to list asynchronously
            var groups = await grouped.ToListAsync2(cancellationToken);

            foreach (var group in groups)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var row = new Dictionary<string, object>
                {
                    [Dimensions[0].Name] = group.Key
                };

                foreach (var measure in Measures)
                {
                    var aggregator = measure.Aggregator.Compile();
                    row[measure.Name] = aggregator(group.AsQueryable());
                }

                result.Add(row);
            }
        }
        else if (Dimensions.Count == 2)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var dimExpressions = Dimensions.Select(d =>
                    Expression.Convert(ReplaceParameter(d.Selector.Body, d.Selector.Parameters[0], param),
                        typeof(object)))
                .ToArray();

            var tupleConstructor =
                typeof(ValueTuple<object, object>).GetConstructor([typeof(object), typeof(object)]);
            var tupleExpr = Expression.New(tupleConstructor!, dimExpressions[0], dimExpressions[1]);
            var keySelectorLambda = Expression.Lambda<Func<T, ValueTuple<object, object>>>(tupleExpr, param);

            var grouped = data.GroupBy(keySelectorLambda);
            // Convert to list asynchronously
            var groups = await grouped.ToListAsync2(cancellationToken);

            foreach (var group in groups)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var row = new Dictionary<string, object>();
                var key = group.Key;
                row[Dimensions[0].Name] = key.Item1;
                row[Dimensions[1].Name] = key.Item2;

                foreach (var measure in Measures)
                {
                    var aggregator = measure.Aggregator.Compile();
                    row[measure.Name] = aggregator(group.AsQueryable());
                }

                result.Add(row);
            }
        }
        else
        {
            throw new NotSupportedException("Only 1 or 2 dimensions are supported in this example.");
        }

        foreach (var calculation in TableCalculations)
        {
            //var measureNames = calculation.MeasureNames;
            //todo: check if measure names exist
            calculation.Calculation(result);
        }

        //sort by first dimension
        result.Sort((a, b) => Comparer.Default.Compare(a[Dimensions[0].Name], b[Dimensions[0].Name]));

        return result.ToArray();
    }

    private static Expression ReplaceParameter(Expression expr, ParameterExpression oldParam,
        ParameterExpression newParam)
    {
        return new ParameterReplacer(oldParam, newParam).Visit(expr);
    }

    /// <summary>
    /// Expression visitor that replaces parameter references in expressions.
    /// </summary>
    private class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        /// <summary>
        /// Visits parameter expressions and replaces matching parameters.
        /// </summary>
        /// <param name="node">The parameter expression to visit.</param>
        /// <returns>The replacement parameter or the original if no match.</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == oldParam ? newParam : base.VisitParameter(node);
        }
    }
}

/// <summary>
/// Fluent builder for configuring and executing pivot table transformations.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
/// <remarks>
/// Provides a fluent API for building pivot tables with dimensions, measures, and table calculations.
/// Supports method chaining for easy configuration and can produce typed results or raw dictionaries.
/// </remarks>
public class PivotTableBuilder<TSource>(IQueryable<TSource> data)
{
    private List<Dimension<TSource>> _dimensions { get; } = new();
    private List<Measure<TSource>> _measures { get; } = new();
    private List<TableCalculation> _calculations { get; } = new();
    private IQueryable<TSource> Data { get; } = data;

    /// <summary>
    /// Adds a dimension (grouping column) to the pivot table with the specified name and selector.
    /// </summary>
    /// <param name="name">The display name for the dimension column.</param>
    /// <param name="selector">Expression to select the dimension value from source objects.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PivotTableBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        _dimensions.Add(new Dimension<TSource>(name, selector));
        return this;
    }

    /// <summary>
    /// Adds a pre-configured dimension to the pivot table.
    /// </summary>
    /// <param name="dimension">The dimension configuration to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PivotTableBuilder<TSource> Dimension(Dimension<TSource> dimension)
    {
        _dimensions.Add(dimension);
        return this;
    }

    /// <summary>
    /// Adds a measure (aggregated value) to the pivot table with the specified name and aggregator.
    /// </summary>
    /// <param name="name">The display name for the measure column.</param>
    /// <param name="aggregator">Expression to aggregate values from grouped data (e.g., Sum, Count, Average).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PivotTableBuilder<TSource> Measure(string name, Expression<Func<IQueryable<TSource>, object>> aggregator)
    {
        _measures.Add(new Measure<TSource>(name, aggregator));
        return this;
    }

    /// <summary>
    /// Adds a pre-configured measure to the pivot table.
    /// </summary>
    /// <param name="measure">The measure configuration to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PivotTableBuilder<TSource> Measure(Measure<TSource> measure)
    {
        _measures.Add(measure);
        return this;
    }

    /// <summary>
    /// Adds multiple measures to the pivot table.
    /// </summary>
    /// <param name="measure">The collection of measure configurations to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PivotTableBuilder<TSource> Measures(IEnumerable<Measure<TSource>> measure)
    {
        foreach (var m in measure)
            _measures.Add(m);
        return this;
    }

    /// <summary>
    /// Adds a table calculation for post-aggregation computations.
    /// </summary>
    /// <param name="calculation">The table calculation configuration to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PivotTableBuilder<TSource> TableCalculation(TableCalculation calculation)
    {
        _calculations.Add(calculation);
        return this;
    }

    /// <summary>
    /// Adds multiple table calculations for post-aggregation computations.
    /// </summary>
    /// <param name="calculations">The collection of table calculation configurations to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PivotTableBuilder<TSource> TableCalculations(IEnumerable<TableCalculation> calculations)
    {
        foreach (var c in calculations)
            _calculations.Add(c);
        return this;
    }

    /// <summary>
    /// Creates a mapper for producing typed results from the pivot table.
    /// </summary>
    /// <typeparam name="TDestination">The target type for the pivot table results.</typeparam>
    /// <returns>A mapper that can transform pivot table results to the specified type.</returns>
    public PivotTableMapper<TSource, TDestination> Produces<TDestination>()
    {
        return new PivotTableMapper<TSource, TDestination>(this);
    }

    /// <summary>
    /// Executes the pivot table transformation and returns raw dictionary results.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An array of dictionaries representing the pivot table rows.</returns>
    public Task<Dictionary<string, object>[]> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var pivotTable = new PivotTable<TSource>(_dimensions, _measures, _calculations);
        return pivotTable.ExecuteAsync(Data, cancellationToken);
    }
}

/// <summary>
/// Mapper for transforming pivot table results into strongly-typed objects.
/// </summary>
/// <typeparam name="TSource">The type of the source data objects.</typeparam>
/// <typeparam name="TDestination">The target type for the pivot table results.</typeparam>
/// <remarks>
/// Provides type-safe mapping from pivot table dictionary results to strongly-typed objects
/// using constructor parameter matching and automatic type conversion.
/// </remarks>
public class PivotTableMapper<TSource, TDestination>(PivotTableBuilder<TSource> builder)
{
    /// <summary>Gets the underlying pivot table builder.</summary>
    /// <value>The builder instance that configures the pivot table transformation.</value>
    public PivotTableBuilder<TSource> Builder { get; } = builder;

    /// <summary>
    /// Adds a dimension mapping from source expression to destination property.
    /// </summary>
    /// <param name="from">Expression to select the dimension value from source objects.</param>
    /// <param name="to">Expression indicating the destination property for the dimension.</param>
    /// <returns>The mapper instance for method chaining.</returns>
    public PivotTableMapper<TSource, TDestination> Dimension(Expression<Func<TSource, object>> from, Expression<Func<TDestination, object>> to)
    {
        Builder.Dimension(to.Body.ToString(), from);
        return this;
    }

    /// <summary>
    /// Adds a measure mapping from source aggregation to destination property.
    /// </summary>
    /// <param name="from">Expression to aggregate values from grouped data.</param>
    /// <param name="to">Expression indicating the destination property for the measure.</param>
    /// <returns>The mapper instance for method chaining.</returns>
    public PivotTableMapper<TSource, TDestination> Measure(Expression<Func<IQueryable<TSource>, object>> from, Expression<Func<TDestination, object>> to)
    {
        Builder.Measure(to.Body.ToString(), from);
        return this;
    }

    /// <summary>
    /// Adds a table calculation to the mapped pivot table.
    /// </summary>
    /// <param name="calculation">The table calculation configuration to add.</param>
    /// <returns>The mapper instance for method chaining.</returns>
    public PivotTableMapper<TSource, TDestination> TableCalculation(TableCalculation calculation)
    {
        Builder.TableCalculation(calculation);
        return this;
    }

    /// <summary>
    /// Executes the pivot table transformation and returns strongly-typed results.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async enumerable of strongly-typed objects created from the pivot table results.</returns>
    /// <exception cref="Exception">Thrown when required constructor parameters are missing from the pivot table results.</exception>
    public async IAsyncEnumerable<TDestination> ExecuteAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var rows = await Builder.ExecuteAsync(cancellationToken);
        foreach (var row in rows)
        {
            var targetType = typeof(TDestination);
            var ctor = targetType.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First();
            var parameters = ctor.GetParameters();

            var args = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (!row.TryGetValue(param.Name!, out var value))
                    throw new Exception($"Missing value for parameter '{param.Name}'");
                args[i] = Core.Utils.BestGuessConvert(value, param.ParameterType);
            }
            var item = (TDestination)ctor.Invoke(args);
            yield return item;
        }
    }
}

/// <summary>
/// Extension methods for creating pivot table builders from data collections.
/// </summary>
public static class PivotTableBuilderExtensions
{
    /// <summary>
    /// Creates a pivot table builder from an enumerable data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="data">The enumerable data source.</param>
    /// <returns>A PivotTableBuilder for configuring the pivot table transformation.</returns>
    public static PivotTableBuilder<TSource> ToPivotTable<TSource>(this IEnumerable<TSource> data)
    {
        return new PivotTableBuilder<TSource>(data.AsQueryable());
    }

    /// <summary>
    /// Creates a pivot table builder from a queryable data source.
    /// </summary>
    /// <typeparam name="TSource">The type of the source data objects.</typeparam>
    /// <param name="data">The queryable data source.</param>
    /// <returns>A PivotTableBuilder for configuring the pivot table transformation.</returns>
    [OverloadResolutionPriority(1)]
    public static PivotTableBuilder<TSource> ToPivotTable<TSource>(this IQueryable<TSource> data)
    {
        return new PivotTableBuilder<TSource>(data);
    }
}