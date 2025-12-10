using MappingEngineService.Helpers;
using Newtonsoft.Json.Linq;

namespace MappingEngineService.Tests.Helpers;

public class JsonPathResolverTests
{
    private readonly JsonPathResolver _resolver;

    public JsonPathResolverTests()
    {
        _resolver = new JsonPathResolver();
    }

    [Fact]
    public void GetValue_SimpleProperty_ReturnsValue()
    {
        // Arrange
        var source = JObject.Parse(@"{ ""name"": ""John"" }");

        // Act
        var result = _resolver.GetValue(source, "name");

        // Assert
        result.Should().NotBeNull();
        result!.ToString().Should().Be("John");
    }

    [Fact]
    public void GetValue_NestedProperty_ReturnsValue()
    {
        // Arrange
        var source = JObject.Parse(@"{ ""patient"": { ""name"": ""John"" } }");

        // Act
        var result = _resolver.GetValue(source, "patient.name");

        // Assert
        result.Should().NotBeNull();
        result!.ToString().Should().Be("John");
    }

    [Fact]
    public void GetValue_MissingPath_ReturnsNull()
    {
        // Arrange
        var source = JObject.Parse(@"{ ""name"": ""John"" }");

        // Act
        var result = _resolver.GetValue(source, "age");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SetValue_SimpleProperty_SetsValue()
    {
        // Arrange
        var target = new JObject();

        // Act
        _resolver.SetValue(target, "name", new JValue("John"));

        // Assert
        target["name"]!.ToString().Should().Be("John");
    }

    [Fact]
    public void SetValue_NestedProperty_CreatesIntermediateNodes()
    {
        // Arrange
        var target = new JObject();

        // Act
        _resolver.SetValue(target, "patient.name.given", new JValue("John"));

        // Assert
        target["patient"]!["name"]!["given"]!.ToString().Should().Be("John");
    }

    [Fact]
    public void SetValue_ArrayNotation_CreatesArray()
    {
        // Arrange
        var target = new JObject();

        // Act
        _resolver.SetValue(target, "name[0].given", new JValue("John"));

        // Assert
        var array = target["name"] as JArray;
        array.Should().NotBeNull();
        array![0]!["given"]!.ToString().Should().Be("John");
    }
}
