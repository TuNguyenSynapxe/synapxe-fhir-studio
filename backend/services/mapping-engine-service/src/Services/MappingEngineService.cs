using MappingEngineService.DTOs;
using MappingEngineService.Models;
using MappingEngineService.Repositories;
using MappingEngineService.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace MappingEngineService.Services;

public class MappingEngineService : IMappingEngineService
{
    private readonly IMappingRepository _mappingRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly IPathResolver _pathResolver;
    private readonly ITransformationEngine _transformationEngine;
    private readonly ILogger<MappingEngineService> _logger;

    public MappingEngineService(
        IMappingRepository mappingRepository,
        ITemplateRepository templateRepository,
        IPathResolver pathResolver,
        ITransformationEngine transformationEngine,
        ILogger<MappingEngineService> logger)
    {
        _mappingRepository = mappingRepository;
        _templateRepository = templateRepository;
        _pathResolver = pathResolver;
        _transformationEngine = transformationEngine;
        _logger = logger;
    }

    public async Task<MappingExecutionResponse> ExecuteMappingAsync(MappingExecutionRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new MappingExecutionResponse();
        var statistics = new MappingStatistic();

        try
        {
            _logger.LogInformation("Executing mapping: {MappingId} for project: {ProjectId}", 
                request.MappingId, request.ProjectId);

            // 1. Retrieve mapping definition
            var mapping = await _mappingRepository.GetByIdAsync(request.MappingId);
            if (mapping == null)
            {
                throw new InvalidOperationException($"Mapping not found: {request.MappingId}");
            }

            // Log warning if mapping is Draft
            if (mapping.Status == MappingStatus.Draft)
            {
                var warningLog = new MappingLogEntry
                {
                    Path = "mapping",
                    Message = $"Warning: Executing Draft mapping '{mapping.Name}'. Consider publishing to Active status.",
                    Severity = LogSeverity.Warning
                };
                response.Logs.Add(warningLog);
                _logger.LogWarning("Executing Draft mapping: {MappingId}", request.MappingId);
            }

            // 2. Retrieve template
            var template = await _templateRepository.GetByIdAsync(request.TemplateId);
            if (template == null)
            {
                throw new InvalidOperationException($"Template not found: {request.TemplateId}");
            }

            // 3. Parse template content into JObject
            JObject fhirBundle;
            try
            {
                fhirBundle = JObject.Parse(template.TemplateContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse template content: {ex.Message}", ex);
            }

            // 4. Process each mapping item
            foreach (var item in mapping.Items)
            {
                try
                {
                    _logger.LogDebug("Processing mapping item: {ItemId} - {SourcePath} -> {TargetPath}", 
                        item.Id, item.SourcePath, item.TargetPath);

                    // Read source value
                    var sourceValue = _pathResolver.GetValue(request.SourcePayload, item.SourcePath);

                    if (sourceValue == null)
                    {
                        if (item.IsRequired)
                        {
                            var errorLog = new MappingLogEntry
                            {
                                Path = item.SourcePath,
                                Message = $"Required field '{item.SourcePath}' is missing in source payload",
                                Severity = LogSeverity.Error
                            };
                            response.Logs.Add(errorLog);
                            statistics.Errors++;
                            _logger.LogError("Required field missing: {SourcePath}", item.SourcePath);
                        }
                        else
                        {
                            var warningLog = new MappingLogEntry
                            {
                                Path = item.SourcePath,
                                Message = $"Optional field '{item.SourcePath}' is missing in source payload",
                                Severity = LogSeverity.Warning
                            };
                            response.Logs.Add(warningLog);
                            statistics.FieldsSkipped++;
                            _logger.LogWarning("Optional field missing: {SourcePath}", item.SourcePath);
                        }
                        continue;
                    }

                    // Apply transformation if present
                    JToken transformedValue = sourceValue;
                    if (!string.IsNullOrWhiteSpace(item.TransformationExpression))
                    {
                        try
                        {
                            var inputString = sourceValue.Type == JTokenType.String 
                                ? sourceValue.ToString() 
                                : sourceValue.ToString();
                            
                            var transformedString = _transformationEngine.Transform(inputString, item.TransformationExpression);
                            
                            if (transformedString != null)
                            {
                                transformedValue = new JValue(transformedString);
                                _logger.LogDebug("Applied transformation '{Expression}': '{Before}' -> '{After}'", 
                                    item.TransformationExpression, inputString, transformedString);
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorLog = new MappingLogEntry
                            {
                                Path = item.SourcePath,
                                Message = $"Transformation failed for '{item.TransformationExpression}': {ex.Message}",
                                Severity = LogSeverity.Error
                            };
                            response.Logs.Add(errorLog);
                            statistics.Errors++;
                            _logger.LogError(ex, "Transformation failed for {SourcePath}", item.SourcePath);
                            
                            // Use original value if transformation fails
                            transformedValue = sourceValue;
                        }
                    }

                    // Write to target path
                    _pathResolver.SetValue(fhirBundle, item.TargetPath, transformedValue);
                    statistics.FieldsMapped++;
                    
                    _logger.LogDebug("Successfully mapped {SourcePath} -> {TargetPath}", 
                        item.SourcePath, item.TargetPath);
                }
                catch (Exception ex)
                {
                    var errorLog = new MappingLogEntry
                    {
                        Path = item.SourcePath,
                        Message = $"Failed to process mapping item: {ex.Message}",
                        Severity = LogSeverity.Error
                    };
                    response.Logs.Add(errorLog);
                    statistics.Errors++;
                    _logger.LogError(ex, "Error processing mapping item: {ItemId}", item.Id);
                    
                    // Continue processing other items
                    continue;
                }
            }

            // 5. Set the FHIR bundle in response
            response.FhirBundle = fhirBundle;

            // 6. Add success log
            var successLog = new MappingLogEntry
            {
                Path = "execution",
                Message = $"Mapping execution completed. Mapped: {statistics.FieldsMapped}, Skipped: {statistics.FieldsSkipped}, Errors: {statistics.Errors}",
                Severity = LogSeverity.Info
            };
            response.Logs.Add(successLog);

            stopwatch.Stop();
            statistics.ExecutionTime = stopwatch.Elapsed;
            response.Statistics = statistics;

            _logger.LogInformation("Mapping execution completed in {ElapsedMs}ms. Mapped: {Mapped}, Skipped: {Skipped}, Errors: {Errors}", 
                stopwatch.ElapsedMilliseconds, statistics.FieldsMapped, statistics.FieldsSkipped, statistics.Errors);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            statistics.ExecutionTime = stopwatch.Elapsed;
            statistics.Errors++;

            var errorLog = new MappingLogEntry
            {
                Path = "execution",
                Message = $"Mapping execution failed: {ex.Message}",
                Severity = LogSeverity.Error
            };
            response.Logs.Add(errorLog);
            response.Statistics = statistics;

            _logger.LogError(ex, "Mapping execution failed for {MappingId}", request.MappingId);
            
            throw;
        }
    }
}
