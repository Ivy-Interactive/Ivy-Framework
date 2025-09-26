# Installation

<Ingress>
Getting started with Ivy.
</Ingress>

Ivy can be installed easily using the .NET command line tool.

## 1. Install Ivy Globally

Run the following command in your terminal to install Ivy as a global tool:

```terminal
>dotnet tool install -g Ivy.Console
```

<Callout Type="tip">
If you're using a specific operating system, read the instructions in your terminal after installing Ivy.Console.
You can always see all available commands by using `ivy --help`.
</Callout>

This will install Ivy globally on your machine. You can now use the `ivy` command in your terminal.

## 2. Create a New Ivy Project

Use the Ivy CLI to scaffold a new project:

```terminal
>ivy init --namespace Acme.InternalProject
>dotnet watch
```

For more terminal usage, check out the [CLI section](../03_CLI/01_Overview.md).
