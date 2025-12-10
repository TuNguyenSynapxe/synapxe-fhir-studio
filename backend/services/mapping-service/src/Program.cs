using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using MappingService.Middleware;
using MappingService.Repositories;
using MappingService.Validators;
using Svc = MappingService.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting mapping-service");

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
    builder.Services.AddValidatorsFromAssemblyContaining<MappingRequestValidator>();

    // Register application services
    builder.Services.AddSingleton<IMappingRepository, InMemoryMappingRepository>();
    builder.Services.AddScoped<Svc.IMappingService, Svc.MappingService>();

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

    app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "mapping-service" }));

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
