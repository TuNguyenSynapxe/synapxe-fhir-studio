using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using MappingEngineService.DTOs;
using MappingEngineService.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace MappingEngineService.Tests.Integration;

public class MappingEngineIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MappingEngineIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExecuteMapping_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{
                ""patient"": {
                    ""firstName"": ""John"",
                    ""lastName"": ""smith"",
                    ""birthDate"": ""1990-01-01"",
                    ""gender"": ""MALE""
                }
            }")
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1/transform/mapping/execute", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<MappingExecutionResponse>>(responseBody);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FhirBundle.Should().NotBeNull();
        result.Data.Statistics.FieldsMapped.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteMapping_MissingSourceFields_ReturnsSuccessWithWarnings()
    {
        // Arrange
        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{
                ""patient"": {
                    ""firstName"": ""John""
                }
            }")
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1/transform/mapping/execute", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<MappingExecutionResponse>>(responseBody);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Logs.Should().Contain(l => l.Severity == LogSeverity.Warning || l.Severity == LogSeverity.Error);
    }

    [Fact]
    public async Task ExecuteMapping_InvalidMappingId_ReturnsBadRequest()
    {
        // Arrange
        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "invalid-mapping",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{ ""patient"": { ""name"": ""John"" } }")
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1/transform/mapping/execute", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Error.Code.Should().Be("INVALID_OPERATION");
    }

    [Fact]
    public async Task ExecuteMapping_EmptyRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new MappingExecutionRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/v1/transform/mapping/execute", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PreviewMapping_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{
                ""patient"": {
                    ""firstName"": ""John"",
                    ""lastName"": ""doe"",
                    ""birthDate"": ""1990-01-01"",
                    ""gender"": ""male""
                }
            }")
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1/transform/mapping/preview", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<MappingExecutionResponse>>(responseBody);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteMapping_WithTransformations_AppliesCorrectly()
    {
        // Arrange
        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{
                ""patient"": {
                    ""firstName"": ""John"",
                    ""lastName"": ""smith"",
                    ""birthDate"": ""1990-01-01"",
                    ""gender"": ""MALE""
                }
            }")
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/v1/transform/mapping/execute", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<MappingExecutionResponse>>(responseBody);
        result.Should().NotBeNull();
        result!.Data!.FhirBundle.Should().NotBeNull();
        
        // Check that transformations were applied
        var bundle = result.Data.FhirBundle;
        var lastName = bundle["name"]?[0]?["family"]?.ToString();
        lastName.Should().Be("SMITH"); // Should be uppercase due to transformation
        
        var gender = bundle["gender"]?.ToString();
        gender.Should().Be("male"); // Should be lowercase due to transformation
    }
}
