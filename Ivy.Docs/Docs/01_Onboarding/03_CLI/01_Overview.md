# Overview

The `Ivy` CLI is the production-ready tool that is used to create and manage projects.

To get started, let's install the CLI using the dotnet command line tool:

```terminal
> dotnet tool install -g Ivy.Console
```

This command will install `Ivy` globally and on the machine and we can now use the `ivy` command in our terminal.

To run a command, we use `ivy` and pass the command, subcommand(s) and or any other options as argument:

```terminal
> ivy [command] [subcommand] --[options]
```

Let's initialize a new project:

```terminal
> ivy init --namespace TodoApp
```

Here we use the `init` command to initialize a new project and passed `--namespace` option to specify the namespace to use for the project.

Next, let's explore the all the commands available in our CLI.

### [init](02_Init.md)

The [`init`](02_Init.md) command is used to create a new project. More details can be found here.

### [auth](04_Auth.md)

The [`auth`](04_Auth.md) command allows us to manage all [Auth providers](04_Auth.md) supported by the Ivy Framework.

### [db](03_Db.md)

The [`db`](03_Db.md) command is used to create and manage [databases](03_Db.md) supported by the Ivy Framework.

### [deploy](05_Deploy.md)

We use the [`deploy`](05_Deploy.md) command to deploy our project.

### app

The `app` command is used to create or modify an `Ivy` app using the `Ivy` agent.

### docs

We use the `docs` command to view Ivy documentation.

### samples

The `samples` command is used to open Ivy Samples.

### version

This command displays the current Ivy version.

### update

The `update` command is used to **update Ivy** to the latest version. This command updates our `Ivy` installation to the latest version.

### upgrade

The `ugrade` command is used to **ugrade the project** to the latest version. This command updates our **Ivy project** to the latest version.

### fix

The `fix` command is used to fix issues in our project.

### connect

We use the `connect` command to manage all connections.

### login

The `login` comman is used to Authenticate with Ivy.

### logout

The logout command removes stored Authentication token.
