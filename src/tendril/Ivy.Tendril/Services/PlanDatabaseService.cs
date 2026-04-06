using System.Globalization;
using Ivy.Tendril.Apps.Plans;
using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Services;

public class PlanDatabaseService : IPlanDatabaseService
{
    private readonly SqliteConnection _connection;

    public PlanDatabaseService(string databasePath)
    {
        _connection = new SqliteConnection($"Data Source={databasePath};Mode=ReadWriteCreate");
        _connection.Open();

        using var pragmaCmd = _connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        pragmaCmd.ExecuteNonQuery();

        EnsureSchema();
    }

    public void EnsureSchema()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Plans (
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

            CREATE INDEX IF NOT EXISTS idx_plans_state ON Plans(State);
            CREATE INDEX IF NOT EXISTS idx_plans_project ON Plans(Project);
            CREATE INDEX IF NOT EXISTS idx_plans_updated ON Plans(Updated DESC);

            CREATE TABLE IF NOT EXISTS Repos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                RepoPath TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_repos_plan ON Repos(PlanId);

            CREATE TABLE IF NOT EXISTS Commits (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                CommitHash TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_commits_plan ON Commits(PlanId);

            CREATE TABLE IF NOT EXISTS PullRequests (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                PrUrl TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_prs_plan ON PullRequests(PlanId);

            CREATE TABLE IF NOT EXISTS Verifications (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                Name TEXT NOT NULL,
                Status TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_verifications_plan ON Verifications(PlanId);

            CREATE TABLE IF NOT EXISTS RelatedPlans (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                RelatedPlanPath TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_related_plan ON RelatedPlans(PlanId);

            CREATE TABLE IF NOT EXISTS DependsOn (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                DependsOnPlanPath TEXT NOT NULL,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_depends_plan ON DependsOn(PlanId);

            CREATE TABLE IF NOT EXISTS Costs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanId INTEGER NOT NULL,
                Promptware TEXT NOT NULL,
                Tokens INTEGER NOT NULL,
                Cost REAL NOT NULL,
                LogTimestamp TEXT,
                FOREIGN KEY (PlanId) REFERENCES Plans(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_costs_plan ON Costs(PlanId);

            CREATE TABLE IF NOT EXISTS Recommendations (
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
            CREATE INDEX IF NOT EXISTS idx_recommendations_plan ON Recommendations(PlanId);
            CREATE INDEX IF NOT EXISTS idx_recommendations_state ON Recommendations(State);

            CREATE TABLE IF NOT EXISTS SyncMetadata (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    public List<PlanFile> GetPlans(PlanStatus? statusFilter = null)
    {
        var sql = """
            SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                   YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated
            FROM Plans
            """;

        if (statusFilter.HasValue)
            sql += " WHERE State = @state";

        sql += " ORDER BY Id";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        if (statusFilter.HasValue)
            cmd.Parameters.AddWithValue("@state", statusFilter.Value.ToString());

        // Collect plan rows first
        var planRows = new List<(int Id, SqliteDataReader Reader)>();
        var planIds = new List<int>();
        var plans = new List<PlanFile>();

        using var reader = cmd.ExecuteReader();
        var rawPlans = new List<(int Id, string Title, string Project, string Level, string State,
            string FolderPath, string FolderName, string YamlRaw, int RevisionCount,
            string LatestContent, string Created, string Updated)>();

        while (reader.Read())
        {
            var planId = reader.GetInt32(0);
            planIds.Add(planId);
            rawPlans.Add((planId, reader.GetString(1), reader.GetString(2), reader.GetString(3),
                reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7),
                reader.GetInt32(8), reader.GetString(9), reader.GetString(10), reader.GetString(11)));
        }

        if (rawPlans.Count == 0)
            return plans;

        // Batch-fetch all child table data
        var allRepos = BatchGetList(planIds, "Repos", "RepoPath");
        var allCommits = BatchGetList(planIds, "Commits", "CommitHash");
        var allPrs = BatchGetList(planIds, "PullRequests", "PrUrl");
        var allVerifications = BatchGetVerifications(planIds);
        var allRelatedPlans = BatchGetList(planIds, "RelatedPlans", "RelatedPlanPath");
        var allDependsOn = BatchGetList(planIds, "DependsOn", "DependsOnPlanPath");

        foreach (var row in rawPlans)
        {
            var plan = BuildPlanFileFromRow(row.Id, row.Title, row.Project, row.Level, row.State,
                row.FolderPath, row.FolderName, row.YamlRaw, row.RevisionCount, row.LatestContent,
                row.Created, row.Updated,
                allRepos.GetValueOrDefault(row.Id, []),
                allCommits.GetValueOrDefault(row.Id, []),
                allPrs.GetValueOrDefault(row.Id, []),
                allVerifications.GetValueOrDefault(row.Id, []),
                allRelatedPlans.GetValueOrDefault(row.Id, []),
                allDependsOn.GetValueOrDefault(row.Id, []));
            if (plan != null)
                plans.Add(plan);
        }

        return plans;
    }

    public PlanFile? GetPlanByFolder(string folderPath)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                   YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated
            FROM Plans WHERE FolderPath = @folderPath
            """;
        cmd.Parameters.AddWithValue("@folderPath", folderPath);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return BuildPlanFile(reader.GetInt32(0), reader);

        return null;
    }

    public PlanFile? GetPlanById(int planId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                   YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated
            FROM Plans WHERE Id = @id
            """;
        cmd.Parameters.AddWithValue("@id", planId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return BuildPlanFile(planId, reader);

        return null;
    }

    /// <summary>
    /// Builds a PlanFile for a single plan query. Fetches child tables individually.
    /// For bulk queries, use BuildPlanFileFromRow with pre-fetched child data instead.
    /// </summary>
    private PlanFile? BuildPlanFile(int planId, SqliteDataReader reader)
    {
        return BuildPlanFileFromRow(planId,
            reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
            reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetInt32(8),
            reader.GetString(9), reader.GetString(10), reader.GetString(11),
            GetListForPlan(planId, "Repos", "RepoPath"),
            GetListForPlan(planId, "Commits", "CommitHash"),
            GetListForPlan(planId, "PullRequests", "PrUrl"),
            GetVerificationsForPlan(planId),
            GetListForPlan(planId, "RelatedPlans", "RelatedPlanPath"),
            GetListForPlan(planId, "DependsOn", "DependsOnPlanPath"));
    }

    private static PlanFile? BuildPlanFileFromRow(int planId, string title, string project, string level,
        string state, string folderPath, string folderName, string yamlRaw, int revisionCount,
        string latestContent, string createdStr, string updatedStr,
        List<string> repos, List<string> commits, List<string> prs,
        List<PlanVerificationEntry> verifications, List<string> relatedPlans, List<string> dependsOn)
    {
        if (!Enum.TryParse<PlanStatus>(state, ignoreCase: true, out var status))
            status = PlanStatus.Draft;

        var created = DateTime.Parse(createdStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        var updated = DateTime.Parse(updatedStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

        var metadata = new PlanMetadata(planId, project, level, title, status,
            repos, commits, prs, verifications, relatedPlans, dependsOn, created, updated);

        return new PlanFile(metadata, latestContent, folderPath, yamlRaw, revisionCount);
    }

    private List<string> GetListForPlan(int planId, string table, string column)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT {column} FROM {table} WHERE PlanId = @planId";
        cmd.Parameters.AddWithValue("@planId", planId);

        var list = new List<string>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(reader.GetString(0));
        return list;
    }

    private List<PlanVerificationEntry> GetVerificationsForPlan(int planId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT Name, Status FROM Verifications WHERE PlanId = @planId";
        cmd.Parameters.AddWithValue("@planId", planId);

        var list = new List<PlanVerificationEntry>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new PlanVerificationEntry { Name = reader.GetString(0), Status = reader.GetString(1) });
        return list;
    }

    /// <summary>Batch-fetches a string child table for multiple plan IDs in one query.</summary>
    private Dictionary<int, List<string>> BatchGetList(List<int> planIds, string table, string column)
    {
        var result = new Dictionary<int, List<string>>();
        if (planIds.Count == 0) return result;

        using var cmd = _connection.CreateCommand();
        var idList = string.Join(",", planIds);
        cmd.CommandText = $"SELECT PlanId, {column} FROM {table} WHERE PlanId IN ({idList})";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var planId = reader.GetInt32(0);
            if (!result.TryGetValue(planId, out var list))
            {
                list = new List<string>();
                result[planId] = list;
            }
            list.Add(reader.GetString(1));
        }
        return result;
    }

    /// <summary>Batch-fetches verifications for multiple plan IDs in one query.</summary>
    private Dictionary<int, List<PlanVerificationEntry>> BatchGetVerifications(List<int> planIds)
    {
        var result = new Dictionary<int, List<PlanVerificationEntry>>();
        if (planIds.Count == 0) return result;

        using var cmd = _connection.CreateCommand();
        var idList = string.Join(",", planIds);
        cmd.CommandText = $"SELECT PlanId, Name, Status FROM Verifications WHERE PlanId IN ({idList})";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var planId = reader.GetInt32(0);
            if (!result.TryGetValue(planId, out var list))
            {
                list = new List<PlanVerificationEntry>();
                result[planId] = list;
            }
            list.Add(new PlanVerificationEntry { Name = reader.GetString(1), Status = reader.GetString(2) });
        }
        return result;
    }

    public PlanReaderService.PlanCountSnapshot ComputePlanCounts()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            SELECT
                COALESCE(SUM(CASE WHEN State = 'Draft' THEN 1 ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN State = 'ReadyForReview' THEN 1 ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN State = 'Failed' THEN 1 ELSE 0 END), 0),
                COALESCE(SUM(CASE WHEN State = 'Icebox' THEN 1 ELSE 0 END), 0),
                (SELECT COUNT(*) FROM Recommendations WHERE State = 'Pending')
            FROM Plans
            """;

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new PlanReaderService.PlanCountSnapshot(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4)
            );
        }

        return new PlanReaderService.PlanCountSnapshot(0, 0, 0, 0, 0);
    }

    public decimal GetPlanTotalCost(int planId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(SUM(Cost), 0) FROM Costs WHERE PlanId = @planId";
        cmd.Parameters.AddWithValue("@planId", planId);
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToDecimal(result, CultureInfo.InvariantCulture) : 0m;
    }

    public int GetPlanTotalTokens(int planId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(SUM(Tokens), 0) FROM Costs WHERE PlanId = @planId";
        cmd.Parameters.AddWithValue("@planId", planId);
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result, CultureInfo.InvariantCulture) : 0;
    }

    public List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            SELECT
                strftime('%Y-%m-%d %H:00:00', c.LogTimestamp) as Hour,
                p.Project,
                SUM(c.Cost) as TotalCost,
                SUM(c.Tokens) as TotalTokens
            FROM Costs c
            JOIN Plans p ON p.Id = c.PlanId
            WHERE c.LogTimestamp IS NOT NULL AND c.LogTimestamp >= @cutoff
            GROUP BY Hour, p.Project
            ORDER BY Hour, p.Project
            """;
        cmd.Parameters.AddWithValue("@cutoff", cutoff.ToString("O", CultureInfo.InvariantCulture));

        var result = new List<HourlyTokenBurn>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var hourStr = reader.GetString(0);
            if (DateTime.TryParse(hourStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var hour))
            {
                result.Add(new HourlyTokenBurn
                {
                    Hour = DateTime.SpecifyKind(hour, DateTimeKind.Utc),
                    Project = reader.GetString(1),
                    Cost = Convert.ToDecimal(reader.GetDouble(2), CultureInfo.InvariantCulture),
                    Tokens = reader.GetInt32(3)
                });
            }
        }

        return result;
    }

    public List<Recommendation> GetRecommendations()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            SELECT r.Title, r.Description, r.State, r.PlanId, r.PlanTitle,
                   r.PlanFolderName, r.Project, r.Date, r.SourcePlanStatus, r.DeclineReason
            FROM Recommendations r
            ORDER BY r.Date DESC
            """;

        var result = new List<Recommendation>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (!Enum.TryParse<PlanStatus>(reader.GetString(8), ignoreCase: true, out var sourceStatus))
                sourceStatus = PlanStatus.Draft;

            result.Add(new Recommendation(
                Title: reader.GetString(0),
                Description: reader.GetString(1),
                State: reader.GetString(2),
                PlanId: reader.GetInt32(3).ToString("D5"),
                PlanTitle: reader.GetString(4),
                PlanFolderName: reader.GetString(5),
                Project: reader.GetString(6),
                Date: DateTime.Parse(reader.GetString(7), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                SourcePlanStatus: sourceStatus,
                DeclineReason: reader.IsDBNull(9) ? null : reader.GetString(9)
            ));
        }

        return result;
    }

    public int GetPendingRecommendationsCount()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Recommendations WHERE State = 'Pending'";
        return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    public List<PlanFile> SearchPlans(string query)
    {
        var search = $"%{query}%";
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                   YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated
            FROM Plans
            WHERE Title LIKE @search OR LatestRevisionContent LIKE @search
                  OR CAST(Id AS TEXT) LIKE @search OR Project LIKE @search
            ORDER BY Id
            """;
        cmd.Parameters.AddWithValue("@search", search);

        var planIds = new List<int>();
        var rawPlans = new List<(int Id, string Title, string Project, string Level, string State,
            string FolderPath, string FolderName, string YamlRaw, int RevisionCount,
            string LatestContent, string Created, string Updated)>();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var planId = reader.GetInt32(0);
            planIds.Add(planId);
            rawPlans.Add((planId, reader.GetString(1), reader.GetString(2), reader.GetString(3),
                reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7),
                reader.GetInt32(8), reader.GetString(9), reader.GetString(10), reader.GetString(11)));
        }

        if (rawPlans.Count == 0)
            return [];

        var allRepos = BatchGetList(planIds, "Repos", "RepoPath");
        var allCommits = BatchGetList(planIds, "Commits", "CommitHash");
        var allPrs = BatchGetList(planIds, "PullRequests", "PrUrl");
        var allVerifications = BatchGetVerifications(planIds);
        var allRelatedPlans = BatchGetList(planIds, "RelatedPlans", "RelatedPlanPath");
        var allDependsOn = BatchGetList(planIds, "DependsOn", "DependsOnPlanPath");

        var plans = new List<PlanFile>();
        foreach (var row in rawPlans)
        {
            var plan = BuildPlanFileFromRow(row.Id, row.Title, row.Project, row.Level, row.State,
                row.FolderPath, row.FolderName, row.YamlRaw, row.RevisionCount, row.LatestContent,
                row.Created, row.Updated,
                allRepos.GetValueOrDefault(row.Id, []),
                allCommits.GetValueOrDefault(row.Id, []),
                allPrs.GetValueOrDefault(row.Id, []),
                allVerifications.GetValueOrDefault(row.Id, []),
                allRelatedPlans.GetValueOrDefault(row.Id, []),
                allDependsOn.GetValueOrDefault(row.Id, []));
            if (plan != null)
                plans.Add(plan);
        }

        return plans;
    }

    public void UpsertPlan(PlanFile plan)
    {
        using var transaction = _connection.BeginTransaction();
        try
        {
            UpsertPlanInternal(plan);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private void UpsertPlanInternal(PlanFile plan)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Plans (Id, Title, Project, Level, State, FolderPath, FolderName,
                               YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated)
            VALUES (@id, @title, @project, @level, @state, @folderPath, @folderName,
                    @yamlRaw, @revisionCount, @latestContent, @created, @updated)
            ON CONFLICT(Id) DO UPDATE SET
                Title = excluded.Title,
                Project = excluded.Project,
                Level = excluded.Level,
                State = excluded.State,
                FolderPath = excluded.FolderPath,
                FolderName = excluded.FolderName,
                YamlRaw = excluded.YamlRaw,
                RevisionCount = excluded.RevisionCount,
                LatestRevisionContent = excluded.LatestRevisionContent,
                Created = excluded.Created,
                Updated = excluded.Updated
            """;

        cmd.Parameters.AddWithValue("@id", plan.Id);
        cmd.Parameters.AddWithValue("@title", plan.Title);
        cmd.Parameters.AddWithValue("@project", plan.Project);
        cmd.Parameters.AddWithValue("@level", plan.Level);
        cmd.Parameters.AddWithValue("@state", plan.Status.ToString());
        cmd.Parameters.AddWithValue("@folderPath", plan.FolderPath);
        cmd.Parameters.AddWithValue("@folderName", plan.FolderName);
        cmd.Parameters.AddWithValue("@yamlRaw", plan.PlanYamlRaw);
        cmd.Parameters.AddWithValue("@revisionCount", plan.RevisionCount);
        cmd.Parameters.AddWithValue("@latestContent", plan.LatestRevisionContent);
        cmd.Parameters.AddWithValue("@created", plan.Created.ToString("O", CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("@updated", plan.Updated.ToString("O", CultureInfo.InvariantCulture));

        cmd.ExecuteNonQuery();

        // Sync child tables
        SyncChildTable(plan.Id, "Repos", "RepoPath", plan.Repos);
        SyncChildTable(plan.Id, "Commits", "CommitHash", plan.Commits);
        SyncChildTable(plan.Id, "PullRequests", "PrUrl", plan.Prs);
        SyncVerifications(plan.Id, plan.Verifications);
        SyncChildTable(plan.Id, "RelatedPlans", "RelatedPlanPath", plan.RelatedPlans);
        SyncChildTable(plan.Id, "DependsOn", "DependsOnPlanPath", plan.DependsOn);
    }

    private void SyncChildTable(int planId, string table, string column, List<string> values)
    {
        using var deleteCmd = _connection.CreateCommand();
        deleteCmd.CommandText = $"DELETE FROM {table} WHERE PlanId = @planId";
        deleteCmd.Parameters.AddWithValue("@planId", planId);
        deleteCmd.ExecuteNonQuery();

        if (values.Count == 0) return;

        using var insertCmd = _connection.CreateCommand();
        insertCmd.CommandText = $"INSERT INTO {table} (PlanId, {column}) VALUES (@planId, @value)";
        insertCmd.Parameters.AddWithValue("@planId", planId);
        insertCmd.Parameters.AddWithValue("@value", string.Empty);

        foreach (var value in values)
        {
            insertCmd.Parameters["@planId"].Value = planId;
            insertCmd.Parameters["@value"].Value = value;
            insertCmd.ExecuteNonQuery();
        }
    }

    private void SyncVerifications(int planId, List<PlanVerificationEntry> verifications)
    {
        using var deleteCmd = _connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Verifications WHERE PlanId = @planId";
        deleteCmd.Parameters.AddWithValue("@planId", planId);
        deleteCmd.ExecuteNonQuery();

        if (verifications.Count == 0) return;

        using var insertCmd = _connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Verifications (PlanId, Name, Status) VALUES (@planId, @name, @status)";
        insertCmd.Parameters.AddWithValue("@planId", planId);
        insertCmd.Parameters.AddWithValue("@name", string.Empty);
        insertCmd.Parameters.AddWithValue("@status", string.Empty);

        foreach (var v in verifications)
        {
            insertCmd.Parameters["@planId"].Value = planId;
            insertCmd.Parameters["@name"].Value = v.Name;
            insertCmd.Parameters["@status"].Value = v.Status;
            insertCmd.ExecuteNonQuery();
        }
    }

    public void DeletePlan(int planId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Plans WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", planId);
        cmd.ExecuteNonQuery();
    }

    public void UpsertCosts(int planId, List<CostEntry> costs)
    {
        using var deleteCmd = _connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Costs WHERE PlanId = @planId";
        deleteCmd.Parameters.AddWithValue("@planId", planId);
        deleteCmd.ExecuteNonQuery();

        if (costs.Count == 0) return;

        using var insertCmd = _connection.CreateCommand();
        insertCmd.CommandText = """
            INSERT INTO Costs (PlanId, Promptware, Tokens, Cost, LogTimestamp)
            VALUES (@planId, @promptware, @tokens, @cost, @logTimestamp)
            """;
        insertCmd.Parameters.AddWithValue("@planId", planId);
        insertCmd.Parameters.AddWithValue("@promptware", string.Empty);
        insertCmd.Parameters.AddWithValue("@tokens", 0);
        insertCmd.Parameters.AddWithValue("@cost", 0.0);
        insertCmd.Parameters.AddWithValue("@logTimestamp", DBNull.Value);

        foreach (var cost in costs)
        {
            insertCmd.Parameters["@planId"].Value = planId;
            insertCmd.Parameters["@promptware"].Value = cost.Promptware;
            insertCmd.Parameters["@tokens"].Value = cost.Tokens;
            insertCmd.Parameters["@cost"].Value = (double)cost.Cost;
            insertCmd.Parameters["@logTimestamp"].Value = cost.LogTimestamp.HasValue
                ? cost.LogTimestamp.Value.ToString("O", CultureInfo.InvariantCulture)
                : DBNull.Value;
            insertCmd.ExecuteNonQuery();
        }
    }

    public void UpsertRecommendations(int planId, string folderName, List<RecommendationYaml> recommendations,
        string project, string planTitle, DateTime updated, PlanStatus status)
    {
        using var deleteCmd = _connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Recommendations WHERE PlanId = @planId";
        deleteCmd.Parameters.AddWithValue("@planId", planId);
        deleteCmd.ExecuteNonQuery();

        if (recommendations.Count == 0) return;

        using var insertCmd = _connection.CreateCommand();
        insertCmd.CommandText = """
            INSERT INTO Recommendations (PlanId, Title, Description, State, DeclineReason,
                                         PlanTitle, PlanFolderName, Project, Date, SourcePlanStatus)
            VALUES (@planId, @title, @description, @state, @declineReason,
                    @planTitle, @planFolderName, @project, @date, @sourcePlanStatus)
            """;
        insertCmd.Parameters.AddWithValue("@planId", planId);
        insertCmd.Parameters.AddWithValue("@title", string.Empty);
        insertCmd.Parameters.AddWithValue("@description", string.Empty);
        insertCmd.Parameters.AddWithValue("@state", string.Empty);
        insertCmd.Parameters.AddWithValue("@declineReason", DBNull.Value);
        insertCmd.Parameters.AddWithValue("@planTitle", string.Empty);
        insertCmd.Parameters.AddWithValue("@planFolderName", string.Empty);
        insertCmd.Parameters.AddWithValue("@project", string.Empty);
        insertCmd.Parameters.AddWithValue("@date", string.Empty);
        insertCmd.Parameters.AddWithValue("@sourcePlanStatus", string.Empty);

        foreach (var rec in recommendations)
        {
            insertCmd.Parameters["@planId"].Value = planId;
            insertCmd.Parameters["@title"].Value = rec.Title;
            insertCmd.Parameters["@description"].Value = rec.Description;
            insertCmd.Parameters["@state"].Value = string.IsNullOrWhiteSpace(rec.State) ? "Pending" : rec.State;
            insertCmd.Parameters["@declineReason"].Value = (object?)rec.DeclineReason ?? DBNull.Value;
            insertCmd.Parameters["@planTitle"].Value = planTitle;
            insertCmd.Parameters["@planFolderName"].Value = folderName;
            insertCmd.Parameters["@project"].Value = project;
            insertCmd.Parameters["@date"].Value = updated.ToString("O", CultureInfo.InvariantCulture);
            insertCmd.Parameters["@sourcePlanStatus"].Value = status.ToString();
            insertCmd.ExecuteNonQuery();
        }
    }

    public void BulkUpsertPlans(List<PlanFile> plans)
    {
        using var transaction = _connection.BeginTransaction();
        try
        {
            foreach (var plan in plans)
                UpsertPlanInternal(plan);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public long GetDatabaseSize()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT page_count * page_size FROM pragma_page_count(), pragma_page_size()";
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt64(result, CultureInfo.InvariantCulture) : 0;
    }

    public DateTime GetLastSyncTime()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT Value FROM SyncMetadata WHERE Key = 'LastSyncTime'";
        var result = cmd.ExecuteScalar();
        if (result is string s && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt))
            return dt;
        return DateTime.MinValue;
    }

    public void SetLastSyncTime(DateTime time)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO SyncMetadata (Key, Value) VALUES ('LastSyncTime', @value)
            ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value
            """;
        cmd.Parameters.AddWithValue("@value", time.ToString("O", CultureInfo.InvariantCulture));
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
