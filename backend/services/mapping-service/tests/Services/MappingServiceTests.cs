using Microsoft.Extensions.Logging;
using MappingService.DTOs;
using MappingService.Models;
using MappingService.Repositories;
using Svc = MappingService.Services;

namespace MappingService.Tests.Services;

public class MappingServiceTests
{
    private readonly Mock<IMappingRepository> _mockRepository;
    private readonly Mock<ILogger<Svc.MappingService>> _mockLogger;
    private readonly Svc.MappingService _service;

    public MappingServiceTests()
    {
        _mockRepository = new Mock<IMappingRepository>();
        _mockLogger = new Mock<ILogger<Svc.MappingService>>();
        _service = new Svc.MappingService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateMappingAsync_CreatesNewDraftMapping()
    {
        // Arrange
        var request = new MappingRequest
        {
            ProjectId = "project-001",
            Name = "Test Mapping",
            Description = "Test Description",
            SourceSchemaId = "schema-001",
            TemplateId = "template-001",
            FhirVersion = "R4",
            Items = new List<MappingItemDto>
            {
                new MappingItemDto
                {
                    SourcePath = "patient.name",
                    TargetPath = "name[0].family",
                    IsRequired = true
                }
            }
        };

        var createdMapping = new MappingDefinition
        {
            Id = "new-id",
            ProjectId = request.ProjectId,
            Name = request.Name,
            Description = request.Description,
            SourceSchemaId = request.SourceSchemaId,
            TemplateId = request.TemplateId,
            FhirVersion = request.FhirVersion,
            Version = 1,
            Status = MappingStatus.Draft,
            Items = new List<MappingItem>
            {
                new MappingItem
                {
                    SourcePath = "patient.name",
                    TargetPath = "name[0].family",
                    IsRequired = true
                }
            }
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<MappingDefinition>()))
            .ReturnsAsync(createdMapping);

        // Act
        var result = await _service.CreateMappingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Mapping");
        result.Status.Should().Be("Draft");
        result.Version.Should().Be(1);
        _mockRepository.Verify(r => r.CreateAsync(It.Is<MappingDefinition>(m => 
            m.Status == MappingStatus.Draft && m.Version == 1)), Times.Once);
    }

    [Fact]
    public async Task UpdateMappingAsync_CannotUpdateActiveMapping()
    {
        // Arrange
        var request = new MappingRequest
        {
            ProjectId = "project-001",
            Name = "Updated Mapping",
            SourceSchemaId = "schema-001",
            TemplateId = "template-001",
            FhirVersion = "R4",
            Items = new List<MappingItemDto>()
        };

        var existingMapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Active
        };

        _mockRepository.Setup(r => r.ExistsAsync("mapping-001")).ReturnsAsync(true);
        _mockRepository.Setup(r => r.GetByIdAsync("mapping-001")).ReturnsAsync(existingMapping);

