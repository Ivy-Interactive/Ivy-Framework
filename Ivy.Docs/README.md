# Welcome to the Ivy.Docs team!

Initial preparations

Get a basic understanding of Ivy by trying it out. Follow the instructions on:

https://github.com/Ivy-Interactive/Ivy-Framework

Run `ivy samples` and review the code in `Ivy.Samples`.

## Instructions

We write our documentation in Markdown. Work is tracked in this Google Sheet:

https://docs.google.com/spreadsheets/d/1d3QWH53ayadDYPu4kl3Bq7F5ZElVfzqIAv02C7vE9-E/edit?usp=sharing
(you will need to request write access)

You are free to work on anything with status `Todo`. Submit a PR for each Markdown document. When you have submitted a PR set status `PR Submitted` and also enter the URL to the PR in the `PR` column. While working on a document, set the status to `In Progress`. When you are done with a document, set the status to `Finished` and enter the URL to the PR in the `PR` column.

**Review the pages marked as `Finished` to get an idea of what is expected of you.**

The Markdown files are compiled to C#/Ivy during the build of the `Ivy.Docs` project.

Clone https://github.com/Ivy-Interactive/Ivy-Framework, then go to the `Ivy.Samples` folder and run `dotnet watch`. Navigate to the URL that is printed.

Any changes in the Markdown files are compiled and hot-reloaded as you write.

When writing code blocks, we have some custom syntax:

````
```csharp demo-below
new Button("Styled Button")
    .Icon(Icons.ArrowRight, Align.Right)
    .BorderRadius(BorderRadius.Full)
    .Large() 
```
````

This will show in the documentation as a code block with the Ivy demo below it.

Alternatives: `csharp demo-tabs` for tabbed code/demo blocks and `csharp` for a regular code block without any special handling. There is also `terminal` for terminal commands.

Note: the demo blocks need to be a single C# expression that returns a widget or a view. For more complex examples, you need to write a full Ivy view class. 

See: https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy.Docs/Docs/01_Onboarding/01_GettingStarted/03_Basics.md  

## Notes

* Make sure your english is without spelling or grammar mistakes. Use Grammarly or similar tools to check your writing.
* It's ok to use AI tools to help you write, but make sure you review the output and ensure it is correct.
* All Ivy C# code needs to work!
* Use the `Ivy.Samples` project to learn how to use Ivy.
* Report any Ivy bugs on https://github.com/Ivy-Interactive/Ivy-Framework/issues. 

**Do not submit PRs if `Ivy.Samples` does not compile.**
