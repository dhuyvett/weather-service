# weather-service <!-- omit in toc -->

A sample microservice that illustrates techniques for building a production ready .NET 7.0 service.

## Table of Contents <!-- omit in toc -->

- [Project Setup](#project-setup)
  - [pre-commit Configuration](#pre-commit-configuration)
  - [,editorconfig File](#editorconfig-file)
  - [.gitignore File](#gitignore-file)
  - [Solution File](#solution-file)
  - [Service Project](#service-project)

## Project Setup

Starting from a basic git repository with `README.md` and `LICENSE` files, add the following.

### pre-commit Configuration

[pre-commit](https://pre-commit.com/) is a tool for keeping the code that gets committed clean and compliant with project standards.
`pre-commit` is installed and run on the developers' workstations with a configuration that gets checked in to [SCM](https://www.atlassian.com/git/tutorials/source-code-management).
Unfortunately, that means that the configured rules are only enforced if the developer chooses to install `pre-commit`.
It can, however, be integrated into the [CICD](https://en.wikipedia.org/wiki/CI/CD) workflow to prevent non-complying code from being merged into a shared branch.

`pre-commit` has a [wide assortment of hooks](https://pre-commit.com/hooks.html) to enforce various standards.
The hooks you choose will define the shared standards for your project.
For now, you can start with some basic hooks.

Install `pre-commit` as [specified](https://pre-commit.com/#installation).

The config file should be in the root of your git repository and must be named exactly: `.pre-commit-config.yaml`.
If you use the `.yml` extension, `pre-commit` will not recognize the file.

```yaml
repos:
  - repo: https://github.com/pre-commit/pre-commit-hooks
    rev: v4.4.0
    hooks:
      - id: check-yaml
      - id: end-of-file-fixer
      - id: trailing-whitespace
      - id: fix-byte-order-marker
      - id: check-json
      - id: check-merge-conflict
      - id: check-xml
      - id: mixed-line-ending
  - repo: https://github.com/zricethezav/gitleaks
    rev: v8.16.0
    hooks:
      - id: gitleaks
```

Once your `.pre-commit-config.yaml` file exists, you can configure the git webhooks (which confusingly uses the `install` command) and `pre-commit` will run before each commit.

```sh
~/src/weather-service (project-setup)$ pre-commit install
pre-commit installed at .git/hooks/pre-commit
~/src/weather-service (project-setup)$ git add .pre-commit-config.yaml
~/src/weather-service (project-setup)$ git commit -m "added pre-commit config"
check yaml...............................................................Passed
fix end of files.........................................................Passed
trim trailing whitespace.................................................Passed
fix utf-8 byte order marker..............................................Passed
check json...........................................(no files to check)Skipped
check for merge conflicts................................................Passed
check xml............................................(no files to check)Skipped
mixed line ending........................................................Passed
Detect hardcoded secrets.................................................Passed
[project-setup c92da25] added pre-commit config
 1 file changed, 16 insertions(+)
 create mode 100644 .pre-commit-config.yaml
~/src/weather-service (project-setup)$
```

### ,editorconfig File

An [.editorconfig](https://editorconfig.org/) file (along with tools that respect it) can help to keep formatting consistent.

The `dotnet` CLI can add the `.editorconfig` file to your repo

```sh
~/src/weather-service (project-setup)$ dotnet new editorconfig
The template "EditorConfig file" was created successfully.
```

The resulting `.editorconfig` includes windows centric rules like `end_of_line = crlf`.
A somewhat better starting point is [provided in the documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options#example-editorconfig-file), though even that needs some adjustments.
In the end, the formatting standards need to be determined by the project team.
It is important to be sure that the formatting rules defined in `.editorconfig` are compatible with the checks in `.pre-commit-config.yaml`.

### .gitignore File

The `dotnet` CLI has a command for generating an appropriate `.gitignore` file.

```sh
~/src/weather-service (project-setup)$ dotnet new gitignore
The template "dotnet gitignore file" was created successfully.
```

The generated `.gitignore` file is pretty good, but may need tweaking, depending on which [IDE](https://en.wikipedia.org/wiki/Integrated_development_environment)s are used by the team.
IDE specific files should not be checked into SCM, and the generated `.gitignore` doesn't quite align with this.
For example, if you are using [Visual Studio Code](https://code.visualstudio.com/) the generated gitignore tries to pick and choose IDE specific files to add to git.

```gitignore
# VS Code files for those working on multiple tools
.vscode/*
!.vscode/settings.json
!.vscode/tasks.json
!.vscode/launch.json
!.vscode/extensions.json
*.code-workspace
```

A more appropriate rule is simply

```gitignore
.vscode
```

### Solution File

Whether or not you use [solution files](https://learn.microsoft.com/en-us/visualstudio/extensibility/internals/solution-dot-sln-file?view=vs-2022) will depend on your approach to source control.
If your approach is to use a [monorepo](https://en.wikipedia.org/wiki/Monorepo) then you will probably use one or more `solution files` to manage builds within the repo. Even if your approach is `multirepo`, you may use `solutions files`.
With multirepo, every library that is referenced by more than one project should be in its own repository.
But you might find yourself adding single use libraries to service projects in order to manage code organization and visibility.
Tests should be in their own project, but it is convenient to have them in the same repo as the code being tested.
If you have more than one .NET project per repository, `solution files` are a convenient way to manage dependencies within the repo.

I tend to create a solution file and a subdirectory for the service project because it provides the maximum flexibility at the cost of a bit more up-front configuration.

```sh
~/src/weather-service (project-setup)$ dotnet new sln --name Weather
The template "Solution File" was created successfully.
```

### Service Project

The main service project is created with the simple `web` template rather than the more complex `webapi` template.
The `webapi` template adds a number of features that would just need to be removed at a later stage.

```sh
~/src/weather-service (project-setup)$ dotnet new web --name WeatherService --exclude-launch-settings
The template "ASP.NET Core Empty" was created successfully.

Processing post-creation actions...
Restoring /home/ddhuyvetter/src/weather-service/WeatherService/WeatherService.csproj:
  Determining projects to restore...
  Restored /home/ddhuyvetter/src/weather-service/WeatherService/WeatherService.csproj (in 57 ms).
Restore succeeded.


~/src/weather-service (project-setup)$ dotnet sln add WeatherService
Project `WeatherService/WeatherService.csproj` added to the solution.
```
