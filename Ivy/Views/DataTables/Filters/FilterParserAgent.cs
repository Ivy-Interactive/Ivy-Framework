using Ivy.Protos.DataTable;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Ivy.Views.DataTables.Filters;

public class FilterParserAgent(IChatClient chatClient, ILogger<FilterParserAgent>? logger = null)
{
    public Task<DataTableFilterParserResponse> Parse(DataTableFilterParserRequest request)
    {
        throw new NotImplementedException();
    }
}