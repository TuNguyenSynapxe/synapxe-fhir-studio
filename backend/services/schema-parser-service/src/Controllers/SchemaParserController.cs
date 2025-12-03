using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SchemaParserService.Models;
using SchemaParserService.Services;

namespace SchemaParserService.Controllers;

[ApiController]
[Route("v1/transform/schema")]
[Produces("application/json")]
public class SchemaParserController : ControllerBase
{
    private readonly ISchemaParserService _schemaParserService;
    private readonly IValidator<ParseSchemaRequest> _validator;
    private readonly ILogger<SchemaParserController> _logger;

    public SchemaParserController(
        ISchemaParserService schemaParserService,
        IValidator<ParseSchemaRequest> validator,
        ILogger<SchemaParserController> logger)
    {
        _schemaParserService = schemaParserService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Parse legacy schema (CSV/XML/JSON/XSD) into normalized SchemaDefinition
    /// </summary>
    /// <param name="request">Schema parsing request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parsed schema definition</returns>
    [HttpPost("parse")]
    [ProducesResponseType(typeof(SuccessResponse<SchemaDefinition>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ParseSchema(
        [FromBody] ParseSchemaRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogInformation("Received parse schema request for {Name}", request.Name);

        // Validate request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for request: {Errors}", 
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var errorResponse = new ErrorResponse
            {
                Success = false,
                CorrelationId = correlationId,
                Error = new ErrorDetails
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Request validation failed",
                    Details = validationResult.Errors.Select(e => e.ErrorMessage).ToList(),
                    Target = "parseSchema",
                    TraceId = HttpContext.TraceIdentifier
                }
            };

            return BadRequest(errorResponse);
        }

        try
        {
            var schemaDefinition = await _schemaParserService.ParseSchemaAsync(request, cancellationToken);

            var successResponse = new SuccessResponse<SchemaDefinition>
            {
                Success = true,
                Data = schemaDefinition,
                Warnings = new List<string>(),
                CorrelationId = correlationId
            };

            return Ok(successResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing schema");
            throw; // Let the global exception handler deal with it
        }
    }
}
