---
name: ivy-convert-reflex
description: >
  Convert a Reflex (Python) application to an Ivy project. Use when the user wants to
  migrate from Reflex, convert a Reflex app, or build an Ivy app from Reflex Python
  source files. Handles .py files, folders, and GitHub URLs.
allowed-tools: Bash(dotnet:*) Bash(git:*) Bash(test:*) Bash(mkdir:*) Read Write Edit Glob Grep
effort: high
argument-hint: "[path or GitHub URL]"
---

# ivy-convert-reflex

Convert a Reflex application to an Ivy project.

## Reference Files

The [references/](references/) folder contains 94 reference files with Reflex-to-Ivy component mappings (one `.md` per `rx.*` component). Read the relevant reference files before implementing the conversion to understand how to map Reflex features to Ivy features.

## Step 1: Locate the Reflex Application

You need a path to a `.py` file, a folder, or a GitHub URL containing the Reflex application. Check if a value was provided via `$ARGUMENTS`. If not, ask the user to provide one.

- If it is a **GitHub URL** (starts with `https://github.com/`): Clone it to `.ivy/source/<repo-name>/` using `git clone <url> .ivy/source/<repo-name>/` and use that as the path going forward.
- If it is a **local path**: Use it directly.
- Verify the path exists with `test -f "<path>"` or `test -d "<path>"`.

## Step 2: Research the Reflex Application

Read all the `.py` files and build a mental model of all Reflex features used in the application (`rx.*` components, state classes, event handlers, etc.).

Use the reference files in [references/](references/) to learn how to map Reflex features to Ivy features.

Gather enough information to produce a complete conversion guide before proceeding to the next step.

## Step 3: Write the Conversion Guide

Write a summarized conversion guide that maps the Reflex features used in the application to Ivy features. The conversion guide should be structured in a way that makes it easy to follow when implementing the conversion. Use markdown formatting to make it clear and organized -- but be concise and token efficient.

Present the plan to the user for approval before proceeding.

## Step 4: Implementation

Identify if there are any connections (db, auth, api) that should be set up using the appropriate connection skill (e.g., `/ivy-create-db-connection` for databases, `/ivy-create-auth-connection` for auth, `/ivy-create-any-connection` for APIs).

Given the conversion guide from the previous step, implement the conversion of the Reflex application to an Ivy application. Use the conversion guide and the reference files to map Reflex features to Ivy features.
