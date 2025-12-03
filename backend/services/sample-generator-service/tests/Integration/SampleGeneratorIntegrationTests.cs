using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using SampleGeneratorService.Models;

namespace SampleGeneratorService.Tests.Integration;

public class SampleGeneratorIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SampleGeneratorIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateSampleData_WithValidRequest_ShouldReturn200()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 2,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode
                {
                    Name = "patient",
                    DataType = "Grouping",
                    Cardinality = "1",
                    Children = new List<SchemaNode>
                    {
                        new SchemaNode { Name = "id", DataType = "String", Cardinality = "1", SampleValue = "P12345" },
                        new SchemaNode { Name = "name", DataType = "String", Cardinality = "1" }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Add("X-Correlation-Id", Guid.NewGuid().ToString());

        // Act
        var response = await _client.PostAsync("/v1/transform/sample/generate", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("success");
        responseContent.Should().Contain("P12345");
    }

    [Fact]
    public async Task GenerateSampleData_WithoutCorrelationId_ShouldReturn200WithGeneratedId()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode { Name = "field1", DataType = "String", Cardinality = "1" }
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1/transform/sample/generate", content);

        // Assert - should still work, middleware will generate correlation ID
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Correlation-Id");
    }

    [Fact]
    public async Task GenerateSampleData_WithInvalidRequest_ShouldReturn400()
    {
        // Arrange - RecordCount = 0 is invalid
        var request = new SampleGenerationRequest
        {
            RecordCount = 0,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode { Name = "field1", DataType = "String", Cardinality = "1" }
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Add("X-Correlation-Id", Guid.NewGuid().ToString());

        // Act
        var response = await _client.PostAsync("/v1/transform/sample/generate", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateSampleData_WithNoSchema_ShouldReturn400()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1
            // No schema provided
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Add("X-Correlation-Id", Guid.NewGuid().ToString());

        // Act
        var response = await _client.PostAsync("/v1/transform/sample/generate", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
        content.Should().Contain("sample-generator-service");
    }
}
