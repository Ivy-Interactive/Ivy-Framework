using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views.Tables;

/// <summary>
/// A view for rendering a simple static table from a collection of models and column definitions.
/// Constructs the table header and rows based on the provided data.
/// </summary>
public class TableView<TModel>(IEnumerable<TModel> model, params ITableColumn<TModel>[] columns) : ViewBase, IStateless
{
    public override object? Build()
    {
        var header = new TableRow(columns.Select(c => c.Build(model).header).ToArray()).IsHeader();
        var rows = model.Select(m => new TableRow(columns.Select(c => c.Build(new[] { m }).cells[0]).ToArray())).ToArray();
        var joined = new[] { header }.Concat(rows).ToArray();
        return new Table(joined);
    }
}