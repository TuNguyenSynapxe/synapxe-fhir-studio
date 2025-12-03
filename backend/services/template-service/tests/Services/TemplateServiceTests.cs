using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using TemplateService.Models;
using TemplateService.Repositories;
using Svc = TemplateService.Services;

namespace TemplateService.Tests.Services;

public class TemplateServiceTests
{
    private readonly Mock<ITemplateRepository> _mockRepository;
    private readonly Mock<ILogger<Svc.TemplateService>> _mockLogger;
    private readonly Svc.TemplateService _service;

    public TemplateServiceTests()
    {
        _mockRepository = new Mock<ITemplateRepository>();
        _mockLogger = new Mock<ILogger<Svc.TemplateService>>();
        _service = new Svc.TemplateService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllTemplatesAsync_ReturnsAllTemplates()
    {
        // Arrange
        var templates = new List<Template>
        {
            new Template
            {
                Id = "1",
                Name = "Patient Template",
                ResourceType = "Patient",
                FhirVersion = "R4",
                TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\"}")
            },
            new Template
            {
                Id = "2",
                Name = "Observation Template",
                ResourceType = "Observation",
                FhirVersion = "R4",
                TemplateContent = JObject.Parse("{\"resourceType\":\"Observation\"}")
            }
        };

        _mockRepository.Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(templates);

        // Act
        var result = await _service.GetAllTemplatesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<TemplateResponse>();
        _mockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    [Fact]
    public async Task GetAllTemplatesAsync_WithFhirVersionFilter_ReturnsFilteredTemplates()
    {
        // Arrange
        var templates = new List<Template>
        {
            new Template
            {
                Id = "1",
                Name = "Patient Template",
                ResourceType = "Patient",
                FhirVersion = "R4",
                TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\"}")
            }
        };

        _mockRepository.Setup(r => r.GetAllAsync("R4"))
            .ReturnsAsync(templates);

        // Act
        var result = await _service.GetAllTemplatesAsync("R4");

        // Assert
        result.Should().HaveCount(1);
        result.First().FhirVersion.Should().Be("R4");
        _mockRepository.Verify(r => r.GetAllAsync("R4"), Times.Once);
    }

    [Fact]
    public async Task GetTemplateByIdAsync_ExistingId_ReturnsTemplate()
    {
        // Arrange
        var template = new Template
        {
            Id = "1",
            Name = "Patient Template",
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\"}")
        };

        _mockRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(template);

        // Act
        var result = await _service.GetTemplateByIdAsync("1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("1");
        result.Name.Should().Be("Patient Template");
        _mockRepository.Verify(r => r.GetByIdAsync("1"), Times.Once);
    }

    [Fact]
    public async Task GetTemplateByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync("999"))
            .ReturnsAsync((Template?)null);

        // Act
        var result = await _service.GetTemplateByIdAsync("999");

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync("999"), Times.Once);
    }

    [Fact]
    public async Task CreateTemplateAsync_ValidRequest_ReturnsCreatedTemplate()
    {
        // Arrange
        var request = new TemplateRequest
        {
            Name = "New Template",
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\"}")
        };

        var createdTemplate = new Template
        {
            Id = "new-id",
            Name = request.Name,
            ResourceType = request.ResourceType,
            FhirVersion = request.FhirVersion,
            TemplateContent = request.TemplateContent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Template>()))
            .ReturnsAsync(createdTemplate);

        // Act
        var result = await _service.CreateTemplateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Template");
        result.Id.Should().Be("new-id");
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Template>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTemplateAsync_ExistingId_ReturnsUpdatedTemplate()
    {
        // Arrange
        var request = new TemplateRequest
        {
            Name = "Updated Template",
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\"}")
        };

        var updatedTemplate = new Template
        {
            Id = "1",
            Name = request.Name,
            ResourceType = request.ResourceType,
            FhirVersion = request.FhirVersion,
            TemplateContent = request.TemplateContent,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.ExistsAsync("1"))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateAsync("1", It.IsAny<Template>()))
            .ReturnsAsync(updatedTemplate);

        // Act
        var result = await _service.UpdateTemplateAsync("1", request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Template");
        result.Id.Should().Be("1");
        _mockRepository.Verify(r => r.ExistsAsync("1"), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync("1", It.IsAny<Template>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTemplateAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var request = new TemplateRequest
        {
            Name = "Updated Template",
            ResourceType = "Patient",
            FhirVersion = "R4",
            TemplateContent = JObject.Parse("{\"resourceType\":\"Patient\"}")
        };

        _mockRepository.Setup(r => r.ExistsAsync("999"))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateTemplateAsync("999", request);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.ExistsAsync("999"), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<Template>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTemplateAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync("1"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteTemplateAsync("1");

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync("1"), Times.Once);
    }

    [Fact]
    public async Task DeleteTemplateAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync("999"))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteTemplateAsync("999");

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync("999"), Times.Once);
    }
}
