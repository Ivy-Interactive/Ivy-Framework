using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database.Migrations;

public class Migration_002_Fts5Search : IMigration
{
    public int Version => 2;
    public string Description => "Add FTS5 full-text search for plans";

    public void Apply(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE VIRTUAL TABLE PlanSearch USING fts5(
                Title,
                LatestRevisionContent,
                Project,
                InitialPrompt,
                content='Plans',
                content_rowid=Id
            );

            CREATE TRIGGER plans_fts_insert AFTER INSERT ON Plans BEGIN
                INSERT INTO PlanSearch(rowid, Title, LatestRevisionContent, Project, InitialPrompt)
                VALUES (new.Id, new.Title, new.LatestRevisionContent, new.Project, new.InitialPrompt);
            END;

            CREATE TRIGGER plans_fts_update AFTER UPDATE ON Plans BEGIN
                INSERT INTO PlanSearch(PlanSearch, rowid, Title, LatestRevisionContent, Project, InitialPrompt)
                VALUES ('delete', old.Id, old.Title, old.LatestRevisionContent, old.Project, old.InitialPrompt);
                INSERT INTO PlanSearch(rowid, Title, LatestRevisionContent, Project, InitialPrompt)
                VALUES (new.Id, new.Title, new.LatestRevisionContent, new.Project, new.InitialPrompt);
            END;

            CREATE TRIGGER plans_fts_delete AFTER DELETE ON Plans BEGIN
                INSERT INTO PlanSearch(PlanSearch, rowid, Title, LatestRevisionContent, Project, InitialPrompt)
                VALUES ('delete', old.Id, old.Title, old.LatestRevisionContent, old.Project, old.InitialPrompt);
            END;
            """;
        cmd.ExecuteNonQuery();

        // Populate FTS5 index from existing plans
        using var countCmd = connection.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM Plans";
        var planCount = Convert.ToInt32(countCmd.ExecuteScalar(), CultureInfo.InvariantCulture);

        if (planCount > 0)
        {
            Console.WriteLine($"  Populating FTS5 index with {planCount} existing plans...");
            using var populateCmd = connection.CreateCommand();
            populateCmd.CommandText = """
                INSERT INTO PlanSearch(rowid, Title, LatestRevisionContent, Project, InitialPrompt)
                SELECT Id, Title, LatestRevisionContent, Project, InitialPrompt FROM Plans;
                """;
            populateCmd.ExecuteNonQuery();
        }

        using var setVersionCmd = connection.CreateCommand();
        setVersionCmd.CommandText = "PRAGMA user_version = 2;";
        setVersionCmd.ExecuteNonQuery();
    }
}
