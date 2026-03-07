namespace Ivy.Agent.EfQuery.Test;

public class SqlValidatorTests
{
    [Theory]
    [InlineData("SELECT * FROM Products")]
    [InlineData("SELECT Name FROM Products WHERE Price > 10")]
    [InlineData("  SELECT TOP 100 * FROM Orders")]
    [InlineData("select count(*) from Customers")]
    public void Validate_ValidSelectStatements_ReturnsNull(string sql)
    {
        Assert.Null(SqlValidator.Validate(sql));
    }

    [Fact]
    public void Validate_InsertStatement_ReturnsError()
    {
        var error = SqlValidator.Validate("INSERT INTO Products (Name) VALUES ('test')");
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_DeleteStatement_ReturnsError()
    {
        var error = SqlValidator.Validate("DELETE FROM Products WHERE Id = 1");
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_DropTable_ReturnsError()
    {
        var error = SqlValidator.Validate("DROP TABLE Products");
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_UpdateStatement_ReturnsError()
    {
        var error = SqlValidator.Validate("UPDATE Products SET Price = 0");
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_MultipleStatements_ReturnsError()
    {
        var error = SqlValidator.Validate("SELECT 1; DROP TABLE Products");
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_LineComment_ReturnsError()
    {
        var error = SqlValidator.Validate("SELECT * FROM Products -- this is a comment");
        Assert.NotNull(error);
        Assert.Contains("comment", error);
    }

    [Fact]
    public void Validate_BlockComment_ReturnsError()
    {
        var error = SqlValidator.Validate("SELECT * FROM Products /* hidden */");
        Assert.NotNull(error);
        Assert.Contains("comment", error);
    }

    [Fact]
    public void Validate_NotStartingWithSelect_ReturnsError()
    {
        var error = SqlValidator.Validate("WITH cte AS (SELECT 1) SELECT * FROM cte");
        Assert.NotNull(error);
        Assert.Contains("SELECT", error);
    }

    [Fact]
    public void Validate_AlterStatement_ReturnsError()
    {
        var error = SqlValidator.Validate("ALTER TABLE Products ADD COLUMN Foo INT");
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_CreateStatement_ReturnsError()
    {
        var error = SqlValidator.Validate("CREATE TABLE Evil (Id INT)");
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_TruncateStatement_ReturnsError()
    {
        var error = SqlValidator.Validate("TRUNCATE TABLE Products");
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_ExecStatement_ReturnsError()
    {
        var error = SqlValidator.Validate("EXEC sp_executesql N'SELECT 1'");
        Assert.NotNull(error);
    }
}
