using System.Collections;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Ivy.Views.Charts;

public record Dimension<T>(string Name, Expression<Func<T, object>> Selector);
public record Measure<T>(string Name, Expression<Func<IQueryable<T>, object>> Aggregator);
public record TableCalculation(string Name, string[] MeasureNames, Action<List<Dictionary<string, object>>> Calculation);

public class PivotTable<T>
{
    public IList<Dimension<T>> Dimensions { get; } = new List<Dimension<T>>();
    public IList<Measure<T>> Measures { get; } = new List<Measure<T>>();
    public IList<TableCalculation> TableCalculations { get; } = new List<TableCalculation>();

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

    private class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == oldParam ? newParam : base.VisitParameter(node);
        }
    }
}

public class PivotTableBuilder<TSource>(IQueryable<TSource> data)
{
    private List<Dimension<TSource>> _dimensions { get; } = new();
    private List<Measure<TSource>> _measures { get; } = new();
    private List<TableCalculation> _calculations { get; } = new();
    private IQueryable<TSource> Data { get; } = data;

    public PivotTableBuilder<TSource> Dimension(string name, Expression<Func<TSource, object>> selector)
    {
        _dimensions.Add(new Dimension<TSource>(name, selector));
        return this;
    }

    public PivotTableBuilder<TSource> Dimension(Dimension<TSource> dimension)
    {
        _dimensions.Add(dimension);
        return this;
    }

    public PivotTableBuilder<TSource> Measure(string name, Expression<Func<IQueryable<TSource>, object>> aggregator)
    {
        _measures.Add(new Measure<TSource>(name, aggregator));
        return this;
    }

    public PivotTableBuilder<TSource> Measure(Measure<TSource> measure)
    {
        _measures.Add(measure);
        return this;
    }

    public PivotTableBuilder<TSource> Measures(IEnumerable<Measure<TSource>> measure)
    {
        foreach (var m in measure)
            _measures.Add(m);
        return this;
    }

    public PivotTableBuilder<TSource> TableCalculation(TableCalculation calculation)
    {
        _calculations.Add(calculation);
        return this;
    }

    public PivotTableBuilder<TSource> TableCalculations(IEnumerable<TableCalculation> calculations)
    {
        foreach (var c in calculations)
            _calculations.Add(c);
        return this;
    }

    public PivotTableMapper<TSource, TDestination> Produces<TDestination>()
    {
        return new PivotTableMapper<TSource, TDestination>(this);
    }

    public Task<Dictionary<string, object>[]> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var pivotTable = new PivotTable<TSource>(_dimensions, _measures, _calculations);
        return pivotTable.ExecuteAsync(Data, cancellationToken);
    }
}

public class PivotTableMapper<TSource, TDestination>(PivotTableBuilder<TSource> builder)
{
    public PivotTableBuilder<TSource> Builder { get; } = builder;

    public PivotTableMapper<TSource, TDestination> Dimension(Expression<Func<TSource, object>> from, Expression<Func<TDestination, object>> to)
    {
        Builder.Dimension(to.Body.ToString(), from);
        return this;
    }

    public PivotTableMapper<TSource, TDestination> Measure(Expression<Func<IQueryable<TSource>, object>> from, Expression<Func<TDestination, object>> to)
    {
        Builder.Measure(to.Body.ToString(), from);
        return this;
    }

    public PivotTableMapper<TSource, TDestination> TableCalculation(TableCalculation calculation)
    {
        Builder.TableCalculation(calculation);
        return this;
    }

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

public static class PivotTableBuilderExtensions
{
    public static PivotTableBuilder<TSource> ToPivotTable<TSource>(this IEnumerable<TSource> data)
    {
        return new PivotTableBuilder<TSource>(data.AsQueryable());
    }

    [OverloadResolutionPriority(1)]
    public static PivotTableBuilder<TSource> ToPivotTable<TSource>(this IQueryable<TSource> data)
    {
        return new PivotTableBuilder<TSource>(data);
    }
}