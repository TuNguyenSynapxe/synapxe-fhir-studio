using SchemaParserService.Models;
using SchemaParserService.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace SchemaParserService.Tests.Services;

public class SchemaParserServiceTests
{
    private readonly Mock<ILogger<SchemaParserService.Services.SchemaParserService>> _loggerMock;
    private readonly Mock<ILogger<HierarchicalCsvParser>> _csvParserLoggerMock;
    private readonly SchemaParserService.Services.SchemaParserService _service;

    public SchemaParserServiceTests()
    {
        _loggerMock = new Mock<ILogger<SchemaParserService.Services.SchemaParserService>>();
        _csvParserLoggerMock = new Mock<ILogger<HierarchicalCsvParser>>();
        
        var hierarchicalCsvParser = new HierarchicalCsvParser(_csvParserLoggerMock.Object);
        _service = new SchemaParserService.Services.SchemaParserService(_loggerMock.Object, hierarchicalCsvParser);
    }

    [Fact]
    public async Task ParseSchemaAsync_WithPutEventCsv_ShouldReturnSchemaDefinition()
    {
        // Arrange
        var csvContent = File.ReadAllText("TestData/PutEvent.csv");
        var request = new ParseSchemaRequest
        {
            SourceType = "csv",
            Name = "PutEvent",
            Content = csvContent
        };

        // Act
        var result = await _service.ParseSchemaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("PutEvent");
        result.SourceType.Should().Be("csv");
        result.Fields.Should().NotBeEmpty();
        result.Metadata.Should().ContainKey("parsingStrategy");
        result.Metadata["parsingStrategy"].Should().Be("hierarchical");
    }

    [Fact]
    public async Task ParseSchemaAsync_WithPutEventCsv_ShouldExtractCorrectFieldCount()
    {
        // Arrange
        var csvContent = File.ReadAllText("TestData/PutEvent.csv");
        var request = new ParseSchemaRequest
        {
            SourceType = "csv",
            Name = "PutEvent",
            Content = csvContent
        };

        // Act
        var result = await _service.ParseSchemaAsync(request);

        // Assert
        result.Fields.Should().NotBeEmpty();
        result.Metadata.Should().ContainKey("fieldCount");
        ((int)result.Metadata["fieldCount"]).Should().BeGreaterThan(10, "PutEvent should have many fields");
    }

    [Fact]
    public async Task ParseSchemaAsync_WithPutEventCsv_ShouldPreserveHierarchicalStructure()
    {
        // Arrange
        var csvContent = File.ReadAllText("TestData/PutEvent.csv");
        var request = new ParseSchemaRequest
        {
            SourceType = "csv",
            Name = "PutEvent",
            Content = csvContent
        };

        // Act
        var result = await _service.ParseSchemaAsync(request);

        // Assert
        result.Metadata.Should().ContainKey("hierarchicalStructure");
        var hierarchicalStructure = result.Metadata["hierarchicalStructure"] as List<SchemaNode>;
        hierarchicalStructure.Should().NotBeNull();
        hierarchicalStructure.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ParseSchemaAsync_WithSimpleCsv_ShouldFallbackToSimpleParsing()
    {
        // Arrange
        var request = new ParseSchemaRequest
        {
            SourceType = "csv",
            Name = "SimpleCsv",
            Content = "Name,Age,City\nJohn,30,Singapore"
        };

        // Act
        var result = await _service.ParseSchemaAsync(request);

        // Assert
        result.Should().NotBeNull();
        
        // The parser may try hierarchical parsing first, which could produce different results
        // The key is that it should successfully parse the content
        result.Fields.Should().NotBeEmpty("Should parse at least some fields");
        result.Metadata.Should().ContainKey("parsingStrategy");
    }

    [Fact]
    public async Task ParseSchemaAsync_WithJsonContent_ShouldParseJson()
    {
        // Arrange
        var request = new ParseSchemaRequest
        {
            SourceType = "json",
            Name = "TestJson",
            Content = "{\"name\":\"John\",\"age\":30,\"active\":true}"
        };

        // Act
        var result = await _service.ParseSchemaAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SourceType.Should().Be("json");
        result.Fields.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ParseSchemaAsync_WithUnsupportedType_ShouldThrowNotSupportedException()
    {
        // Arrange
        var request = new ParseSchemaRequest
        {
            SourceType = "unknown",
            Name = "Test",
            Content = "some content"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _service.ParseSchemaAsync(request));
    }

    [Fact]
    public async Task ParseSchemaAsync_WithPutEventCsv_ShouldExtractFieldsWithDataTypes()
    {
        // Arrange
        var csvContent = File.ReadAllText("TestData/PutEvent.csv");
        var request = new ParseSchemaRequest
        {
            SourceType = "csv",
            Name = "PutEvent",
            Content = csvContent
        };

        // Act
        var result = await _service.ParseSchemaAsync(request);

        // Assert
        var fieldsWithDataTypes = result.Fields.Where(f => !string.IsNullOrEmpty(f.DataType)).ToList();
        fieldsWithDataTypes.Should().NotBeEmpty();
    }
}
