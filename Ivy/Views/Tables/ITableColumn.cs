namespace Ivy.Views.Tables;

public interface ITableColumn<in TModel>
{
    public (TableCell header, TableCell[] cells) Build(IEnumerable<TModel> records);
}