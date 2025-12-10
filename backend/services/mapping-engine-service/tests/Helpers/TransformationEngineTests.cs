using MappingEngineService.Helpers;

namespace MappingEngineService.Tests.Helpers;

public class TransformationEngineTests
{
    private readonly TransformationEngine _engine;

    public TransformationEngineTests()
    {
        _engine = new TransformationEngine();
    }

    [Fact]
    public void Transform_Uppercase_ReturnsUppercase()
    {
        // Arrange
        var input = "john";

        // Act
        var result = _engine.Transform(input, "UPPERCASE");

        // Assert
        result.Should().Be("JOHN");
    }

    [Fact]
    public void Transform_Lowercase_ReturnsLowercase()
    {
        // Arrange
        var input = "JOHN";

        // Act
        var result = _engine.Transform(input, "LOWERCASE");

        // Assert
        result.Should().Be("john");
    }

    [Fact]
    public void Transform_Prefix_AddsPrefixToValue()
    {
        // Arrange
        var input = "Smith";

        // Act
        var result = _engine.Transform(input, "PREFIX:Mr. ");

        // Assert
        result.Should().Be("Mr. Smith");
    }

    [Fact]
    public void Transform_Suffix_AddsSuffixToValue()
    {
        // Arrange
        var input = "John";

        // Act
        var result = _engine.Transform(input, "SUFFIX: Jr.");

        // Assert
        result.Should().Be("John Jr.");
    }

    [Fact]
    public void Transform_Constant_ReturnsConstantValue()
    {
        // Arrange
        var input = "anything";

        // Act
        var result = _engine.Transform(input, "CONSTANT:fixed-value");

        // Assert
        result.Should().Be("fixed-value");
    }

    [Fact]
    public void Transform_Replace_ReplacesSubstring()
    {
        // Arrange
        var input = "John Doe";

        // Act
        var result = _engine.Transform(input, "REPLACE:Doe,Smith");

        // Assert
        result.Should().Be("John Smith");
    }

    [Fact]
    public void Transform_Default_ReturnsInputWhenNotEmpty()
    {
        // Arrange
        var input = "John";

        // Act
        var result = _engine.Transform(input, "DEFAULT:Unknown");

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void Transform_Default_ReturnsDefaultWhenEmpty()
    {
        // Arrange
        var input = "";

        // Act
        var result = _engine.Transform(input, "DEFAULT:Unknown");

        // Assert
        result.Should().Be("Unknown");
    }

    [Fact]
    public void Transform_NoExpression_ReturnsOriginalValue()
    {
        // Arrange
        var input = "John";

        // Act
        var result = _engine.Transform(input, null);

        // Assert
        result.Should().Be("John");
    }

    [Fact]
    public void Transform_UnknownExpression_ReturnsOriginalValue()
    {
        // Arrange
        var input = "John";

        // Act
        var result = _engine.Transform(input, "UNKNOWN:something");

        // Assert
        result.Should().Be("John");
    }
}
