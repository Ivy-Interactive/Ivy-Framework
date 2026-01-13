using Spectre.Console;

namespace Ivy.Database.Generator.Toolkit;

public static class Utils
{
    public static async Task<T> WithSpinner<T>(Func<Task<T>> task, string title, bool verbose)
    {
        if (verbose)
        {
            return await task();
        }
        
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Default)
            .StartAsync(title, 
                async _ => await task()
            );
    }
    
    public static async Task WithSpinner(Func<Task> task, string title, bool verbose)
    {
        if (verbose)
        {
            AnsiConsole.MarkupLine($"[yellow]{title}[/]");
            await task();
            return;
        }
        
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Default)
            .StartAsync(title, 
                async _ => await task()
            );
    }
}