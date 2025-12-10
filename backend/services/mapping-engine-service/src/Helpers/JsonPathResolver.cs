using Newtonsoft.Json.Linq;

namespace MappingEngineService.Helpers;

public class JsonPathResolver : IPathResolver
{
    public JToken? GetValue(JObject source, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            // Support both dotted notation (patient.name.given) and JSONPath ($.patient.name.given)
            var normalizedPath = path.StartsWith("$") ? path : $"$.{path}";
            var token = source.SelectToken(normalizedPath);
            return token;
        }
        catch
        {
            return null;
        }
    }

    public void SetValue(JObject target, string path, JToken? value)
    {
        if (string.IsNullOrWhiteSpace(path) || value == null)
            return;

        var pathParts = path.Split('.');
        JObject current = target;

        // Navigate/create intermediate nodes
        for (int i = 0; i < pathParts.Length - 1; i++)
        {
            var part = pathParts[i];
            
            // Handle array notation (e.g., "items[0]")
            var arrayMatch = System.Text.RegularExpressions.Regex.Match(part, @"^(.+)\[(\d+)\]$");
            if (arrayMatch.Success)
            {
                var arrayName = arrayMatch.Groups[1].Value;
                var index = int.Parse(arrayMatch.Groups[2].Value);
                
                if (current[arrayName] == null)
                {
                    current[arrayName] = new JArray();
                }
                
                var array = (JArray)current[arrayName]!;
                
                // Ensure array has enough elements
                while (array.Count <= index)
                {
                    array.Add(new JObject());
                }
                
                current = (JObject)array[index];
            }
            else
            {
                if (current[part] == null)
                {
                    current[part] = new JObject();
                }
                
                current = (JObject)current[part]!;
            }
        }

        // Set the final value
        var finalPart = pathParts[^1];
        
        // Handle array notation in final part
        var finalArrayMatch = System.Text.RegularExpressions.Regex.Match(finalPart, @"^(.+)\[(\d+)\]$");
        if (finalArrayMatch.Success)
        {
            var arrayName = finalArrayMatch.Groups[1].Value;
            var index = int.Parse(finalArrayMatch.Groups[2].Value);
            
            if (current[arrayName] == null)
            {
                current[arrayName] = new JArray();
            }
            
            var array = (JArray)current[arrayName]!;
            
            while (array.Count <= index)
            {
                array.Add(null);
            }
            
            array[index] = value;
        }
        else
        {
            current[finalPart] = value;
        }
    }
}
