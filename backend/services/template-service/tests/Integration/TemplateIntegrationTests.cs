using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TemplateService.Models;

namespace TemplateService.Tests.Integration;

public class TemplateIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TemplateIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ok");
        content.Should().Contain("template-service");
    }

    [Fact]
    public async Task GetAll_ReturnsTemplates()
    {
        // Act
        var response = await _client.GetAsync("/v1/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<List<TemplateResponse>>>(content);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCountGreaterThanOrEqualTo(2); // Seeded data
        result.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAll_WithFhirVersionFilter_ReturnsFilteredTemplates()
    {
        // Act
        var response = await _client.GetAsync("/v1/templates?fhirVersion=R4");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<List<TemplateResponse>>>(content);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().AllSatisfy(t => t.FhirVersion.Should().Be("R4"));
    }

    [Fact]
    public async Task GetById_ExistingTemplate_ReturnsTemplate()
    {
        // Arrange - First get all to find an existing ID
        var getAllResponse = await _client.GetAsync("/v1/templates");
        var getAllContent = await getAllResponse.Content.ReadAsStringAsync();
        var templates = JsonConvert.DeserializeObject<SuccessResponse<List<TemplateResponse>>>(getAllContent);
        var existingId = templates!.Data!.First().Id;

        // Act
        var response = await _client.GetAsync($"/v1/templates/{existingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<TemplateResponse>>(content);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingId);
    }

    [Fact]
    public async Task GetById_NonExistingTemplate_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/v1/templates/non-existing-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ErrorResponse>(content);

        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new TemplateRequest
        {
            Name = "Test Condition Template",
            ResourceType = "Condition",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse(@"{
                ""resourceType"": ""Condition"",
                ""code"": {},
                ""subject"": {}
            }")
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/v1/templates", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<TemplateResponse>>(responseContent);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test Condition Template");
        result.Data.ResourceType.Should().Be("Condition");
        result.Data.Id.Should().NotBeNullOrEmpty();
        
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange - Missing required fields
        var request = new TemplateRequest
        {
            Name = "", // Invalid - empty name
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse("{}")
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/v1/templates", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ExistingTemplate_ReturnsUpdated()
    {
        // Arrange - Create a template first
        var createRequest = new TemplateRequest
        {
            Name = "Original Name",
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\"}")
        };

        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest),
            Encoding.UTF8,
            "application/json");

        var createResponse = await _client.PostAsync("/v1/templates", createContent);
        var createResult = JsonConvert.DeserializeObject<SuccessResponse<TemplateResponse>>(
            await createResponse.Content.ReadAsStringAsync());
        var templateId = createResult!.Data!.Id;

        // Update request
        var updateRequest = new TemplateRequest
        {
            Name = "Updated Name",
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\",\"active\":true}")
        };

        var updateContent = new StringContent(
            JsonConvert.SerializeObject(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PutAsync($"/v1/templates/{templateId}", updateContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<TemplateResponse>>(responseContent);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Updated Name");
        result.Data.Id.Should().Be(templateId);
    }

    [Fact]
    public async Task Update_NonExistingTemplate_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new TemplateRequest
        {
            Name = "Updated Name",
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\"}")
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(updateRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PutAsync("/v1/templates/non-existing-id", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ExistingTemplate_ReturnsOk()
    {
        // Arrange - Create a template first
        var createRequest = new TemplateRequest
        {
            Name = "To Be Deleted",
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\"}")
        };

        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest),
            Encoding.UTF8,
            "application/json");

        var createResponse = await _client.PostAsync("/v1/templates", createContent);
        var createResult = JsonConvert.DeserializeObject<SuccessResponse<TemplateResponse>>(
            await createResponse.Content.ReadAsStringAsync());
        var templateId = createResult!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/v1/templates/{templateId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<object>>(responseContent);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Verify deletion
        var getResponse = await _client.GetAsync($"/v1/templates/{templateId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistingTemplate_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/v1/templates/non-existing-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CorrelationId_IsPresentInResponse()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        _client.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);

        // Act
        var response = await _client.GetAsync("/v1/templates");

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-Id");
        response.Headers.GetValues("X-Correlation-Id").First().Should().Be(correlationId);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<List<TemplateResponse>>>(content);
        result!.CorrelationId.Should().Be(correlationId);
    }
}
