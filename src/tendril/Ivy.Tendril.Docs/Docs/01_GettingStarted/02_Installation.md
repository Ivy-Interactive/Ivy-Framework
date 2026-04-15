---
searchHints:
  - install
  - setup
  - prerequisites
  - getting-started
  - macos
  - windows
  - onboarding
  - tendril-home
icon: Download
---

# Installation

<Ingress>
Install Tendril on macOS, Linux, or Windows using one of the methods below.
</Ingress>

## Quick Install

One-liner: installs Tendril and required backend tools.

### macOS / Linux

```bash
curl -sSf https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.sh | sh
```

### Windows

```powershell
Invoke-RestMethod -Uri https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/main/src/tendril/install.ps1 | Invoke-Expression
```

### .NET Tool

Global install from NuGet:

```bash
dotnet tool install --g Ivy.Tendril
```

## Run

```bash
tendril
```

## Update

You can update Ivy Tendril at anytime after the initial install using the dotnet tool update command:

```bash
dotnet tool update --global Ivy.Tendril
```

## First run: onboarding

When configuration is missing, Tendril shows **`OnboardingApp`**—a stepper (`Welcome` – `Software` – `Coding agent` – `Storage` – `Project` – `Complete`) defined in the onboarding apps. Below are the two steps that define **where data lives** and **how your first project is wired**.

### Tendril Home (storage)

Choose the directory that becomes **`TENDRIL_HOME`**. It stores **`config.yaml`**, **Plans**, **Inbox**, **Trash**, **logs** under each plan, and everything else Tendril writes to disk. Defaults to `TENDRIL_HOME` if set, otherwise **`~/.tendril`**; you can enter any path.

![Onboarding: Tendril Data Location — pick folder for plans, inbox, trash, and config](/tendril-docs/assets/onboarding-tendril-home.png "Tendril Home — where logs and config.yaml live")

### Project setup

Add your **first project**: a **name**, optional **context**, at least one **repository path** on disk (this is the repo Tendril uses for work and where **`MakePr` opens pull requests**). Add **verifications** with short names and prompts—the agent runs these checks during execution.

All of this is written to **`TENDRIL_HOME/config.yaml`** when you finish onboarding (and you can edit the same file or use **Settings** later).

![Onboarding: Project Setup — name, repos for PRs, verification prompts saved to config.yaml](/tendril-docs/assets/onboarding-project-setup.png "Project setup — repository paths and verifications stored under Tendril Home")

After onboarding, use **[Setup & Settings](../03_Configuration/01_Setup.md)** to add projects, repos, and verification definitions without re-running the wizard.
