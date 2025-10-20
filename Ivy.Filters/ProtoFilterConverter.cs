using Google.Protobuf.WellKnownTypes;

namespace Ivy.Filters;

/// <summary>
/// Converts FilterModel to Ivy.Protos.DataTable.Filter (protobuf format)
/// </summary>
public static class ProtoFilterConverter
{
    /// <summary>
    /// Converts a FilterModel to a protobuf Filter that can be used with QueryProcessor
    /// </summary>
    public static object ConvertToProtoFilter(FilterModel model)
    {
        // Create the protobuf Filter dynamically using reflection
        // This avoids adding a direct dependency on Ivy.Protos
        var filterType = System.Type.GetType("Ivy.Protos.DataTable.Filter, Ivy");
        if (filterType == null)
            throw new InvalidOperationException("Could not find Ivy.Protos.DataTable.Filter type");

        var filter = Activator.CreateInstance(filterType);
        if (filter == null)
            throw new InvalidOperationException("Could not create Filter instance");

        return model switch
        {
            GroupFilterModel group => CreateGroupFilter(group, filterType),
            FieldFilterModel field => CreateFieldFilter(field, filterType),
            _ => throw new ArgumentException($"Unknown filter model type: {model.GetType().Name}")
        };
    }

    private static object CreateGroupFilter(GroupFilterModel group, System.Type filterType)
    {
        var filterGroupType = System.Type.GetType("Ivy.Protos.DataTable.FilterGroup, Ivy");
        if (filterGroupType == null)
            throw new InvalidOperationException("Could not find FilterGroup type");

        var filterGroup = Activator.CreateInstance(filterGroupType);
        if (filterGroup == null)
            throw new InvalidOperationException("Could not create FilterGroup instance");

        // Set Op property
        var opProperty = filterGroupType.GetProperty("Op");
        var logicalOperatorType = filterGroupType.GetNestedType("Types")?.GetNestedType("LogicalOperator");
        if (opProperty == null || logicalOperatorType == null)
            throw new InvalidOperationException("Could not find FilterGroup.Op or LogicalOperator enum");

        var opValue = group.Type.ToUpperInvariant() == "AND"
            ? System.Enum.Parse(logicalOperatorType, "And")
            : System.Enum.Parse(logicalOperatorType, "Or");
        opProperty.SetValue(filterGroup, opValue);

        // Set Filters property (repeated field)
        var filtersProperty = filterGroupType.GetProperty("Filters");
        if (filtersProperty == null)
            throw new InvalidOperationException("Could not find FilterGroup.Filters property");

        var filters = filtersProperty.GetValue(filterGroup);
        var addMethod = filters?.GetType().GetMethod("Add", new[] { filterType });
        if (addMethod == null)
            throw new InvalidOperationException("Could not find Add method on Filters collection");

        foreach (var condition in group.Conditions)
        {
            var childFilter = ConvertToProtoFilter(condition);
            addMethod.Invoke(filters, new[] { childFilter });
        }

        // Create Filter with Group
        var filter = Activator.CreateInstance(filterType);
        var groupProperty = filterType.GetProperty("Group");
        groupProperty?.SetValue(filter, filterGroup);

        return filter!;
    }

    private static object CreateFieldFilter(FieldFilterModel field, System.Type filterType)
    {
        var conditionType = System.Type.GetType("Ivy.Protos.DataTable.Condition, Ivy");
        if (conditionType == null)
            throw new InvalidOperationException("Could not find Condition type");

        var condition = Activator.CreateInstance(conditionType);
        if (condition == null)
            throw new InvalidOperationException("Could not create Condition instance");

        // Set Column
        var columnProperty = conditionType.GetProperty("Column");
        columnProperty?.SetValue(condition, field.ColId);

        // Set Function
        var functionProperty = conditionType.GetProperty("Function");
        functionProperty?.SetValue(condition, MapFilterTypeToFunction(field.Type));

        // Set Args
        var argsProperty = conditionType.GetProperty("Args");
        if (argsProperty != null && field.Filter != null)
        {
            var args = argsProperty.GetValue(condition);
            var addMethod = args?.GetType().GetMethod("Add");

            if (addMethod != null)
            {
                // Add primary filter value
                var packedValue = PackValue(field.Filter);
                addMethod.Invoke(args, new[] { packedValue });

                // Add FilterTo if present (for range queries)
                if (field.FilterTo != null)
                {
                    var packedValueTo = PackValue(field.FilterTo);
                    addMethod.Invoke(args, new[] { packedValueTo });
                }
            }
        }

        // Create Filter with Condition
        var filter = Activator.CreateInstance(filterType);
        var conditionProperty = filterType.GetProperty("Condition");
        conditionProperty?.SetValue(filter, condition);

        return filter!;
    }

    private static string MapFilterTypeToFunction(string filterType)
    {
        return filterType.ToLowerInvariant() switch
        {
            "contains" => "contains",
            "notcontains" => "notcontains",
            "startswith" => "startswith",
            "endswith" => "endswith",
            "equals" => "equals",
            "notequal" => "notequals",
            "greaterthan" => "greaterthan",
            "greaterthanorequal" => "greaterthanorequal",
            "lessthan" => "lessthan",
            "lessthanorequal" => "lessthanorequal",
            "blank" => "blank",
            "notblank" => "notblank",
            _ => filterType.ToLowerInvariant()
        };
    }

    private static Any PackValue(object value)
    {
        return value switch
        {
            string str => Any.Pack(new StringValue { Value = str }),
            int i => Any.Pack(new Int32Value { Value = i }),
            long l => Any.Pack(new Int64Value { Value = l }),
            double d => Any.Pack(new DoubleValue { Value = d }),
            float f => Any.Pack(new FloatValue { Value = f }),
            bool b => Any.Pack(new BoolValue { Value = b }),
            _ => Any.Pack(new StringValue { Value = value.ToString() ?? "" })
        };
    }
}
