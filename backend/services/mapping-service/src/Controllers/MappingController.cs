using Microsoft.AspNetCore.Mvc;
using MappingService.DTOs;
using MappingService.Models;
using MappingService.Services;

namespace MappingService.Controllers;

[ApiController]
[Route("v1/mappings")]
public class MappingController : ControllerBase
{
    private readonly IMappingService _mappingService;
    private readonly ILogger<MappingController> _logger;

    public MappingController(IMappingService mappingService, ILogger<MappingController> logger)
    {
        _mappingService = mappingService;
        _logger = logger;
    }

    /// <summary>
    /// Get all mappings, optionally filtered by projectId
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? projectId = null)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("GetAll mappings request received. ProjectId: {ProjectId}", projectId);

        var mappings = await _mappingService.GetAllMappingsAsync(projectId);

        var response = new SuccessResponse<IEnumerable<MappingResponse>>
        {
            Data = mappings,
            CorrelationId = correlationId
        };

        return Ok(response);
    }

    /// <summary>
    /// Get a specific mapping by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("GetById mapping request received. Id: {MappingId}", id);

        var mapping = await _mappingService.GetMappingByIdAsync(id);

        if (mapping == null)
        {
            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "NOT_FOUND",
                    Message = $"Mapping with id '{id}' not found",
                    Target = "id"
                },
                CorrelationId = correlationId
            };
            return NotFound(errorResponse);
        }

        var response = new SuccessResponse<MappingResponse>
        {
            Data = mapping,
            CorrelationId = correlationId
        };

        return Ok(response);
    }

    /// <summary>
    /// Create a new mapping (Draft status)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MappingRequest request)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("Create mapping request received. Name: {MappingName}", request.Name);

        var created = await _mappingService.CreateMappingAsync(request);

        var response = new SuccessResponse<MappingResponse>
        {
            Data = created,
            CorrelationId = correlationId
        };

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    /// <summary>
    /// Update an existing mapping (Draft only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] MappingRequest request)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("Update mapping request received. Id: {MappingId}", id);

        try
        {
            var updated = await _mappingService.UpdateMappingAsync(id, request);

            if (updated == null)
            {
                var errorResponse = new ErrorResponse
                {
                    Error = new ErrorDetails
                    {
                        Code = "NOT_FOUND",
                        Message = $"Mapping with id '{id}' not found",
                        Target = "id"
                    },
                    CorrelationId = correlationId
                };
                return NotFound(errorResponse);
            }

            var response = new SuccessResponse<MappingResponse>
            {
                Data = updated,
                CorrelationId = correlationId
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "INVALID_OPERATION",
                    Message = ex.Message,
                    Target = "status"
                },
                CorrelationId = correlationId
            };
            return BadRequest(errorResponse);
        }
    }

    /// <summary>
    /// Publish a mapping (set Status = Active, increment Version)
    /// </summary>
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(string id)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("Publish mapping request received. Id: {MappingId}", id);

        try
        {
            var published = await _mappingService.PublishMappingAsync(id);

            if (published == null)
            {
                var errorResponse = new ErrorResponse
                {
                    Error = new ErrorDetails
                    {
                        Code = "NOT_FOUND",
                        Message = $"Mapping with id '{id}' not found",
                        Target = "id"
                    },
                    CorrelationId = correlationId
                };
                return NotFound(errorResponse);
            }

            var response = new SuccessResponse<MappingResponse>
            {
                Data = published,
                CorrelationId = correlationId
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "INVALID_OPERATION",
                    Message = ex.Message,
                    Target = "status"
                },
                CorrelationId = correlationId
            };
            return BadRequest(errorResponse);
        }
    }

    /// <summary>
    /// Delete a mapping (soft delete - set Status = Deprecated)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
        _logger.LogInformation("Delete mapping request received. Id: {MappingId}", id);

        var deleted = await _mappingService.DeleteMappingAsync(id);

        if (!deleted)
        {
            var errorResponse = new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = "NOT_FOUND",
                    Message = $"Mapping with id '{id}' not found",
                    Target = "id"
                },
                CorrelationId = correlationId
            };
            return NotFound(errorResponse);
        }

        var response = new SuccessResponse<object>
        {
            Data = new { message = "Mapping deleted successfully (status set to Deprecated)" },
            CorrelationId = correlationId
        };

        return Ok(response);
    }
}
