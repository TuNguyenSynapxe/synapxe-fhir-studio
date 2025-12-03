using Newtonsoft.Json.Linq;

namespace TemplateService.Models;

public class TemplateResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string FhirVersion { get; set; } = string.Empty;
    public JObject TemplateContent { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static TemplateResponse FromTemplate(Template template)
    {
        return new TemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            ResourceType = template.ResourceType,
            FhirVersion = template.FhirVersion,
            TemplateContent = template.TemplateContent,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}
