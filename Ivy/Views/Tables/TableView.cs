using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views.Tables;

/// <summary>
/// Renders a table from data models and column definitions by generating TableRow and TableCell components.
/// </summary>
/// <typeparam name="TModel">The type of data objects to display in table rows.</typeparam>
/// <param name="model">The collection of data objects to render as table rows.</param>
/// <param name="columns">The column definitions that specify how to extract and format data from each model object.</param>
public class TableView<TModel>(IEnumerable<TModel> model, params ITableColumn<TModel>[] columns) : ViewBase, IStateless
{
    /// <summary>
    /// Builds the table by creating a header row and data rows from the model and column definitions.
    /// </summary>
    /// <returns>A Table widget containing the header and all data rows.</returns>
    public override object? Build()
    {
        var header = new TableRow(columns.Select(c => c.Build(model).header).ToArray()).IsHeader();
        var rows = model.Select(m => new TableRow(columns.Select(c => c.Build(new[] { m }).cells[0]).ToArray())).ToArray();
        var joined = new[] { header }.Concat(rows).ToArray();
        return new Table(joined);
    }
}