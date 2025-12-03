using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using TemplateService.Middleware;
using TemplateService.Repositories;
using TemplateService.Validators;
using Svc = TemplateService.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting template-service");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services
    builder.Services.AddControllers()
        .AddNewtonsoftJson(); // Use Newtonsoft.Json for serialization
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<TemplateRequestValidator>();

    // Register application services
    builder.Services.AddSingleton<ITemplateRepository, StubTemplateRepository>();
    builder.Services.AddScoped<Svc.ITemplateService, Svc.TemplateService>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Add custom middleware
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseRouting();
    app.MapControllers();

    app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "template-service" }));

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

// Make the implicit Program class public so test projects can access it
public partial class Program { }
