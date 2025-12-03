using FluentValidation;
using Serilog;
using SampleGeneratorService.Middleware;
using SampleGeneratorService.Services;
using SampleGeneratorService.Validators;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Sample Generator Service", Version = "v1" });
});

// Register application services
builder.Services.AddScoped<ISampleGeneratorService, SampleGeneratorService.Services.SampleGeneratorService>();
builder.Services.AddScoped<IOpenAiSampleGenerator, OpenAiSampleGeneratorStub>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<SampleGenerationRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline
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

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "sample-generator-service", timestamp = DateTime.UtcNow }));

app.Run();

// Make Program class accessible for testing
public partial class Program { }
