using System.Linq.Expressions;
using Ivy.Builders;

namespace Ivy.Views.Tables;

public class TableColumn<TModel, TValue>(Expression<Func<TModel, TValue>> selector, string headerText, IBuilder<TModel>? builder = null) : ITableColumn<TModel>
{
    public (TableCell header, TableCell[] cells) Build(IEnumerable<TModel> records)
    {
        IBuilder<TModel> actualBuilder = builder ?? new DefaultBuilder<TModel>();

        var header = new TableCell(headerText);

        var cells = records.Select(m => new TableCell(actualBuilder.Build(selector.Compile()(m), m))).ToArray();

        return (header, cells);
    }
}