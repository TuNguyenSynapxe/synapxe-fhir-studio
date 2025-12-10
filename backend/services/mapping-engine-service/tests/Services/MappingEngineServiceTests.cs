using MappingEngineService.DTOs;
using MappingEngineService.Models;
using MappingEngineService.Services;
using MappingEngineService.Repositories;
using MappingEngineService.Helpers;
using Moq;
using Newtonsoft.Json.Linq;

namespace MappingEngineService.Tests.Services;

public class MappingEngineServiceTests
{
    private readonly Mock<IMappingRepository> _mockMappingRepo;
    private readonly Mock<ITemplateRepository> _mockTemplateRepo;
    private readonly Mock<IPathResolver> _mockPathResolver;
    private readonly Mock<ITransformationEngine> _mockTransformationEngine;
    private readonly Mock<ILogger<MappingEngineService.Services.MappingEngineService>> _mockLogger;
    private readonly MappingEngineService.Services.MappingEngineService _service;

    public MappingEngineServiceTests()
    {
        _mockMappingRepo = new Mock<IMappingRepository>();
        _mockTemplateRepo = new Mock<ITemplateRepository>();
        _mockPathResolver = new Mock<IPathResolver>();
        _mockTransformationEngine = new Mock<ITransformationEngine>();
        _mockLogger = new Mock<ILogger<MappingEngineService.Services.MappingEngineService>>();

        _service = new MappingEngineService.Services.MappingEngineService(
            _mockMappingRepo.Object,
            _mockTemplateRepo.Object,
            _mockPathResolver.Object,
            _mockTransformationEngine.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task ExecuteMappingAsync_SimpleFieldMapping_MapsFieldSuccessfully()
    {
        // Arrange
        var mapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Active,
            Items = new List<MappingItem>
            {
                new MappingItem
                {
                    SourcePath = "patient.name",
                    TargetPath = "name",
                    IsRequired = true
                }
            }
        };

        var template = new Template
        {
            Id = "template-001",
            TemplateContent = @"{ ""resourceType"": ""Patient"" }"
        };

        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{ ""patient"": { ""name"": ""John"" } }")
        };

        _mockMappingRepo.Setup(x => x.GetByIdAsync("mapping-001")).ReturnsAsync(mapping);
        _mockTemplateRepo.Setup(x => x.GetByIdAsync("template-001")).ReturnsAsync(template);
        _mockPathResolver.Setup(x => x.GetValue(It.IsAny<JObject>(), "patient.name"))
            .Returns(new JValue("John"));

        // Act
        var result = await _service.ExecuteMappingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Statistics.FieldsMapped.Should().Be(1);
        result.Statistics.Errors.Should().Be(0);
        _mockPathResolver.Verify(x => x.SetValue(It.IsAny<JObject>(), "name", It.IsAny<JToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteMappingAsync_WithTransformation_AppliesTransformation()
    {
        // Arrange
        var mapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Active,
            Items = new List<MappingItem>
            {
                new MappingItem
                {
                    SourcePath = "patient.name",
                    TargetPath = "name",
                    TransformationExpression = "UPPERCASE",
                    IsRequired = true
                }
            }
        };

        var template = new Template
        {
            Id = "template-001",
            TemplateContent = @"{ ""resourceType"": ""Patient"" }"
        };

        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{ ""patient"": { ""name"": ""john"" } }")
        };

        _mockMappingRepo.Setup(x => x.GetByIdAsync("mapping-001")).ReturnsAsync(mapping);
        _mockTemplateRepo.Setup(x => x.GetByIdAsync("template-001")).ReturnsAsync(template);
        _mockPathResolver.Setup(x => x.GetValue(It.IsAny<JObject>(), "patient.name"))
            .Returns(new JValue("john"));
        _mockTransformationEngine.Setup(x => x.Transform("john", "UPPERCASE"))
            .Returns("JOHN");

        // Act
        var result = await _service.ExecuteMappingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Statistics.FieldsMapped.Should().Be(1);
        _mockTransformationEngine.Verify(x => x.Transform("john", "UPPERCASE"), Times.Once);
    }

    [Fact]
    public async Task ExecuteMappingAsync_MissingRequiredField_LogsError()
    {
        // Arrange
        var mapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Active,
            Items = new List<MappingItem>
            {
                new MappingItem
                {
                    SourcePath = "patient.name",
                    TargetPath = "name",
                    IsRequired = true
                }
            }
        };

        var template = new Template
        {
            Id = "template-001",
            TemplateContent = @"{ ""resourceType"": ""Patient"" }"
        };

        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{ ""patient"": { } }")
        };

        _mockMappingRepo.Setup(x => x.GetByIdAsync("mapping-001")).ReturnsAsync(mapping);
        _mockTemplateRepo.Setup(x => x.GetByIdAsync("template-001")).ReturnsAsync(template);
        _mockPathResolver.Setup(x => x.GetValue(It.IsAny<JObject>(), "patient.name"))
            .Returns((JToken?)null);