        // Act & Assert
        await _service.Invoking(s => s.UpdateMappingAsync("mapping-001", request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot update Active mapping*");
    }

    [Fact]
    public async Task UpdateMappingAsync_UpdatesDraftMapping()
    {
        // Arrange
        var request = new MappingRequest
        {
            ProjectId = "project-001",
            Name = "Updated Mapping",
            Description = "Updated Description",
            SourceSchemaId = "schema-001",
            TemplateId = "template-001",
            FhirVersion = "R4",
            Items = new List<MappingItemDto>
            {
                new MappingItemDto
                {
                    SourcePath = "patient.id",
                    TargetPath = "id",
                    IsRequired = true
                }
            }
        };

        var existingMapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Draft,
            Version = 1
        };

        var updatedMapping = new MappingDefinition
        {
            Id = "mapping-001",
            Name = request.Name,
            Description = request.Description,
            Status = MappingStatus.Draft,
            Version = 1
        };

        _mockRepository.Setup(r => r.ExistsAsync("mapping-001")).ReturnsAsync(true);
        _mockRepository.Setup(r => r.GetByIdAsync("mapping-001")).ReturnsAsync(existingMapping);
        _mockRepository.Setup(r => r.UpdateAsync("mapping-001", It.IsAny<MappingDefinition>()))
            .ReturnsAsync(updatedMapping);

        // Act
        var result = await _service.UpdateMappingAsync("mapping-001", request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Mapping");
        _mockRepository.Verify(r => r.UpdateAsync("mapping-001", It.IsAny<MappingDefinition>()), Times.Once);
    }

    [Fact]
    public async Task PublishMappingAsync_DeprecatesExistingActiveAndIncrementsVersion()
    {
        // Arrange
        var draftMapping = new MappingDefinition
        {
            Id = "mapping-draft",
            ProjectId = "project-001",
            SourceSchemaId = "schema-001",
            TemplateId = "template-001",
            FhirVersion = "R4",
            Version = 1,
            Status = MappingStatus.Draft
        };

        var existingActiveMapping = new MappingDefinition
        {
            Id = "mapping-active",
            ProjectId = "project-001",
            SourceSchemaId = "schema-001",
            TemplateId = "template-001",
            FhirVersion = "R4",
            Version = 2,
            Status = MappingStatus.Active
        };

        _mockRepository.Setup(r => r.GetByIdAsync("mapping-draft")).ReturnsAsync(draftMapping);
        _mockRepository.Setup(r => r.GetActiveMappingAsync("project-001", "schema-001", "template-001", "R4"))
            .ReturnsAsync(existingActiveMapping);
        _mockRepository.Setup(r => r.UpdateAsync("mapping-active", It.IsAny<MappingDefinition>()))
            .ReturnsAsync(existingActiveMapping);
        _mockRepository.Setup(r => r.UpdateAsync("mapping-draft", It.IsAny<MappingDefinition>()))
            .ReturnsAsync((string id, MappingDefinition m) =>
            {
                m.Id = id;
                return m;
            });

        // Act
        var result = await _service.PublishMappingAsync("mapping-draft");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Active");
        result.Version.Should().Be(3); // Incremented from existing active's version

        // Verify existing active was deprecated
        _mockRepository.Verify(r => r.UpdateAsync("mapping-active", It.Is<MappingDefinition>(m =>
            m.Status == MappingStatus.Deprecated)), Times.Once);

        // Verify draft was published with incremented version
        _mockRepository.Verify(r => r.UpdateAsync("mapping-draft", It.Is<MappingDefinition>(m =>
            m.Status == MappingStatus.Active && m.Version == 3)), Times.Once);
    }

    [Fact]
    public async Task PublishMappingAsync_NoExistingActive_PublishesWithVersion1()
    {
        // Arrange
        var draftMapping = new MappingDefinition
        {
            Id = "mapping-draft",
            ProjectId = "project-001",
            SourceSchemaId = "schema-001",
            TemplateId = "template-001",
            FhirVersion = "R4",
            Version = 1,
            Status = MappingStatus.Draft
        };

        _mockRepository.Setup(r => r.GetByIdAsync("mapping-draft")).ReturnsAsync(draftMapping);
        _mockRepository.Setup(r => r.GetActiveMappingAsync("project-001", "schema-001", "template-001", "R4"))
            .ReturnsAsync((MappingDefinition?)null);
        _mockRepository.Setup(r => r.UpdateAsync("mapping-draft", It.IsAny<MappingDefinition>()))
            .ReturnsAsync((string id, MappingDefinition m) =>
            {
                m.Id = id;
                return m;
            });

        // Act
        var result = await _service.PublishMappingAsync("mapping-draft");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Active");
        result.Version.Should().Be(1); // Keeps version 1 since no existing active

        _mockRepository.Verify(r => r.UpdateAsync("mapping-draft", It.Is<MappingDefinition>(m =>
            m.Status == MappingStatus.Active && m.Version == 1)), Times.Once);
    }

    [Fact]
    public async Task PublishMappingAsync_AlreadyActive_ThrowsException()
    {
        // Arrange
        var activeMapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Active
        };

        _mockRepository.Setup(r => r.GetByIdAsync("mapping-001")).ReturnsAsync(activeMapping);

        // Act & Assert
        await _service.Invoking(s => s.PublishMappingAsync("mapping-001"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Mapping is already Active.");
    }

    [Fact]
    public async Task PublishMappingAsync_Deprecated_ThrowsException()
    {
        // Arrange
        var deprecatedMapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Deprecated
        };

        _mockRepository.Setup(r => r.GetByIdAsync("mapping-001")).ReturnsAsync(deprecatedMapping);

        // Act & Assert
        await _service.Invoking(s => s.PublishMappingAsync("mapping-001"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot publish a Deprecated mapping.");
    }

    [Fact]
    public async Task DeleteMappingAsync_SetsStatusToDeprecated()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync("mapping-001")).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteMappingAsync("mapping-001");

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync("mapping-001"), Times.Once);
    }

    [Fact]
    public async Task GetAllMappingsAsync_ReturnsAllMappings()
    {
        // Arrange
        var mappings = new List<MappingDefinition>
        {
            new MappingDefinition { Id = "1", ProjectId = "project-001", Name = "Mapping 1" },
            new MappingDefinition { Id = "2", ProjectId = "project-001", Name = "Mapping 2" }
        };

        _mockRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(mappings);

        // Act
        var result = await _service.GetAllMappingsAsync();

        // Assert
        result.Should().HaveCount(2);
        _mockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    [Fact]
    public async Task GetAllMappingsAsync_WithProjectIdFilter_ReturnsFilteredMappings()
    {
        // Arrange
        var mappings = new List<MappingDefinition>
        {
            new MappingDefinition { Id = "1", ProjectId = "project-001", Name = "Mapping 1" }
        };

        _mockRepository.Setup(r => r.GetAllAsync("project-001")).ReturnsAsync(mappings);

        // Act
        var result = await _service.GetAllMappingsAsync("project-001");

        // Assert
        result.Should().HaveCount(1);
        result.First().ProjectId.Should().Be("project-001");
        _mockRepository.Verify(r => r.GetAllAsync("project-001"), Times.Once);
    }
}
