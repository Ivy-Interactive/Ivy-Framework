using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database.Migrations;

public class Migration_007_FtsSourceUrl : IMigration
{
    public int Version => 7;
    public string Description => "Add SourceUrl to FTS5 PlanSearch index";

    public void Apply(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            DROP TRIGGER IF EXISTS plans_fts_insert;
            DROP TRIGGER IF EXISTS plans_fts_update;
            DROP TRIGGER IF EXISTS plans_fts_delete;
            DROP TABLE IF EXISTS PlanSearch;

            CREATE VIRTUAL TABLE PlanSearch USING fts5(
                Title,
                LatestRevisionContent,
                Project,
                InitialPrompt,
                SourceUrl,
                content='Plans',
                content_rowid=Id
            );

            CREATE TRIGGER plans_fts_insert AFTER INSERT ON Plans BEGIN
                INSERT INTO PlanSearch(rowid, Title, LatestRevisionContent, Project, InitialPrompt, SourceUrl)
                VALUES (new.Id, new.Title, new.LatestRevisionContent, new.Project, new.InitialPrompt, new.SourceUrl);
            END;

            CREATE TRIGGER plans_fts_update AFTER UPDATE ON Plans BEGIN
                INSERT INTO PlanSearch(PlanSearch, rowid, Title, LatestRevisionContent, Project, InitialPrompt, SourceUrl)
                VALUES ('delete', old.Id, old.Title, old.LatestRevisionContent, old.Project, old.InitialPrompt, old.SourceUrl);
                INSERT INTO PlanSearch(rowid, Title, LatestRevisionContent, Project, InitialPrompt, SourceUrl)
                VALUES (new.Id, new.Title, new.LatestRevisionContent, new.Project, new.InitialPrompt, new.SourceUrl);
            END;

            CREATE TRIGGER plans_fts_delete AFTER DELETE ON Plans BEGIN
                INSERT INTO PlanSearch(PlanSearch, rowid, Title, LatestRevisionContent, Project, InitialPrompt, SourceUrl)
                VALUES ('delete', old.Id, old.Title, old.LatestRevisionContent, old.Project, old.InitialPrompt, old.SourceUrl);
            END;
            """;
        cmd.ExecuteNonQuery();

        // Repopulate FTS5 index with SourceUrl
        using var countCmd = connection.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM Plans";
        var planCount = Convert.ToInt32(countCmd.ExecuteScalar(), CultureInfo.InvariantCulture);

        if (planCount > 0)
        {
            Console.WriteLine($"  Repopulating FTS5 index with {planCount} plans (now includes SourceUrl)...");
            using var populateCmd = connection.CreateCommand();
            populateCmd.CommandText = """
                INSERT INTO PlanSearch(rowid, Title, LatestRevisionContent, Project, InitialPrompt, SourceUrl)
                SELECT Id, Title, LatestRevisionContent, Project, InitialPrompt, SourceUrl FROM Plans;
                """;
            populateCmd.ExecuteNonQuery();
        }

        using var setVersionCmd = connection.CreateCommand();
        setVersionCmd.CommandText = "PRAGMA user_version = 7;";
        setVersionCmd.ExecuteNonQuery();
    }
}
