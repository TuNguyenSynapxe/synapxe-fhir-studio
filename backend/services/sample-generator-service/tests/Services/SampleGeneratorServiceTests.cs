using Microsoft.Extensions.Logging;
using SampleGeneratorService.Models;
using SampleGeneratorService.Services;
using Svc = SampleGeneratorService.Services.SampleGeneratorService;

namespace SampleGeneratorService.Tests.Services;

public class SampleGeneratorServiceTests
{
    private readonly Mock<ILogger<Svc>> _loggerMock;
    private readonly Mock<IOpenAiSampleGenerator> _aiGeneratorMock;
    private readonly Svc _service;

    public SampleGeneratorServiceTests()
    {
        _loggerMock = new Mock<ILogger<Svc>>();
        _aiGeneratorMock = new Mock<IOpenAiSampleGenerator>();
        _aiGeneratorMock.Setup(x => x.GenerateValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string?)null); // Always return null to test deterministic generation

        _service = new Svc(_loggerMock.Object, _aiGeneratorMock.Object);
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithHierarchicalSchema_ShouldGenerateDeterministicData()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
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
                        new SchemaNode { Name = "id", DataType = "String", Cardinality = "1" },
                        new SchemaNode { Name = "name", DataType = "String", Cardinality = "1" }
                    }
                }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].Should().ContainKey("id");
        result[0].Should().ContainKey("name");
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithSampleValue_ShouldUseSampleValue()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode
                {
                    Name = "institutionCode",
                    DataType = "String",
                    Cardinality = "1",
                    SampleValue = "\"CGH\""
                }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result.Should().NotBeEmpty();
        result[0]["institutionCode"].Should().Be("CGH"); // Should strip quotes
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithArrayCardinality_ShouldGenerateArray()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode
                {
                    Name = "identifiers",
                    DataType = "String",
                    Cardinality = "1 … *"
                }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result.Should().NotBeEmpty();
        result[0]["identifiers"].Should().BeAssignableTo<List<object>>();
        var array = result[0]["identifiers"] as List<object>;
        array.Should().NotBeEmpty();
        array.Should().HaveCountGreaterThanOrEqualTo(1);
        array.Should().HaveCountLessThanOrEqualTo(3);
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithRequiredField_ShouldAlwaysInclude()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 5,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode
                {
                    Name = "mandatoryField",
                    DataType = "String",
                    Cardinality = "Mandatory"
                }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result.Should().HaveCount(5);
        foreach (var sample in result)
        {
            sample.Should().ContainKey("mandatoryField");
        }
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithOptionalField_ShouldIncludeRandomly()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 10,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode
                {
                    Name = "optionalField",
                    DataType = "String",
                    Cardinality = "0 … 1"
                }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        var includeCount = result.Count(s => s.ContainsKey("optionalField"));
        includeCount.Should().BeGreaterThan(0).And.BeLessThan(10); // Should be roughly 50%
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithNestedHierarchy_ShouldPreserveStructure()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
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
                        new SchemaNode
                        {
                            Name = "identification",
                            DataType = "Grouping",
                            Cardinality = "1",
                            Children = new List<SchemaNode>
                            {
                                new SchemaNode { Name = "id", DataType = "String", Cardinality = "1" },
                                new SchemaNode { Name = "type", DataType = "String", Cardinality = "1" }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Should().ContainKey("identification");
        var identification = result[0]["identification"] as Dictionary<string, object>;
        identification.Should().NotBeNull();
        identification!.Should().ContainKey("id");
        identification.Should().ContainKey("type");
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithIntegerDataType_ShouldGenerateNumbers()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode { Name = "age", DataType = "Integer", Cardinality = "1" },
                new SchemaNode { Name = "count", DataType = "Long", Cardinality = "1" }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result[0]["age"].Should().BeOfType<int>();
        result[0]["count"].Should().BeOfType<int>();
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithBooleanDataType_ShouldGenerateBoolean()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode { Name = "isActive", DataType = "Boolean", Cardinality = "1" }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result[0]["isActive"].Should().BeOfType<bool>();
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithDateTimeDataType_ShouldGenerateDateString()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode { Name = "timestamp", DataType = "DateTime", Cardinality = "1" }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result[0]["timestamp"].Should().BeOfType<string>();
        var dateStr = result[0]["timestamp"] as string;
        dateStr.Should().Contain("T"); // ISO 8601 format
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithMultipleRecords_ShouldGenerateCorrectCount()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 5,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode { Name = "field1", DataType = "String", Cardinality = "1" }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithSchemaDefinition_ShouldGenerateData()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
            Seed = 42,
            SchemaDefinition = new SchemaDefinition
            {
                Name = "TestSchema",
                Fields = new List<SchemaField>
                {
                    new SchemaField { Name = "id", DataType = "String", IsRequired = true },
                    new SchemaField { Name = "name", DataType = "String", IsRequired = true }
                }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result.Should().NotBeEmpty();
        result[0].Should().ContainKey("id");
        result[0].Should().ContainKey("name");
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithoutSchemaDefinitionOrHierarchical_ShouldThrowException()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.GenerateSamplesAsync(request));
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithArrayOfObjects_ShouldGenerateNestedArrays()
    {
        // Arrange
        var request = new SampleGenerationRequest
        {
            RecordCount = 1,
            Seed = 42,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode
                {
                    Name = "addresses",
                    DataType = "Grouping",
                    Cardinality = "1 … *",
                    Children = new List<SchemaNode>
                    {
                        new SchemaNode { Name = "street", DataType = "String", Cardinality = "1" },
                        new SchemaNode { Name = "city", DataType = "String", Cardinality = "1" }
                    }
                }
            }
        };

        // Act
        var result = await _service.GenerateSamplesAsync(request);

        // Assert
        result[0].Should().ContainKey("addresses");
        var addresses = result[0]["addresses"] as List<Dictionary<string, object>>;
        addresses.Should().NotBeNull();
        addresses.Should().NotBeEmpty();
        addresses![0].Should().ContainKey("street");
        addresses[0].Should().ContainKey("city");
    }

    [Fact]
    public async Task GenerateSamplesAsync_WithSameSeed_ShouldGenerateSameData()
    {
        // Arrange
        var request1 = new SampleGenerationRequest
        {
            RecordCount = 1,
            Seed = 12345,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode { Name = "randomField", DataType = "Integer", Cardinality = "1" }
            }
        };

        var request2 = new SampleGenerationRequest
        {
            RecordCount = 1,
            Seed = 12345,
            HierarchicalSchema = new List<SchemaNode>
            {
                new SchemaNode { Name = "randomField", DataType = "Integer", Cardinality = "1" }
            }
        };

        // Act
        var result1 = await _service.GenerateSamplesAsync(request1);
        var result2 = await _service.GenerateSamplesAsync(request2);

        // Assert
        result1[0]["randomField"].Should().Be(result2[0]["randomField"]);
    }
}
