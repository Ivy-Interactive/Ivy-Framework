using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ivy.Test;

[Table("TestUser")]
public class TestUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? Bio { get; set; }

    public ICollection<TestPost> Posts { get; set; } = null!;
}

[Table("TestPost")]
public class TestPost
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AuthorId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [ForeignKey(nameof(AuthorId))]
    public TestUser Author { get; set; } = null!;
}

public class TypeDescriberTests
{
    [Fact]
    public void Describe_SingleType_ContainsTypeName()
    {
        var result = TypeDescriber.Describe(typeof(TestUser));
        Assert.Contains("TestUser", result);
    }

    [Fact]
    public void Describe_SingleType_ContainsTableAnnotation()
    {
        var result = TypeDescriber.Describe(typeof(TestUser));
        Assert.Contains("(Table: TestUser)", result);
    }

    [Fact]
    public void Describe_SingleType_ShowsPrimaryKey()
    {
        var result = TypeDescriber.Describe(typeof(TestUser));
        Assert.Contains("[PK, Identity]", result);
    }

    [Fact]
    public void Describe_SingleType_ShowsRequired()
    {
        var result = TypeDescriber.Describe(typeof(TestUser));
        Assert.Contains("[Required]", result);
    }

    [Fact]
    public void Describe_SingleType_ShowsNullable()
    {
        var result = TypeDescriber.Describe(typeof(TestUser));
        Assert.Contains("string?", result);
    }

    [Fact]
    public void Describe_SingleType_ShowsCollection()
    {
        var result = TypeDescriber.Describe(typeof(TestUser));
        Assert.Contains("Collection<TestPost>", result);
    }

    [Fact]
    public void Describe_SingleType_ShowsForeignKey()
    {
        var result = TypeDescriber.Describe(typeof(TestPost));
        Assert.Contains("FK -> TestUser", result);
    }

    [Fact]
    public void Describe_SingleType_ShowsMaxLength()
    {
        var result = TypeDescriber.Describe(typeof(TestPost));
        Assert.Contains("MaxLength(200)", result);
    }

    [Fact]
    public void Describe_MultipleTypes_ContainsAllTypes()
    {
        var result = TypeDescriber.Describe([typeof(TestUser), typeof(TestPost)]);
        Assert.Contains("TestUser", result);
        Assert.Contains("TestPost", result);
    }

    [Fact]
    public void Describe_WithHeader_IncludesHeader()
    {
        var result = TypeDescriber.Describe([typeof(TestUser)], "BlogDb Models");
        Assert.StartsWith("## BlogDb Models", result);
    }
}
