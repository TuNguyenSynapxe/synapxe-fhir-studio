using SchemaParserService.Models;

namespace SchemaParserService.Services;

public interface ISchemaParserService
{
    Task<SchemaDefinition> ParseSchemaAsync(ParseSchemaRequest request, CancellationToken cancellationToken = default);
}
