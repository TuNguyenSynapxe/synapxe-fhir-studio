using Microsoft.AspNetCore.Mvc;
using TemplateService.Models;
using TemplateService.Services;

namespace TemplateService.Controllers;

[ApiController]
[Route("v1/templates")]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplateController> _logger;

    public TemplateController(ITemplateService templateService, ILogger<TemplateController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Get all templates, optionally filtered by FHIR version
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? fhirVersion = null)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("GetAll templates request received. FhirVersion: {FhirVersion}", fhirVersion);

        var templates = await _templateService.GetAllTemplatesAsync(fhirVersion);

        var response = new SuccessResponse<IEnumerable<TemplateResponse>>
        {
            Data = templates,
            CorrelationId = correlationId
        };

        return Ok(response);
    }

    /// <summary>
    /// Get a specific template by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("GetById template request received. Id: {TemplateId}", id);

        var template = await _templateService.GetTemplateByIdAsync(id);

        if (template == null)
        {
            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "NOT_FOUND",
                    Message = $"Template with id '{id}' not found",
                    Target = "id"
                },
                CorrelationId = correlationId
            };
            return NotFound(errorResponse);
        }

        var response = new SuccessResponse<TemplateResponse>
        {
            Data = template,
            CorrelationId = correlationId
        };

        return Ok(response);
    }

    /// <summary>
    /// Create a new template
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TemplateRequest request)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("Create template request received. Name: {TemplateName}", request.Name);

        var created = await _templateService.CreateTemplateAsync(request);

        var response = new SuccessResponse<TemplateResponse>
        {
            Data = created,
            CorrelationId = correlationId
        };

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    /// <summary>
    /// Update an existing template
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] TemplateRequest request)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("Update template request received. Id: {TemplateId}", id);

        var updated = await _templateService.UpdateTemplateAsync(id, request);

        if (updated == null)
        {
            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "NOT_FOUND",
                    Message = $"Template with id '{id}' not found",
                    Target = "id"
                },
                CorrelationId = correlationId
            };
            return NotFound(errorResponse);
        }

        var response = new SuccessResponse<TemplateResponse>
        {
            Data = updated,
            CorrelationId = correlationId
        };

        return Ok(response);
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("Delete template request received. Id: {TemplateId}", id);

        var deleted = await _templateService.DeleteTemplateAsync(id);

        if (!deleted)
        {
            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "NOT_FOUND",
                    Message = $"Template with id '{id}' not found",
                    Target = "id"
                },
                CorrelationId = correlationId
            };
            return NotFound(errorResponse);
        }

        var response = new SuccessResponse<object>
        {
            Data = new { message = "Template deleted successfully" },
            CorrelationId = correlationId
        };

        return Ok(response);
    }
}
