using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database.Migrations;

public class Migration_001_InitialSchema : IMigration
{
    public int Version => 1;
    public string Description => "Initial schema from plan 01967";

    public void Apply(SqliteConnection connection)
    {
        bool databaseExists;
        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Plans';";
            databaseExists = checkCmd.ExecuteScalar() != null;
        }

        if (databaseExists)
        {
            Console.WriteLine("  Existing schema detected, setting version to 1");
            using var versionCmd = connection.CreateCommand();
            versionCmd.CommandText = "PRAGMA user_version = 1;";
            versionCmd.ExecuteNonQuery();
            return;
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE Plans (
                Id INTEGER PRIMARY KEY,
                Title TEXT NOT NULL,
                Project TEXT NOT NULL,
                Level TEXT NOT NULL,
                State TEXT NOT NULL,
                FolderPath TEXT NOT NULL UNIQUE,
                FolderName TEXT NOT NULL,
                YamlRaw TEXT NOT NULL,
                RevisionCount INTEGER NOT NULL DEFAULT 1,
                LatestRevisionContent TEXT NOT NULL DEFAULT '',
                Created TEXT NOT NULL,
                Updated TEXT NOT NULL,
                InitialPrompt TEXT
            );

            CREATE INDEX idx_plans_state ON Plans(State);
            CREATE INDEX idx_plans_project ON Plans(Project);
            CREATE INDEX idx_plans_updated ON Plans(Updated DESC);

            CREATE TABLE Repos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                RepoPath TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX idx_repos_plan ON Repos(PlanId);

            CREATE TABLE Commits (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                CommitHash TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX idx_commits_plan ON Commits(PlanId);

            CREATE TABLE PullRequests (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                PrUrl TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX idx_prs_plan ON PullRequests(PlanId);

            CREATE TABLE Verifications (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                Name TEXT NOT NULL,
                Status TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX idx_verifications_plan ON Verifications(PlanId);

            CREATE TABLE RelatedPlans (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                RelatedPlanPath TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX idx_related_plan ON RelatedPlans(PlanId);

            CREATE TABLE DependsOn (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                DependsOnPlanPath TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX idx_depends_plan ON DependsOn(PlanId);

            CREATE TABLE Costs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                Promptware TEXT NOT NULL,
                Tokens INTEGER NOT NULL,
                Cost REAL NOT NULL,
                LogTimestamp TEXT,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX idx_costs_plan ON Costs(PlanId);

            CREATE TABLE Recommendations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                State TEXT NOT NULL DEFAULT 'Pending',
                DeclineReason TEXT,
                PlanTitle TEXT NOT NULL DEFAULT '',
                PlanFolderName TEXT NOT NULL DEFAULT '',
                Project TEXT NOT NULL DEFAULT '',
                Date TEXT NOT NULL,
                SourcePlanStatus TEXT NOT NULL DEFAULT 'Draft',
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX idx_recommendations_plan ON Recommendations(PlanId);
            CREATE INDEX idx_recommendations_state ON Recommendations(State);

            CREATE TABLE SyncMetadata (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();

        using var setVersionCmd = connection.CreateCommand();
        setVersionCmd.CommandText = "PRAGMA user_version = 1;";
        setVersionCmd.ExecuteNonQuery();
    }
}
