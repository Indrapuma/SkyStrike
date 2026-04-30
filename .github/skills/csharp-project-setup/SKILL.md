---
name: csharp-project-setup
description: 'Set up or repair a C# project in VS Code. Use for .NET SDK checks, C# extension setup, solution and project creation, restore/build/run/test workflows, and optional game-project bootstrapping when explicitly requested.'
argument-hint: 'Describe the project type, target framework, and whether this is a new or existing C# project.'
user-invocable: true
---

# C# Project Setup

## What This Skill Produces

This skill guides the agent through setting up or stabilizing a C# workspace in VS Code.

Use it to:
- Verify that the required .NET SDK is installed
- Check that VS Code has the expected C# tooling available
- Create or organize a solution and project structure
- Restore dependencies and validate build, run, and test commands
- Handle a game-oriented C# project only when the user asks for that branch

## When to Use

Use this skill when the user asks to:
- set up C#
- set up .NET in VS Code
- create a C# project or solution
- fix a broken C# environment
- prepare a C# game project
- bootstrap MonoGame or a similar desktop game workflow

## Inputs To Confirm

Before changing files, confirm or infer these inputs:
- Is this a new project or an existing one?
- Which target framework is expected, such as .NET 8 or .NET 9?
- Is this general C# application work or game development?
- If it is a game project, should the workflow stay plain .NET or use a framework such as MonoGame?

If the user does not specify, default to:
- Workspace-scoped setup
- Current LTS .NET SDK already available on the machine
- A standard solution plus one executable project
- A plain C# project unless the workspace or prompt clearly indicates game-framework setup

## Procedure

1. Inspect the workspace root.
   - Determine whether a solution, project file, or existing source tree is already present.
   - Prefer extending the current structure instead of replacing it.

2. Check the local .NET toolchain.
   - Run `dotnet --info`.
   - If the SDK is missing, stop and tell the user that the .NET SDK must be installed before the setup can be completed.
   - If the SDK is present but the requested target is unavailable, either use an installed compatible target or ask the user which SDK they want.

3. Check VS Code C# tooling expectations.
   - Prefer the C# and C# Dev Kit extensions for standard .NET work.
   - If extension installation is not available through tools, state the exact extensions to install and continue with file-based setup.

4. Branch on project state.

### Existing Project Path

If `.sln` or `.csproj` files already exist:
- Restore dependencies with `dotnet restore`
- Build with `dotnet build`
- If tests exist, run `dotnet test`
- If a runnable app exists, identify the correct startup project and run it with `dotnet run --project <path>`
- Fix only setup-related issues that block restore, build, run, or test

### New Project Path

If the workspace is empty or lacks a C# project:
- Create a solution with `dotnet new sln -n <solution-name>`
- Choose the project template from the user goal:
  - General app: `dotnet new console -n <project-name>`
  - Library: `dotnet new classlib -n <project-name>`
  - Test project: `dotnet new xunit -n <project-name>.Tests`
- Add projects to the solution with `dotnet sln <solution>.sln add <project>.csproj`
- Restore, build, and run the smallest working slice before adding more structure

### Game Project Branch

If the user wants a game-oriented C# setup:
- Check whether they want plain C# scaffolding or a game framework
- For plain C# setup, use the New Project Path and keep the project framework-neutral
- For MonoGame-style setup:
  - Verify whether the MonoGame templates are installed
  - If they are not installed, explain that the template package must be installed before generation
  - Create the project with the appropriate MonoGame template once available
  - Restore and build immediately after generation to catch missing native or graphics dependencies early

5. Validate the result immediately after setup.
   - `dotnet restore`
   - `dotnet build`
   - `dotnet test` if tests exist
   - `dotnet run` for runnable applications

6. Summarize the working entry points.
   - State the main solution or project path
   - State the command to build
   - State the command to run
   - State the command to test, if applicable

## Decision Rules

- If the workspace already contains a `.csproj`, do not generate a new solution unless the current structure is clearly incomplete.
- If the user asks for setup only, avoid adding extra architectural layers.
- If restore fails because of missing SDK workloads or templates, report the exact missing dependency before making broader changes.
- If the repository is empty, create the smallest viable solution and project first, then validate.
- If the project is for a game but the framework is unspecified, ask whether the user wants plain .NET scaffolding or MonoGame before adding framework-specific files.

## Completion Checks

The setup is complete only when all relevant checks pass:
- The workspace contains a valid `.sln` or a clear single-project structure
- The main project restores successfully
- The project builds successfully
- The runnable project starts, if one exists
- Tests pass, if tests exist
- The user has the exact commands needed to continue development

## Response Pattern

When using this skill, respond with:
1. The detected project state
2. The setup path chosen
3. The concrete commands or files created
4. The validation results
5. Any remaining dependency the user must install manually