using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Agent.EfQuery;

internal static class SchemaCollector
{
    public static string CollectSchema(DbContext context)
    {
        var sb = new StringBuilder();

        foreach (var entityType in context.Model.GetEntityTypes())
        {
            var tableName = GetTableNameSafe(entityType);
            sb.AppendLine($"## {tableName}");

            foreach (var property in entityType.GetProperties())
            {
                var pk = property.IsPrimaryKey() ? " [PK]" : "";
                var nullable = property.IsNullable ? " (nullable)" : "";
                var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                sb.AppendLine($"- {property.Name}: {clrType.Name}{pk}{nullable}");
            }

            foreach (var fk in entityType.GetForeignKeys())
            {
                var principalTable = GetTableNameSafe(fk.PrincipalEntityType);
                var fkColumns = string.Join(", ", fk.Properties.Select(p => p.Name));
                sb.AppendLine($"- FK: {fkColumns} -> {principalTable}");
            }

            foreach (var nav in entityType.GetNavigations())
            {
                sb.AppendLine($"- Nav: {nav.Name} -> {nav.TargetEntityType.ClrType.Name}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetTableNameSafe(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType)
    {
        try
        {
            return entityType.GetTableName() ?? entityType.ClrType.Name;
        }
        catch
        {
            return entityType.ClrType.Name;
        }
    }
}
