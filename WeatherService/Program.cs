using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".yml"] = "application/x-yaml";
provider.Mappings[".yaml"] = "application/x-yaml";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});


app.UseSwaggerUi3(cfg =>
{
    cfg.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUi3Route("weather-service", "/weather-service.yaml"));
});

app.MapControllers();

app.Run();
