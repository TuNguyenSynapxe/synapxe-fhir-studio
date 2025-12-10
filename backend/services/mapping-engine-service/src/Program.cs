using MappingEngineService.Middleware;
using MappingEngineService.Repositories;
using MappingEngineService.Services;
using MappingEngineService.Helpers;
using MappingEngineService.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("Starting mapping-engine-service");

// Add services
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Mapping Engine Service API", Version = "v1" });
});

// Register FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<MappingExecutionRequestValidator>();

// Register repositories
builder.Services.AddSingleton<IMappingRepository, InMemoryMappingRepository>();
builder.Services.AddSingleton<ITemplateRepository, InMemoryTemplateRepository>();

// Register helpers
builder.Services.AddSingleton<IPathResolver, JsonPathResolver>();
builder.Services.AddSingleton<ITransformationEngine, TransformationEngine>();

// Register services
builder.Services.AddScoped<IMappingEngineService, MappingEngineService.Services.MappingEngineService>();

var app = builder.Build();

// Configure middleware pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "mapping-engine-service" }));

try
{
    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program accessible for testing
public partial class Program { }
