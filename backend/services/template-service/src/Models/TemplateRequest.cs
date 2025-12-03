using Newtonsoft.Json.Linq;

namespace TemplateService.Models;

public class TemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string FhirVersion { get; set; } = "R4";
    public JObject TemplateContent { get; set; } = new();
}
