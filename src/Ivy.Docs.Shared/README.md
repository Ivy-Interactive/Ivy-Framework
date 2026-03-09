# Welcome to the Ivy.Docs team

Initial preparations

Get a basic understanding of Ivy by trying it out. Follow the instructions on:

[Ivy Framework](https://github.com/Ivy-Interactive/Ivy-Framework)

Run `ivy samples` and review the code in `Ivy.Samples`.


The Markdown files are compiled to C#/Ivy during the build of the `Ivy.Docs` project. The generated C# files are created in the `Generated/` directory and are automatically regenerated when you run `dotnet build` on the `Ivy.Docs` project. The build process uses file hashing to skip regeneration if the markdown files haven't changed.

To force regenerate all the C# files (for example, if you want to ensure they're up to date), you can use the following scripts:

**Windows (PowerShell):**

```powershell
.\Regenerate.ps1
```

**Mac/Linux (Bash):**

```bash
sh ./Regenerate.sh
```

Clone [Ivy Framework](https://github.com/Ivy-Interactive/Ivy-Framework), then go to the `Ivy.Samples` folder and run `ivy run`. Navigate to the URL that is printed.

Any changes in the Markdown files are compiled and hot-reloaded as you write.

When writing code blocks, we have some custom syntax:

```csharp demo-below
new Button("Styled Button")
    .Icon(Icons.ArrowRight, Align.Right)
    .BorderRadius(BorderRadius.Full)
    .Large() 
```

This will show in the documentation as a code block with the Ivy demo below it.

Alternatives: `csharp demo-tabs` for tabbed code/demo blocks and `csharp` for a regular code block without any special handling. There is also `terminal` for terminal commands.

Note: the demo blocks need to be a single C# expression that returns a widget or a view. For more complex examples, you need to write a full Ivy view class.

See: [Basics](https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy.Docs/Docs/01_Onboarding/01_GettingStarted/03_Basics.md)  

## Notes

* Make sure your english is without spelling or grammar mistakes. Use Grammarly or similar tools to check your writing.
* It's ok to use AI tools to help you write, but make sure you review the output and ensure it is correct.
* All Ivy C# code needs to work!
* Use the `Ivy.Samples` project to learn how to use Ivy.
* Report any Ivy bugs on [Issues](https://github.com/Ivy-Interactive/Ivy-Framework/issues).

**Do not submit PRs if `Ivy.Samples` does not compile.**
