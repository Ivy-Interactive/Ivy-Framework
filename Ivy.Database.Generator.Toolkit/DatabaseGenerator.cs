using Ivy.Database.Generator.Toolkit.Databases;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Ivy.Database.Generator.Toolkit;

public class DatabaseGenerator
{
    public async Task<int> GenerateAsync(Type dataContextType, Type dataSeederType, bool verbose, bool yesToAll, bool deleteDatabase, bool seedDatabase, string? projectDirectory, 
        string? connectionString, DatabaseProvider? providerChoice)
    {
        providerChoice ??= AnsiConsole.Prompt(
            new SelectionPrompt<DatabaseProvider>()
                .Title("Select database provider:")
                .AddChoices(Enum.GetValues<DatabaseProvider>()));

        var databaseProvider = DatabaseProviderFactory.Create(providerChoice.Value);

        if (string.IsNullOrEmpty(connectionString))
        {
            var connectionStringPrompt = new TextPrompt<string>("Connection string:");
            var defaultConnectionString = DatabaseProviderFactory.Create(providerChoice.Value)
                .GetDefaultConnectionString(projectDirectory ?? "");

            if (defaultConnectionString != null)
            {
                connectionStringPrompt.DefaultValue(defaultConnectionString);
            }

            connectionString = AnsiConsole.Prompt(
                connectionStringPrompt
            ).Trim();
        }

        if (!deleteDatabase && !yesToAll)
        {
            var continuePrompt = AnsiConsole.Prompt(
                new ConfirmationPrompt("This will delete and recreate the database. Are you sure you want to continue?")
                {
                    DefaultValue = false
                }
            );
            
            if (!continuePrompt)
            {
                AnsiConsole.MarkupLine("[red]Aborted![/]");
                return 1;
            }
            deleteDatabase = true;
        }
        
        var dbContext = (DbContext)databaseProvider.GetType().GetMethod("GetDbContext")!.MakeGenericMethod(dataContextType).Invoke(databaseProvider,
            [connectionString])!;
        
        await Utils.WithSpinner(async () =>
        {
            if (deleteDatabase)
            {
                await dbContext.Database.EnsureDeletedAsync();
            }
            else
            {
                // Check if the database exists and has tables
                if (await dbContext.Database.CanConnectAsync())
                {
                    // Try to check if any entity sets have data
                    bool hasData = false;
                    foreach (var entityType in dbContext.Model.GetEntityTypes())
                    {
                        var clrType = entityType.ClrType;
                        var setMethod = dbContext.GetType().GetMethod("Set", Type.EmptyTypes)?.MakeGenericMethod(clrType);
                        if (setMethod != null)
                        {
                            dynamic? dbSet = setMethod.Invoke(dbContext, null);
                            try
                            {
                                // Try to count records - will fail if table doesn't exist
                                if (await dbSet?.AnyAsync()!)
                                {
                                    hasData = true;
                                    break;
                                }
                            }
                            catch
                            {
                                // Table doesn't exist, which is fine
                            }
                        }
                    }
                    
                    if (hasData)
                    {
                        throw new InvalidOperationException("Database is not empty.");
                    }
                }
            }
            await dbContext.Database.MigrateAsync();
        }, "Creating Tables", verbose);

        if (!seedDatabase && !yesToAll)
        {
            var seedPrompt = AnsiConsole.Prompt(
                new ConfirmationPrompt("Seed database with bogus data?")
                {
                    DefaultValue = false
                }
            );
        
            if (!seedPrompt)
            {
                return 0;
            }
            seedDatabase = true;
        }

        if (seedDatabase)
        {
            var seeder = (IDataSeeder)Activator.CreateInstance(dataSeederType, dbContext)!;
        
            await Utils.WithSpinner(async () =>
            {
                await seeder.SeedAsync();
            }, "Seeding", verbose);
        }
        
        AnsiConsole.MarkupLine($"[green]Done![/]");

        return 0;
    }
}