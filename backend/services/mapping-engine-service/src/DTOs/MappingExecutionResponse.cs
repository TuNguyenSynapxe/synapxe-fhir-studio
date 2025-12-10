using MappingEngineService.Models;
using Newtonsoft.Json.Linq;

namespace MappingEngineService.DTOs;

public class MappingExecutionResponse
{
    public JObject FhirBundle { get; set; } = new();
    public List<MappingLogEntry> Logs { get; set; } = new();
    public MappingStatistic Statistics { get; set; } = new();
}
