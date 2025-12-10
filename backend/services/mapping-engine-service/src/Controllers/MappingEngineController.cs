using Microsoft.AspNetCore.Mvc;
using MappingEngineService.DTOs;
using MappingEngineService.Models;
using MappingEngineService.Services;
using Microsoft.Extensions.Logging;

namespace MappingEngineService.Controllers;

[ApiController]
[Route("v1/transform/mapping")]
public class MappingEngineController : ControllerBase
{
    private readonly IMappingEngineService _mappingEngineService;
    private readonly ILogger<MappingEngineController> _logger;

    public MappingEngineController(
        IMappingEngineService mappingEngineService,
        ILogger<MappingEngineController> logger)
    {
        _mappingEngineService = mappingEngineService;
        _logger = logger;
    }

    [HttpPost("execute")]
    public async Task<ActionResult<SuccessResponse<MappingExecutionResponse>>> ExecuteMapping(
        [FromBody] MappingExecutionRequest request)
    {
        try
        {
            _logger.LogInformation("Execute mapping request received. MappingId: {MappingId}, ProjectId: {ProjectId}", 
                request.MappingId, request.ProjectId);

            var result = await _mappingEngineService.ExecuteMappingAsync(request);

            var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString() ?? string.Empty;

            return Ok(new SuccessResponse<MappingExecutionResponse>
            {
                Success = true,
                Data = result,
                CorrelationId = correlationId
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during mapping execution");

            var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString() ?? string.Empty;

            return BadRequest(new ErrorResponse
            {
                Success = false,
                Error = new ErrorDetails
                {
                    Code = "INVALID_OPERATION",
                    Message = ex.Message
                },
                CorrelationId = correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during mapping execution");

            var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString() ?? string.Empty;

            return StatusCode(500, new ErrorResponse
            {
                Success = false,
                Error = new ErrorDetails
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred during mapping execution"
                },
                CorrelationId = correlationId
            });
        }
    }

    [HttpPost("preview")]
    public async Task<ActionResult<SuccessResponse<MappingExecutionResponse>>> PreviewMapping(
        [FromBody] MappingExecutionRequest request)
    {
        // Preview is the same as execute for Phase 1
        return await ExecuteMapping(request);
    }
}
