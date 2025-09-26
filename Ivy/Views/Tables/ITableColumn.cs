namespace Ivy.Views.Tables;

/// <summary>
/// Interface for table columns that can generate header and data cells from model records.
/// </summary>
/// <typeparam name="TModel">The type of data model for the table rows.</typeparam>
public interface ITableColumn<in TModel>
{
    /// <summary>
    /// Builds the column header and data cells for the given records.
    /// </summary>
    /// <param name="records">The data records to generate cells from.</param>
    /// <returns>Tuple containing the header cell and array of data cells.</returns>
    public (TableCell header, TableCell[] cells) Build(IEnumerable<TModel> records);
}