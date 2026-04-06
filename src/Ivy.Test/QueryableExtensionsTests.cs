using Ivy.Core.Helpers;

namespace Ivy.Test;

public record PositionalRecord(int Id, string Name, string Department, decimal Salary, DateTime HireDate, bool IsActive);

public class QueryableExtensionsTests
{
    private class TestModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Value { get; set; }
        public bool IsActive { get; set; }
    }

    private List<TestModel> GetTestData()
    {
        return new List<TestModel>
        {
            new TestModel { Id = 1, Name = "Item1", Description = "First item", Value = 10.5, IsActive = true },
            new TestModel { Id = 2, Name = "Item2", Description = "Second item", Value = 20.3, IsActive = false },
            new TestModel { Id = 3, Name = "Item3", Description = "Third item", Value = 30.7, IsActive = true }
        };
    }

    [Fact]
    public void RemoveFields_RemoveSingleField_FieldIsRemoved()
    {
        var data = GetTestData().AsQueryable();
        var fieldsToRemove = new[] { "Description" };

        var result = data.RemoveFields(fieldsToRemove).Cast<TestModel>().ToList();

        Assert.Equal(3, result.Count);
        foreach (var item in result)
        {
            Assert.Null(item.Description);
            Assert.NotEqual(0, item.Id);
            Assert.NotNull(item.Name);
            Assert.NotEqual(0, item.Value);
        }
    }

    [Fact]
    public void RemoveFields_RemoveMultipleFields_FieldsAreRemoved()
    {
        var data = GetTestData().AsQueryable();
        var fieldsToRemove = new[] { "Description", "Value", "IsActive" };

        var result = data.RemoveFields(fieldsToRemove).Cast<TestModel>().ToList();

        Assert.Equal(3, result.Count);
        foreach (var item in result)
        {
            Assert.Null(item.Description);
            Assert.Equal(0, item.Value);
            Assert.False(item.IsActive);
            Assert.NotEqual(0, item.Id);
            Assert.NotNull(item.Name);
        }
    }

    [Fact]
    public void RemoveFields_RemoveNonExistentField_NoError()
    {
        var data = GetTestData().AsQueryable();
        var fieldsToRemove = new[] { "NonExistentField", "Description" };

        var result = data.RemoveFields(fieldsToRemove).Cast<TestModel>().ToList();

        Assert.Equal(3, result.Count);
        foreach (var item in result)
        {
            Assert.Null(item.Description);
            Assert.NotEqual(0, item.Id);
            Assert.NotNull(item.Name);
            Assert.NotEqual(0, item.Value);
        }
    }

    [Fact]
    public void RemoveFields_EmptyFieldsArray_NoFieldsRemoved()
    {
        var data = GetTestData().AsQueryable();
        var fieldsToRemove = new string[] { };

        var result = data.RemoveFields(fieldsToRemove).Cast<TestModel>().ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Item1", result[0].Name);
        Assert.Equal("First item", result[0].Description);
        Assert.Equal(10.5, result[0].Value);
        Assert.True(result[0].IsActive);
    }

    [Fact]
    public void RemoveFields_AllFieldsRemoved_AllPropertiesDefault()
    {
        var data = GetTestData().AsQueryable();
        var fieldsToRemove = new[] { "Id", "Name", "Description", "Value", "IsActive" };

        var result = data.RemoveFields(fieldsToRemove).Cast<TestModel>().ToList();

        Assert.Equal(3, result.Count);
        foreach (var item in result)
        {
            Assert.Equal(0, item.Id);
            Assert.Null(item.Name);
            Assert.Null(item.Description);
            Assert.Equal(0, item.Value);
            Assert.False(item.IsActive);
        }
    }

    [Fact]
    public void RemoveFields_PositionalRecord_RemovedFieldGetsDefault()
    {
        var data = new List<PositionalRecord>
        {
            new(1, "Alice", "Engineering", 90000m, new DateTime(2023, 1, 15), true),
            new(2, "Bob", "Marketing", 75000m, new DateTime(2024, 6, 1), false),
        }.AsQueryable();

        var result = data.RemoveFields(["Id"]).Cast<PositionalRecord>().ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(0, result[0].Id);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Engineering", result[0].Department);
        Assert.Equal(90000m, result[0].Salary);
        Assert.Equal(0, result[1].Id);
        Assert.Equal("Bob", result[1].Name);
    }

    [Fact]
    public void RemoveFields_PositionalRecord_RemoveMultipleFields()
    {
        var data = new List<PositionalRecord>
        {
            new(1, "Alice", "Engineering", 90000m, new DateTime(2023, 1, 15), true),
        }.AsQueryable();

        var result = data.RemoveFields(["Id", "HireDate", "IsActive"]).Cast<PositionalRecord>().ToList();

        Assert.Single(result);
        Assert.Equal(0, result[0].Id);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Engineering", result[0].Department);
        Assert.Equal(90000m, result[0].Salary);
        Assert.Equal(default, result[0].HireDate);
        Assert.False(result[0].IsActive);
    }
}
