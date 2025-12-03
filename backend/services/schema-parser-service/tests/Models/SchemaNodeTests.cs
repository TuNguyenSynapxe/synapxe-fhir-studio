using SchemaParserService.Models;

namespace SchemaParserService.Tests.Models;

public class SchemaNodeTests
{
    [Fact]
    public void ToSchemaFields_WithSingleNode_ShouldReturnOneField()
    {
        // Arrange
        var node = new SchemaNode
        {
            Name = "testField",
            Level = 0,
            DataType = "String (50)",
            Cardinality = "1",
            Definition = "Test definition"
        };

        // Act
        var fields = node.ToSchemaFields();

        // Assert
        fields.Should().HaveCount(1);
        fields[0].Name.Should().Be("testField");
        fields[0].DataType.Should().Be("String (50)");
        fields[0].IsRequired.Should().BeTrue();
        fields[0].MaxLength.Should().Be(50);
    }

    [Fact]
    public void ToSchemaFields_WithNestedNodes_ShouldFlattenHierarchy()
    {
        // Arrange
        var rootNode = new SchemaNode
        {
            Name = "parent",
            Level = 0,
            DataType = "Grouping",
            Children = new List<SchemaNode>
            {
                new SchemaNode
                {
                    Name = "child1",
                    Level = 1,
                    DataType = "String"
                },
                new SchemaNode
                {
                    Name = "child2",
                    Level = 1,
                    DataType = "Number"
                }
            }
        };

        // Act
        var fields = rootNode.ToSchemaFields();

        // Assert
        fields.Should().HaveCount(3); // parent + 2 children
        fields.Should().Contain(f => f.Name == "parent");
        fields.Should().Contain(f => f.Name == "parent.child1");
        fields.Should().Contain(f => f.Name == "parent.child2");
    }

    [Fact]
    public void ToSchemaFields_WithArrayCardinality_ShouldSetIsArrayTrue()
    {
        // Arrange
        var node = new SchemaNode
        {
            Name = "arrayField",
            Level = 0,
            DataType = "String",
            Cardinality = "0 … *"
        };

        // Act
        var fields = node.ToSchemaFields();

        // Assert
        fields.Should().HaveCount(1);
        fields[0].IsArray.Should().BeTrue();
    }

    [Fact]
    public void ToSchemaFields_WithMandatoryCardinality_ShouldSetIsRequiredTrue()
    {
        // Arrange
        var node = new SchemaNode
        {
            Name = "mandatoryField",
            Level = 0,
            DataType = "String",
            Cardinality = "Mandatory"
        };

        // Act
        var fields = node.ToSchemaFields();

        // Assert
        fields.Should().HaveCount(1);
        fields[0].IsRequired.Should().BeTrue();
    }

    [Fact]
    public void ToSchemaFields_WithOptionalCardinality_ShouldSetIsRequiredFalse()
    {
        // Arrange
        var node = new SchemaNode
        {
            Name = "optionalField",
            Level = 0,
            DataType = "String",
            Cardinality = "Optional"
        };

        // Act
        var fields = node.ToSchemaFields();

        // Assert
        fields.Should().HaveCount(1);
        fields[0].IsRequired.Should().BeFalse();
    }

    [Fact]
    public void ToSchemaFields_WithMultipleLevels_ShouldCreateCorrectPaths()
    {
        // Arrange
        var rootNode = new SchemaNode
        {
            Name = "level0",
            Level = 0,
            DataType = "Grouping",
            Children = new List<SchemaNode>
            {
                new SchemaNode
                {
                    Name = "level1",
                    Level = 1,
                    DataType = "Grouping",
                    Children = new List<SchemaNode>
                    {
                        new SchemaNode
                        {
                            Name = "level2",
                            Level = 2,
                            DataType = "String"
                        }
                    }
                }
            }
        };

        // Act
        var fields = rootNode.ToSchemaFields();

        // Assert
        fields.Should().HaveCount(3);
        fields.Should().Contain(f => f.Name == "level0");
        fields.Should().Contain(f => f.Name == "level0.level1");
        fields.Should().Contain(f => f.Name == "level0.level1.level2");
    }

    [Fact]
    public void ToSchemaFields_WithRangeCardinality_ShouldDetectArray()
    {
        // Arrange
        var node = new SchemaNode
        {
            Name = "rangeField",
            Level = 0,
            DataType = "String",
            Cardinality = "1 … 2"
        };

        // Act
        var fields = node.ToSchemaFields();

        // Assert
        fields.Should().HaveCount(1);
        fields[0].IsArray.Should().BeTrue();
        fields[0].IsRequired.Should().BeTrue(); // Starts with 1
    }
}