        // Act
        var result = await _service.ExecuteMappingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Statistics.Errors.Should().Be(1);
        result.Logs.Should().Contain(l => l.Severity == LogSeverity.Error);
    }

    [Fact]
    public async Task ExecuteMappingAsync_MissingOptionalField_LogsWarning()
    {
        // Arrange
        var mapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Active,
            Items = new List<MappingItem>
            {
                new MappingItem
                {
                    SourcePath = "patient.phone",
                    TargetPath = "telecom",
                    IsRequired = false
                }
            }
        };

        var template = new Template
        {
            Id = "template-001",
            TemplateContent = @"{ ""resourceType"": ""Patient"" }"
        };

        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{ ""patient"": { } }")
        };

        _mockMappingRepo.Setup(x => x.GetByIdAsync("mapping-001")).ReturnsAsync(mapping);
        _mockTemplateRepo.Setup(x => x.GetByIdAsync("template-001")).ReturnsAsync(template);
        _mockPathResolver.Setup(x => x.GetValue(It.IsAny<JObject>(), "patient.phone"))
            .Returns((JToken?)null);

        // Act
        var result = await _service.ExecuteMappingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Statistics.FieldsSkipped.Should().Be(1);
        result.Logs.Should().Contain(l => l.Severity == LogSeverity.Warning);
    }

    [Fact]
    public async Task ExecuteMappingAsync_DraftMapping_LogsWarning()
    {
        // Arrange
        var mapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Draft,
            Items = new List<MappingItem>()
        };

        var template = new Template
        {
            Id = "template-001",
            TemplateContent = @"{ ""resourceType"": ""Patient"" }"
        };

        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = new JObject()
        };

        _mockMappingRepo.Setup(x => x.GetByIdAsync("mapping-001")).ReturnsAsync(mapping);
        _mockTemplateRepo.Setup(x => x.GetByIdAsync("template-001")).ReturnsAsync(template);

        // Act
        var result = await _service.ExecuteMappingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Logs.Should().Contain(l => l.Severity == LogSeverity.Warning && l.Message.Contains("Draft"));
    }

    [Fact]
    public async Task ExecuteMappingAsync_InvalidMappingId_ThrowsException()
    {
        // Arrange
        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "invalid-mapping",
            TemplateId = "template-001",
            SourcePayload = new JObject()
        };

        _mockMappingRepo.Setup(x => x.GetByIdAsync("invalid-mapping")).ReturnsAsync((MappingDefinition?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ExecuteMappingAsync(request));
    }

    [Fact]
    public async Task ExecuteMappingAsync_InvalidTemplateId_ThrowsException()
    {
        // Arrange
        var mapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Active,
            Items = new List<MappingItem>()
        };

        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "invalid-template",
            SourcePayload = new JObject()
        };

        _mockMappingRepo.Setup(x => x.GetByIdAsync("mapping-001")).ReturnsAsync(mapping);
        _mockTemplateRepo.Setup(x => x.GetByIdAsync("invalid-template")).ReturnsAsync((Template?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ExecuteMappingAsync(request));
    }

    [Fact]
    public async Task ExecuteMappingAsync_TransformationFails_ContinuesWithOriginalValue()
    {
        // Arrange
        var mapping = new MappingDefinition
        {
            Id = "mapping-001",
            Status = MappingStatus.Active,
            Items = new List<MappingItem>
            {
                new MappingItem
                {
                    SourcePath = "patient.name",
                    TargetPath = "name",
                    TransformationExpression = "INVALID",
                    IsRequired = false
                }
            }
        };

        var template = new Template
        {
            Id = "template-001",
            TemplateContent = @"{ ""resourceType"": ""Patient"" }"
        };

        var request = new MappingExecutionRequest
        {
            ProjectId = "project-001",
            MappingId = "mapping-001",
            TemplateId = "template-001",
            SourcePayload = JObject.Parse(@"{ ""patient"": { ""name"": ""John"" } }")
        };

        _mockMappingRepo.Setup(x => x.GetByIdAsync("mapping-001")).ReturnsAsync(mapping);
        _mockTemplateRepo.Setup(x => x.GetByIdAsync("template-001")).ReturnsAsync(template);
        _mockPathResolver.Setup(x => x.GetValue(It.IsAny<JObject>(), "patient.name"))
            .Returns(new JValue("John"));
        _mockTransformationEngine.Setup(x => x.Transform("John", "INVALID"))
            .Throws(new Exception("Transformation error"));

        // Act
        var result = await _service.ExecuteMappingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Statistics.Errors.Should().BeGreaterThan(0);
        result.Logs.Should().Contain(l => l.Severity == LogSeverity.Error && l.Message.Contains("Transformation failed"));
    }
}
