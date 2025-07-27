using Ivy.Database.Generator.Toolkit.Databases;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Ivy.Database.Generator.Toolkit;

public class DatabaseGenerator
{
    public async Task<int> GenerateAsync(Type dataContextType, Type dataSeederType, bool verbose, bool yesToAll, string? projectDirectory, string? connectionString, DatabaseProvider? providerChoice)
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

        if (!yesToAll)
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
        }
        
        var dbContext = (DbContext)databaseProvider.GetType().GetMethod("GetDbContext")!.MakeGenericMethod(dataContextType).Invoke(databaseProvider,
            [connectionString])!;
        
        await Utils.WithSpinner(async () =>
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }, "Creating Tables", verbose);

        if (!yesToAll)
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
        }
        
        var seeder = (IDataSeeder)Activator.CreateInstance(dataSeederType, dbContext)!;
        
        await Utils.WithSpinner(async () =>
        {
            await seeder.SeedAsync();
        }, "Seeding", verbose);
        
        AnsiConsole.MarkupLine($"[green]Done![/]");

        return 0;
    }
}