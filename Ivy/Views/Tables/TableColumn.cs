using System.Linq.Expressions;
using Ivy.Views.Builders;

namespace Ivy.Views.Tables;

/// <summary>
/// Concrete implementation of a table column that extracts values from model objects using a selector expression.
/// </summary>
/// <typeparam name="TModel">The type of data model for the table rows.</typeparam>
/// <typeparam name="TValue">The type of value extracted by the selector expression.</typeparam>
/// <param name="selector">Expression that extracts the column value from each model object.</param>
/// <param name="headerText">The text to display in the column header.</param>
/// <param name="builder">Optional custom builder for rendering cell content. Uses DefaultBuilder if null.</param>
public class TableColumn<TModel, TValue>(Expression<Func<TModel, TValue>> selector, string headerText, IBuilder<TModel>? builder = null) : ITableColumn<TModel>
{
    /// <summary>
    /// Builds the column header and data cells by applying the selector to each record.
    /// </summary>
    /// <param name="records">The data records to generate cells from.</param>
    /// <returns>Tuple containing the header cell and array of data cells.</returns>
    public (TableCell header, TableCell[] cells) Build(IEnumerable<TModel> records)
    {
        IBuilder<TModel> actualBuilder = builder ?? new DefaultBuilder<TModel>();

        var header = new TableCell(headerText);

        var cells = records.Select(m => new TableCell(actualBuilder.Build(selector.Compile()(m), m))).ToArray();

        return (header, cells);
    }
}