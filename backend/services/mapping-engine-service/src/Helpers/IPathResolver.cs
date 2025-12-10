using Newtonsoft.Json.Linq;

namespace MappingEngineService.Helpers;

public interface IPathResolver
{
    /// <summary>
    /// Reads a value from source JSON using dotted path notation (e.g., "patient.name.given")
    /// </summary>
    JToken? GetValue(JObject source, string path);
    
    /// <summary>
    /// Sets a value in target JSON using dotted path notation, creating intermediate nodes as needed
    /// </summary>
    void SetValue(JObject target, string path, JToken? value);
}
