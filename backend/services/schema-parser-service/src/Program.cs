using FluentValidation;
using Serilog;
using SchemaParserService.Middleware;
using SchemaParserService.Models;
using SchemaParserService.Services;
using SchemaParserService.Validators;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Schema Parser Service",
        Version = "1.0.0",
        Description = "Parses legacy schemas (CSV/XML/JSON/XSD) into normalized SchemaDefinition objects."
    });
});

// Register services
builder.Services.AddScoped<ISchemaParserService, SchemaParserService.Services.SchemaParserService>();
builder.Services.AddScoped<SchemaParserService.Services.HierarchicalCsvParser>();

// Register validators
builder.Services.AddScoped<IValidator<ParseSchemaRequest>, ParseSchemaRequestValidator>();

// Add health checks
builder.Services.AddHealthChecks();

// Configure CORS (if needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandling();
app.UseCorrelationId();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Schema Parser Service v1");
    });
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Log startup information
app.Logger.LogInformation("Schema Parser Service started successfully");

try
{
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
