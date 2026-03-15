using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Agent.EfQuery;

internal static class SchemaCollector
{
    private static string? _cachedSchema;

    public static string CollectSchema(DbContext context)
    {
        if (_cachedSchema != null)
            return _cachedSchema;

        var sb = new StringBuilder();

        // Database dialect header
        var providerName = context.Database.ProviderName;
        if (providerName != null)
        {
            var dialect = GetDialectName(providerName);
            sb.AppendLine($"Database: {dialect}");
            sb.AppendLine();
        }

        foreach (var entityType in context.Model.GetEntityTypes())
        {
            // Skip entities marked with [EfQueryIgnore]
            if (entityType.ClrType.GetCustomAttribute<EfQueryIgnoreAttribute>() != null)
                continue;

            var tableName = GetTableNameSafe(entityType);
            sb.Append($"## {tableName}");

            // Entity description
            var entityDesc = entityType.ClrType.GetCustomAttribute<EfQueryDescriptionAttribute>();
            if (entityDesc != null)
                sb.Append($"  DESCRIPTION: {entityDesc.Description}");

            sb.AppendLine();

            // TPH inheritance: discriminator info
            var discriminatorProperty = entityType.FindDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                var discriminatorValue = entityType.GetDiscriminatorValue();
                sb.AppendLine($"- Discriminator: {discriminatorProperty.Name} = {discriminatorValue}");
            }

            // Properties
            foreach (var property in entityType.GetProperties())
            {
                // Skip properties marked with [EfQueryIgnore]
                var propertyInfo = entityType.ClrType.GetProperty(property.Name);
                if (propertyInfo?.GetCustomAttribute<EfQueryIgnoreAttribute>() != null)
                    continue;

                var pk = property.IsPrimaryKey() ? " [PK]" : "";
                var nullable = property.IsNullable ? " (nullable)" : "";
                var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                sb.Append($"- {property.Name}: {clrType.Name}{pk}{nullable}");

                // Property description
                if (propertyInfo?.GetCustomAttribute<EfQueryDescriptionAttribute>() is { } propDesc)
                    sb.Append($"  DESCRIPTION: {propDesc.Description}");

                // Enum values
                if (clrType.IsEnum)
                {
                    var values = Enum.GetValues(clrType);
                    var mappings = new List<string>();
                    foreach (var val in values)
                        mappings.Add($"{Convert.ToInt32(val)} = {val}");
                    sb.Append($"  ENUM VALUES: {string.Join(", ", mappings)}");
                }

                sb.AppendLine();
            }

            // Owned entity navigations
            foreach (var nav in entityType.GetNavigations().Where(n => n.TargetEntityType.IsOwned()))
            {
                sb.AppendLine($"- Owned: {nav.Name}");
                foreach (var ownedProp in nav.TargetEntityType.GetProperties())
                {
                    var ownedClrType = Nullable.GetUnderlyingType(ownedProp.ClrType) ?? ownedProp.ClrType;
                    var ownedNullable = ownedProp.IsNullable ? " (nullable)" : "";
                    sb.AppendLine($"  - {ownedProp.Name}: {ownedClrType.Name}{ownedNullable}");
                }
            }

            // Foreign keys
            foreach (var fk in entityType.GetForeignKeys())
            {
                var principalTable = GetTableNameSafe(fk.PrincipalEntityType);
                var fkColumns = string.Join(", ", fk.Properties.Select(p => p.Name));
                sb.AppendLine($"- FK: {fkColumns} -> {principalTable}");
            }

            // Non-owned navigations
            foreach (var nav in entityType.GetNavigations().Where(n => !n.TargetEntityType.IsOwned()))
            {
                sb.AppendLine($"- Nav: {nav.Name} -> {nav.TargetEntityType.ClrType.Name}");
            }

            sb.AppendLine();
        }

        _cachedSchema = sb.ToString();
        return _cachedSchema;
    }

    /// <summary>
    /// Clears the cached schema. Primarily for testing.
    /// </summary>
    internal static void ClearCache() => _cachedSchema = null;

    private static string GetDialectName(string providerName) =>
        providerName switch
        {
            _ when providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) => "SQL Server",
            _ when providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) => "SQLite",
            _ when providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) => "PostgreSQL",
            _ when providerName.Contains("Pomelo", StringComparison.OrdinalIgnoreCase) => "MySQL",
            _ when providerName.Contains("MySql", StringComparison.OrdinalIgnoreCase) => "MySQL",
            _ when providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase) => "InMemory",
            _ => providerName
        };

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
