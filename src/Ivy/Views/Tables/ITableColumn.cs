// ReSharper disable once CheckNamespace
namespace Ivy;

public interface ITableColumn<in TModel>
{
    public (TableCell header, TableCell[] cells) Build(IEnumerable<TModel> records);
}
