using System.ComponentModel;
using System.Text.Json;
using Ivy.Core.Helpers;

namespace Ivy.Test;

public class JsonEnumConverterTests
{
    // Test enum with Description attributes (like DatabaseNamingConvention)
    private enum TestEnum
    {
        [Description("PascalCase")]
        PascalCase,

        [Description("camelCase")]
        CamelCase,

        [Description("snake_case")]
        SnakeCase,

        // Enum without description to test fallback
        NoDescription
    }

    private readonly JsonSerializerOptions _options;

    public JsonEnumConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new JsonEnumConverter() }
        };
    }

    [Fact]
    public void Write_ShouldSerializeEnumAsName_NotDescription()
    {
        // Arrange
        var enumValue = TestEnum.SnakeCase;

        // Act
        var json = JsonSerializer.Serialize(enumValue, _options);

        // Assert - Should serialize as enum name "SnakeCase", NOT description "snake_case"
        Assert.Equal("\"SnakeCase\"", json);
    }

    [Fact]
    public void Write_AllEnumValues_ShouldSerializeAsNames()
    {
        // Test all enum values to ensure they serialize as names, not descriptions
        var testCases = new[]
        {
            (TestEnum.PascalCase, "\"PascalCase\""),
            (TestEnum.CamelCase, "\"CamelCase\""),
            (TestEnum.SnakeCase, "\"SnakeCase\""),
            (TestEnum.NoDescription, "\"NoDescription\"")
        };

        foreach (var (enumValue, expectedJson) in testCases)
        {
            // Act
            var json = JsonSerializer.Serialize(enumValue, _options);

            // Assert
            Assert.Equal(expectedJson, json);
        }
    }

    [Fact]
    public void Read_ShouldAcceptEnumName()
    {
        // Arrange
        var json = "\"SnakeCase\"";

        // Act
        var result = JsonSerializer.Deserialize<TestEnum>(json, _options);

        // Assert
        Assert.Equal(TestEnum.SnakeCase, result);
    }

    [Fact]
    public void Read_ShouldAcceptDescription_ForBackwardCompatibility()
    {
        // Arrange - JSON contains description, not enum name
        var json = "\"snake_case\"";

        // Act
        var result = JsonSerializer.Deserialize<TestEnum>(json, _options);

        // Assert - Should still parse correctly for backward compatibility
        Assert.Equal(TestEnum.SnakeCase, result);
    }

    [Fact]
    public void Read_ShouldAcceptBothDescriptionAndName()
    {
        var testCases = new[]
        {
            ("\"PascalCase\"", TestEnum.PascalCase),    // Enum name
            ("\"PascalCase\"", TestEnum.PascalCase),    // Description (same as name)
            ("\"CamelCase\"", TestEnum.CamelCase),      // Enum name  
            ("\"camelCase\"", TestEnum.CamelCase),      // Description
            ("\"SnakeCase\"", TestEnum.SnakeCase),      // Enum name
            ("\"snake_case\"", TestEnum.SnakeCase),     // Description
            ("\"NoDescription\"", TestEnum.NoDescription) // No description attribute
        };

        foreach (var (json, expectedEnum) in testCases)
        {
            // Act
            var result = JsonSerializer.Deserialize<TestEnum>(json, _options);

            // Assert
            Assert.Equal(expectedEnum, result);
        }
    }

    [Fact]
    public void Read_InvalidValue_ShouldThrowJsonException()
    {
        // Arrange
        var json = "\"InvalidEnumValue\"";

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TestEnum>(json, _options));
    }

    [Fact]
    public void RoundTrip_EnumValue_ShouldMaintainCorrectValue()
    {
        // This test proves the fix: enum should serialize as name and deserialize correctly

        // Arrange
        var originalValue = TestEnum.SnakeCase;

        // Act - Serialize then deserialize
        var json = JsonSerializer.Serialize(originalValue, _options);
        var roundTripValue = JsonSerializer.Deserialize<TestEnum>(json, _options);

        // Assert
        Assert.Equal(originalValue, roundTripValue);
        // Verify it serialized as enum name, not description
        Assert.Equal("\"SnakeCase\"", json);
    }

    [Fact]
    public void ProveTheBugWasFixed_DescriptionVsEnumName()
    {
        // This test specifically proves the bug that was causing the frontend issue

        // Arrange
        var enumValue = TestEnum.SnakeCase;

        // Act
        var serializedJson = JsonSerializer.Serialize(enumValue, _options);

        // Assert - Before fix: would serialize as "snake_case" (description)
        //         After fix: serializes as "SnakeCase" (enum name)
        Assert.Equal("\"SnakeCase\"", serializedJson);
        Assert.NotEqual("\"snake_case\"", serializedJson);

        // Verify the frontend would receive the correct value
        // Frontend expects: {label: "snake_case", value: "SnakeCase"}
        // Before fix: backend sent {label: "snake_case", value: "snake_case"} ❌
        // After fix: backend sends {label: "snake_case", value: "SnakeCase"} ✅
    }

    [Fact]
    public void DatabaseNamingConvention_SimulateRealScenario()
    {
        // This simulates the exact DatabaseNamingConvention enum from SelectInputApp

        // Arrange - Create enum similar to DatabaseNamingConvention
        var testEnum = TestEnum.SnakeCase; // Represents DatabaseNamingConvention.SnakeCase

        // Act - Simulate what happens when backend serializes current state
        var currentStateJson = JsonSerializer.Serialize(testEnum, _options);

        // Assert - This is what frontend receives as current value
        Assert.Equal("\"SnakeCase\"", currentStateJson);

        // Simulate frontend sending this back to backend
        var backendReceives = JsonSerializer.Deserialize<TestEnum>(currentStateJson, _options);
        Assert.Equal(TestEnum.SnakeCase, backendReceives);

        // Before fix: frontend would receive "snake_case" and send it back,
        // causing "Requested value 'snake_case' was not found" error
        // After fix: frontend receives "SnakeCase" and sends it back successfully
    }
}
