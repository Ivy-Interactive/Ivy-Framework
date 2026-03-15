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

    [Fact]
    public void Validate_MergeStatement_ReturnsError()
    {
        var error = SqlValidator.Validate("MERGE INTO Products USING Source ON 1=1 WHEN MATCHED THEN UPDATE SET Name='x'");
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_MergeKeywordInSelect_ReturnsError()
    {
        // MERGE hidden inside a subquery-like construct after SELECT
        var error = SqlValidator.Validate("SELECT 1; MERGE INTO Products USING Source ON 1=1 WHEN MATCHED THEN UPDATE SET Name='x'");
        Assert.NotNull(error);
    }

    // Comment stripping tests

    [Fact]
    public void Validate_LineCommentHidingDrop_StripsCommentAndPasses()
    {
        // "SELECT 1 -- DROP TABLE x" should pass: after stripping comments, it's just "SELECT 1"
        var error = SqlValidator.Validate("SELECT 1 -- DROP TABLE x");
        Assert.Null(error);
    }

    [Fact]
    public void Validate_BlockCommentHidingDrop_StripsCommentAndPasses()
    {
        // "SELECT 1 /* DROP TABLE x */" should pass: after stripping comments, it's "SELECT 1"
        var error = SqlValidator.Validate("SELECT 1 /* DROP TABLE x */");
        Assert.Null(error);
    }

    [Fact]
    public void Validate_DropHiddenInsideBlockComment_DetectedAfterStrip()
    {
        // "SELECT /* */ DROP TABLE x" - after stripping comment, becomes "SELECT DROP TABLE x" which has DROP
        var error = SqlValidator.Validate("SELECT /* */ DROP TABLE x");
        Assert.NotNull(error);
        Assert.Contains("disallowed", error);
    }

    [Fact]
    public void Validate_DangerousKeywordAfterLineComment_DetectedAfterStrip()
    {
        // Dangerous keyword is NOT inside comment, it's on the next line
        var error = SqlValidator.Validate("SELECT 1\n-- comment\nUNION ALL SELECT * FROM (DELETE FROM x)");
        Assert.NotNull(error);
    }

    [Fact]
    public void StripComments_RemovesLineComments()
    {
        var result = SqlValidator.StripComments("SELECT 1 -- this is a comment");
        Assert.DoesNotContain("--", result);
        Assert.DoesNotContain("this is a comment", result);
        Assert.Contains("SELECT 1", result);
    }

    [Fact]
    public void StripComments_RemovesBlockComments()
    {
        var result = SqlValidator.StripComments("SELECT /* hidden */ 1");
        Assert.DoesNotContain("/*", result);
        Assert.DoesNotContain("hidden", result);
        Assert.Contains("SELECT", result);
        Assert.Contains("1", result);
    }

    [Fact]
    public void StripComments_CollapsesWhitespace()
    {
        var result = SqlValidator.StripComments("SELECT   1   FROM   Products");
        Assert.DoesNotContain("  ", result);
    }
}
