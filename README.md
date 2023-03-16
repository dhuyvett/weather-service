# weather-service <!-- omit in toc -->

A sample microservice that illustrates techniques for building a production ready .NET 7.0 service.

## Table of Contents <!-- omit in toc -->

- [Project Setup](#project-setup)
  - [pre-commit Configuration](#pre-commit-configuration)
  - [,editorconfig File](#editorconfig-file)
  - [.gitignore File](#gitignore-file)
  - [Solution File](#solution-file)
  - [Service Project](#service-project)
- [Define the API](#define-the-api)
  - [OpenAPI spec](#openapi-spec)
  - [NSwag configuration](#nswag-configuration)
  - [WeatherForecastController](#weatherforecastcontroller)
  - [SwaggerUI](#swaggerui)

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

## Define the API

Most examples will create a controller and use [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) to generate the [OpenAPI](https://www.openapis.org/) spec.
There are a couple of issues with that approach.
One issue is illustrated by the app that is created by `dotnet new webapi`.
If you run that app, and look at the generated API spec, it doesn't match the data returned.
Looking at the schema in the API spec, you see:

```json
"date": {
      "year": 0,
      "month": 0,
      "day": 0,
      "dayOfWeek": 0,
      "dayOfYear": 0,
      "dayNumber": 0
    }
```

But looking at the data returned from the server, you see:

```json
 "date": "2023-03-19"
```

There is an annotation that can be used to fix this, but developing and maintaining the annotations becomes an exercise in reverse-engineering the spec that you want into the code.

Also, it is trivial to make a inconsequential seeming change in the code that ends up changing the exposed API.
Because the spec is what clients will use to understand how to call the service, it should be the source of truth, and the code should follow from the spec.

### OpenAPI spec

The `OpenAPI` spec for your service can be authored in your tool of choice, or [online](https://www.openapis.org/).
A good place to start in understanding what goes into the spec is the [documentation](https://oai.github.io/Documentation/) at the `OpenAPI` initiative.
The [spec](./WeatherService/wwwroot/weather-service.yaml) should be put in a directory named `wwwroot` in the project directory.
The location of the spec file is important because it needs to be served as a static file at runtime to enable the [Swagger UI](https://swagger.io/tools/swagger-ui/).

### NSwag configuration

[NSwag](https://github.com/RicoSuter/NSwag) is a toolset that can be used to generate controllers and data types from an `OpenAPI` spec.
It is similar to `Swashbuckle`, but in addition to being able to generate `OpenAPI` spec from C# controllers, `NSwag` can also generate C# controllers from an `OpenAPI` spec.
It is configured using a [JSON file](./WeatherDervice/../WeatherService/nswag.json).
The [documentation](https://github.com/RicoSuter/NSwag/wiki/NSwag-Configuration-Document) for how to configure `NSwag` for controller generation is not great.
Feel free to start with the [example](./WeatherService/nswag.json) and customize by the elements to suit your needs.

The [project](./WeatherService/WeatherService.csproj) file needs to be updated to include `NSwag` and generate controller code during the build:

```xml
  <!-- run the code generator (if needed) before compile -->
  <Target Name="CodeGen" BeforeTargets="BeforeCompile" Inputs="wwwroot\weather-service.yaml"
    Outputs="Generated\WeatherForecastController.cs">
    <Exec WorkingDirectory="$(ProjectDir)" Command="$(NSwagExe_Net70) run nswag.json" />

    <!-- this is needed for the build to recognize the newly created file as it continues with compile -->
    <ItemGroup>
      <Compile Include="Generated\WeatherForecastController.cs" KeepDuplicates="false" />
    </ItemGroup>
  </Target>

  <ItemGroup>
    <PackageReference Include="NSwag.AspNetCore" Version="13.18.2" />
    <PackageReference Include="NSwag.MSBuild" Version="13.18.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
```

It is a good idea to add `Generated` to a [.gitignore](./WeatherService/.gitignore) file in the project directory to prevent the generated code from being checked into source control.

### WeatherForecastController

With the `nswag.json` file and updated project file, `dotnet build` will result in creation of `Generated\WeatherForecastController.cs`.
I've added a simple [derived controller](./WeatherService/Controllers/WeatherForecastControllerImpl.cs) which overrides the service method in the generated controller and provides a simple static implementation.
If the API spec is changed in a way that changes the controller method signature, it will result in a compile error in the derived controller which will make it clear what needs to be changed in code to adapt to the API change.

To use the controller, [Program.cs](./WeatherService/Program.cs) must be updated.
Remove the default "hello world" mapping that was added when the project was created, and add the following:

```c#
// Add services to the container.
builder.Services.AddControllers();
...
app.MapControllers();
```

Then run the service and you can test it with curl:

```sh
curl -X GET "http://localhost:5000/forecast?city=Poway&units=celsius" -H "accept: application/json"
[{"date":"2023-03-15","lowTemperature":0,"highTemperature":100,"summary":"placeholder"}]
```

### SwaggerUI

To use `SwaggerUI` with the service, the service must serve the `OpenAPI` spec as a static file.
This is why [weather-service.yaml](./WeatherService/wwwroot/weather-service.yaml) is in `wwwroot`.
To make sure that the file is included in the build update the [project](./WeatherService/WeatherService.csproj) file, adding:

```xml
  <ItemGroup>
    <Content Update="wwwroot\**\*.*">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
```

.NET does not serve `YAML` files by default.
Update [Program.cs](./WeatherService/Program.cs), adding the following to enable serving `YAML` files

```c#
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".yml"] = "application/x-yaml";
provider.Mappings[".yaml"] = "application/x-yaml";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
```

Finally, Update `Program.cs` to use SwaggerUI

```c#
app.UseSwaggerUi3(cfg =>
{
    cfg.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUi3Route("weather-service", "/weather-service.yaml"));
});
```

Run the service again, and open a browser to: `http://localhost:5000/swagger` to load SwaggerUI.
