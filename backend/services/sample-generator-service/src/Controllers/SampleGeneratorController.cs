using Microsoft.AspNetCore.Mvc;
using SampleGeneratorService.Models;
using SampleGeneratorService.Services;
using FluentValidation;

namespace SampleGeneratorService.Controllers;

[ApiController]
[Route("v1/transform/sample")]
public class SampleGeneratorController : ControllerBase
{
    private readonly ISampleGeneratorService _sampleGeneratorService;
    private readonly IValidator<SampleGenerationRequest> _validator;
    private readonly ILogger<SampleGeneratorController> _logger;

    public SampleGeneratorController(
        ISampleGeneratorService sampleGeneratorService,
        IValidator<SampleGenerationRequest> validator,
        ILogger<SampleGeneratorController> logger)
    {
        _sampleGeneratorService = sampleGeneratorService;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateSampleData(
        [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
        [FromBody] SampleGenerationRequest request)
    {
        // Use correlation ID from header or generate one
        correlationId ??= HttpContext.Items["X-Correlation-Id"]?.ToString() ?? Guid.NewGuid().ToString();
        
        _logger.LogInformation("Generating sample data. CorrelationId: {CorrelationId}", correlationId);

        // Validate request
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Request validation failed",
                    Details = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
                    TraceId = HttpContext.TraceIdentifier
                },
                CorrelationId = correlationId
            };

            return BadRequest(errorResponse);
        }

        try
        {
            var samples = await _sampleGeneratorService.GenerateSamplesAsync(request);

            var response = new SuccessResponse<object>
            {
                Data = new
                {
                    Samples = samples,
                    Count = samples.Count
                },
                CorrelationId = correlationId
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "INVALID_INPUT",
                    Message = ex.Message,
                    TraceId = HttpContext.TraceIdentifier
                },
                CorrelationId = correlationId
            };

            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sample data. CorrelationId: {CorrelationId}", correlationId);

            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "GENERATION_ERROR",
                    Message = "An error occurred while generating sample data",
                    Details = new List<string> { ex.Message },
                    TraceId = HttpContext.TraceIdentifier
                },
                CorrelationId = correlationId
            };

            return StatusCode(500, errorResponse);
        }
    }
}
