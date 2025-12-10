using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using MappingService.DTOs;
using MappingService.Models;

namespace MappingService.Tests.Integration;

public class MappingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MappingIntegrationTests(WebApplicationFactory<Program> factory)
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
        content.Should().Contain("mapping-service");
    }

    [Fact]
    public async Task GetAll_ReturnsMappings()
    {
        // Act
        var response = await _client.GetAsync("/v1/mappings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<List<MappingResponse>>>(content);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new MappingRequest
        {
            ProjectId = "project-test",
            Name = "Test Mapping",
            Description = "Integration test mapping",
            SourceSchemaId = "schema-test",
            TemplateId = "template-test",
            FhirVersion = "R4",
            Items = new List<MappingItemDto>
            {
                new MappingItemDto
                {
                    SourcePath = "patient.firstName",
                    TargetPath = "name[0].given[0]",
                    IsRequired = true,
                    Notes = "First name mapping"
                }
            }
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/v1/mappings", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SuccessResponse<MappingResponse>>(responseContent);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test Mapping");
        result.Data.Status.Should().Be("Draft");
        result.Data.Version.Should().Be(1);
        result.Data.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Publish_DraftMapping_ReturnsActiveMapping()
    {
        // Arrange - Create a draft mapping first
        var createRequest = new MappingRequest
        {
            ProjectId = "project-publish-test",
            Name = "Mapping To Publish",
            SourceSchemaId = "schema-publish",
            TemplateId = "template-publish",
            FhirVersion = "R4",
            Items = new List<MappingItemDto>
            {
                new MappingItemDto
                {
                    SourcePath = "test.field",
                    TargetPath = "test.path",
                    IsRequired = false
                }
            }
        };

        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest),
            Encoding.UTF8,
            "application/json");

        var createResponse = await _client.PostAsync("/v1/mappings", createContent);
        var createResult = JsonConvert.DeserializeObject<SuccessResponse<MappingResponse>>(
            await createResponse.Content.ReadAsStringAsync());
        var mappingId = createResult!.Data!.Id;

        // Act - Publish the mapping
        var publishResponse = await _client.PostAsync($"/v1/mappings/{mappingId}/publish", null);

        // Assert
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var publishContent = await publishResponse.Content.ReadAsStringAsync();
        var publishResult = JsonConvert.DeserializeObject<SuccessResponse<MappingResponse>>(publishContent);

        publishResult.Should().NotBeNull();
        publishResult!.Data.Should().NotBeNull();
        publishResult.Data!.Status.Should().Be("Active");
        publishResult.Data.Version.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task Delete_ExistingMapping_ReturnsOk()
    {
        // Arrange - Create a mapping first
        var createRequest = new MappingRequest
        {
            ProjectId = "project-delete-test",
            Name = "Mapping To Delete",
            SourceSchemaId = "schema-delete",
            TemplateId = "template-delete",
            FhirVersion = "R4",
            Items = new List<MappingItemDto>()
        };

        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest),
            Encoding.UTF8,
            "application/json");

        var createResponse = await _client.PostAsync("/v1/mappings", createContent);
        var createResult = JsonConvert.DeserializeObject<SuccessResponse<MappingResponse>>(
            await createResponse.Content.ReadAsStringAsync());
        var mappingId = createResult!.Data!.Id;

        // Act
        var deleteResponse = await _client.DeleteAsync($"/v1/mappings/{mappingId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ActiveMapping_ReturnsBadRequest()
    {
        // Arrange - Create and publish a mapping
        var createRequest = new MappingRequest
        {
            ProjectId = "project-update-test",
            Name = "Mapping To Update",
            SourceSchemaId = "schema-update",
            TemplateId = "template-update",
            FhirVersion = "R4",
            Items = new List<MappingItemDto>()
        };

        var createContent = new StringContent(
            JsonConvert.SerializeObject(createRequest),
            Encoding.UTF8,
            "application/json");

        var createResponse = await _client.PostAsync("/v1/mappings", createContent);
        var createResult = JsonConvert.DeserializeObject<SuccessResponse<MappingResponse>>(
            await createResponse.Content.ReadAsStringAsync());
        var mappingId = createResult!.Data!.Id;

        // Publish it
        await _client.PostAsync($"/v1/mappings/{mappingId}/publish", null);

        // Act - Try to update the active mapping
        var updateRequest = new MappingRequest
        {
            ProjectId = "project-update-test",
            Name = "Updated Name",
            SourceSchemaId = "schema-update",
            TemplateId = "template-update",
            FhirVersion = "R4",
            Items = new List<MappingItemDto>()
        };

        var updateContent = new StringContent(
            JsonConvert.SerializeObject(updateRequest),
            Encoding.UTF8,
            "application/json");

        var updateResponse = await _client.PutAsync($"/v1/mappings/{mappingId}", updateContent);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
