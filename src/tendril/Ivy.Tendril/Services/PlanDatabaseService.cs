using System.Globalization;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Database;
using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Services;

public class PlanDatabaseService : IPlanDatabaseService, IDisposable
{
    private static readonly HashSet<string> AllowedTableColumns = new(StringComparer.Ordinal)
    {
        "Repos", "RepoPath", "Commits", "CommitHash", "PullRequests", "PrUrl",
        "RelatedPlans", "RelatedPlanPath", "DependsOn", "DependsOnPlanPath", "Verifications"
    };

    private readonly SqliteConnection _connection;
    private readonly object _lock = new();

    public PlanDatabaseService(string databasePath)
    {
        _connection = new SqliteConnection($"Data Source={databasePath};Mode=ReadWriteCreate");
        _connection.Open();

        // Check database integrity before use
        var isCorrupted = false;
        try
        {
            using var integrityCmd = _connection.CreateCommand();
            integrityCmd.CommandText = "PRAGMA integrity_check";
            var integrityResult = integrityCmd.ExecuteScalar()?.ToString();
            isCorrupted = integrityResult != "ok";
        }
        catch (SqliteException)
        {
            isCorrupted = true;
        }

        if (isCorrupted)
        {
            // Corruption detected - release file handles and delete the database
            SqliteConnection.ClearPool(_connection);
            _connection.Dispose();
            File.Delete(databasePath);
            if (File.Exists(databasePath + "-wal"))
                File.Delete(databasePath + "-wal");
            if (File.Exists(databasePath + "-shm"))
                File.Delete(databasePath + "-shm");

            // Reopen clean database
            _connection = new SqliteConnection($"Data Source={databasePath};Mode=ReadWriteCreate");
            _connection.Open();
        }

        using var pragmaCmd = _connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        pragmaCmd.ExecuteNonQuery();

        var migrator = new DatabaseMigrator(_connection);
        migrator.ApplyMigrations();
    }

    public List<PlanFile> GetPlans(PlanStatus? statusFilter = null)
    {
        lock (_lock)
        {
            var sql = """
                      SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                             YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt
                      FROM Plans
                      """;

            if (statusFilter.HasValue)
                sql += " WHERE State = @state";

            sql += " ORDER BY Id";

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;

            if (statusFilter.HasValue)
                cmd.Parameters.AddWithValue("@state", statusFilter.Value.ToString());

            var planIds = new List<int>();
            var plans = new List<PlanFile>();

            using var reader = cmd.ExecuteReader();
            var rawPlans = new List<(int Id, string Title, string Project, string Level, string State,
                string FolderPath, string FolderName, string YamlRaw, int RevisionCount,
                string LatestContent, string Created, string Updated, string? InitialPrompt)>();

            PlanRowOrdinals? ordinals = null;
            while (reader.Read())
            {
                ordinals ??= GetPlanRowOrdinals(reader);
                var row = ReadPlanRow(reader, ordinals.Value);
                planIds.Add(row.Id);
                rawPlans.Add(row);
            }

            if (rawPlans.Count == 0)
                return plans;

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
                    row.Created, row.Updated, row.InitialPrompt,
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
    }

    public PlanFile? GetPlanByFolder(string folderPath)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                                     YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt
                              FROM Plans WHERE FolderPath = @folderPath
                              """;
            cmd.Parameters.AddWithValue("@folderPath", folderPath);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return BuildPlanFile(reader);

            return null;
        }
    }

    public PlanFile? GetPlanById(int planId)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                                     YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt
                              FROM Plans WHERE Id = @id
                              """;
            cmd.Parameters.AddWithValue("@id", planId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return BuildPlanFile(reader);

            return null;
        }
    }

    public PlanReaderService.PlanCountSnapshot ComputePlanCounts()
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              SELECT
                                  COALESCE(SUM(CASE WHEN State IN ('Draft', 'Blocked') THEN 1 ELSE 0 END), 0) AS DraftCount,
                                  COALESCE(SUM(CASE WHEN State = 'ReadyForReview' THEN 1 ELSE 0 END), 0) AS ReadyForReviewCount,
                                  COALESCE(SUM(CASE WHEN State = 'Failed' THEN 1 ELSE 0 END), 0) AS FailedCount,
                                  COALESCE(SUM(CASE WHEN State = 'Icebox' THEN 1 ELSE 0 END), 0) AS IceboxCount,
                                  (SELECT COUNT(*) FROM Recommendations WHERE State = 'Pending') AS PendingRecommendationsCount
                              FROM Plans
                              """;

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var draftOrdinal = reader.GetOrdinal("DraftCount");
                var readyForReviewOrdinal = reader.GetOrdinal("ReadyForReviewCount");
                var failedOrdinal = reader.GetOrdinal("FailedCount");
                var iceboxOrdinal = reader.GetOrdinal("IceboxCount");
                var pendingRecsOrdinal = reader.GetOrdinal("PendingRecommendationsCount");

                return new PlanReaderService.PlanCountSnapshot(
                    reader.GetInt32(draftOrdinal),
                    reader.GetInt32(readyForReviewOrdinal),
                    reader.GetInt32(failedOrdinal),
                    reader.GetInt32(iceboxOrdinal),
                    reader.GetInt32(pendingRecsOrdinal)
                );
            }

            return new PlanReaderService.PlanCountSnapshot(0, 0, 0, 0, 0);
        }
    }

    public decimal GetPlanTotalCost(int planId)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(SUM(Cost), 0) FROM Costs WHERE PlanId = @planId";
            cmd.Parameters.AddWithValue("@planId", planId);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToDecimal(result, CultureInfo.InvariantCulture) : 0m;
        }
    }

    public int GetPlanTotalTokens(int planId)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(SUM(Tokens), 0) FROM Costs WHERE PlanId = @planId";
            cmd.Parameters.AddWithValue("@planId", planId);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result, CultureInfo.InvariantCulture) : 0;
        }
    }

    public List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7)
    {
        lock (_lock)
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
            int hourOrdinal = -1, projectOrdinal = -1, totalCostOrdinal = -1, totalTokensOrdinal = -1;
            while (reader.Read())
            {
                if (hourOrdinal == -1)
                {
                    hourOrdinal = reader.GetOrdinal("Hour");
                    projectOrdinal = reader.GetOrdinal("Project");
                    totalCostOrdinal = reader.GetOrdinal("TotalCost");
                    totalTokensOrdinal = reader.GetOrdinal("TotalTokens");
                }

                var hourStr = reader.GetString(hourOrdinal);
                if (DateTime.TryParse(hourStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal,
                        out var hour))
                    result.Add(new HourlyTokenBurn
                    {
                        Hour = DateTime.SpecifyKind(hour, DateTimeKind.Utc),
                        Project = reader.GetString(projectOrdinal),
                        Cost = Convert.ToDecimal(reader.GetDouble(totalCostOrdinal), CultureInfo.InvariantCulture),
                        Tokens = reader.GetInt32(totalTokensOrdinal)
                    });
            }

            return result;
        }
    }

    public List<Recommendation> GetRecommendations()
    {
        lock (_lock)
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
            int titleOrdinal = -1,
                descOrdinal = -1,
                stateOrdinal = -1,
                planIdOrdinal = -1,
                planTitleOrdinal = -1,
                planFolderNameOrdinal = -1,
                projectOrdinal = -1,
                dateOrdinal = -1,
                sourcePlanStatusOrdinal = -1,
                declineReasonOrdinal = -1;
            while (reader.Read())
            {
                if (titleOrdinal == -1)
                {
                    titleOrdinal = reader.GetOrdinal("Title");
                    descOrdinal = reader.GetOrdinal("Description");
                    stateOrdinal = reader.GetOrdinal("State");
                    planIdOrdinal = reader.GetOrdinal("PlanId");
                    planTitleOrdinal = reader.GetOrdinal("PlanTitle");
                    planFolderNameOrdinal = reader.GetOrdinal("PlanFolderName");
                    projectOrdinal = reader.GetOrdinal("Project");
                    dateOrdinal = reader.GetOrdinal("Date");
                    sourcePlanStatusOrdinal = reader.GetOrdinal("SourcePlanStatus");
                    declineReasonOrdinal = reader.GetOrdinal("DeclineReason");
                }

                var sourcePlanStatusStr = reader.GetString(sourcePlanStatusOrdinal);
                if (!Enum.TryParse<PlanStatus>(sourcePlanStatusStr, true, out var sourceStatus))
                    sourceStatus = PlanStatus.Draft;

                result.Add(new Recommendation(
                    reader.GetString(titleOrdinal),
                    reader.GetString(descOrdinal),
                    reader.GetString(stateOrdinal),
                    reader.GetInt32(planIdOrdinal).ToString("D5"),
                    reader.GetString(planTitleOrdinal),
                    reader.GetString(planFolderNameOrdinal),
                    reader.GetString(projectOrdinal),
                    DateTime.Parse(reader.GetString(dateOrdinal), CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal),
                    sourceStatus,
                    reader.GetStringOrNull(declineReasonOrdinal)
                ));
            }

            return result;
        }
    }

    public int GetPendingRecommendationsCount()
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Recommendations WHERE State = 'Pending'";
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
        }
    }

    public List<PlanFile> SearchPlans(string query)
    {
        lock (_lock)
        {
            // Try FTS5 first
            using var ftsCmd = _connection.CreateCommand();
            ftsCmd.CommandText = """
                                 SELECT p.Id, p.Title, p.Project, p.Level, p.State, p.FolderPath, p.FolderName,
                                        p.YamlRaw, p.RevisionCount, p.LatestRevisionContent, p.Created, p.Updated, p.InitialPrompt
                                 FROM Plans p
                                 INNER JOIN PlanSearch fts ON fts.rowid = p.Id
                                 WHERE PlanSearch MATCH @query
                                 ORDER BY rank, p.Id
                                 """;
            ftsCmd.Parameters.AddWithValue("@query", query);

            var plans = ExecuteSearchQuery(ftsCmd);

            // If FTS5 returns no results, fall back to LIKE for substring matching
            if (plans.Count == 0)
            {
                var search = $"%{query}%";
                using var likeCmd = _connection.CreateCommand();
                likeCmd.CommandText = """
                                      SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                                             YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt
                                      FROM Plans
                                      WHERE Title LIKE @search OR LatestRevisionContent LIKE @search
                                            OR CAST(Id AS TEXT) LIKE @search OR Project LIKE @search
                                      ORDER BY Id
                                      """;
                likeCmd.Parameters.AddWithValue("@search", search);
                plans = ExecuteSearchQuery(likeCmd);
            }

            return plans;
        }
    }

    public void UpsertPlan(PlanFile plan)
    {
        lock (_lock)
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
    }

    public void DeletePlan(int planId)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Plans WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", planId);
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdatePlanState(int planId, PlanStatus state)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "UPDATE Plans SET State = @state, Updated = @updated WHERE Id = @id";
            cmd.Parameters.AddWithValue("@state", state.ToString());
            cmd.Parameters.AddWithValue("@updated", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("@id", planId);
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdatePlanContent(int planId, string latestRevisionContent, int revisionCount)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText =
                "UPDATE Plans SET LatestRevisionContent = @content, RevisionCount = @count, Updated = @updated WHERE Id = @id";
            cmd.Parameters.AddWithValue("@content", latestRevisionContent);
            cmd.Parameters.AddWithValue("@count", revisionCount);
            cmd.Parameters.AddWithValue("@updated", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("@id", planId);
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateRecommendationState(int planId, string recommendationTitle, string newState,
        string? declineReason)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              UPDATE Recommendations SET State = @state, DeclineReason = @reason
                              WHERE PlanId = @planId AND Title = @title
                              """;
            cmd.Parameters.AddWithValue("@state", newState);
            cmd.Parameters.AddWithValue("@reason", (object?)declineReason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@planId", planId);
            cmd.Parameters.AddWithValue("@title", recommendationTitle);
            cmd.ExecuteNonQuery();
        }
    }

    public void UpsertCosts(int planId, List<CostEntry> costs)
    {
        lock (_lock)
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
    }

    public void UpsertRecommendations(int planId, string folderName, List<RecommendationYaml> recommendations,
        string project, string planTitle, DateTime updated, PlanStatus status)
    {
        lock (_lock)
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
    }

    public void BulkUpsertPlans(List<PlanFile> plans, bool forceOverwrite = false)
    {
        lock (_lock)
        {
            if (plans.Count == 0) return;

            using var transaction = _connection.BeginTransaction();
            try
            {
                foreach (var plan in plans)
                    UpsertPlanInternal(plan, forceOverwrite);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public void UpsertJob(JobItem job)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              INSERT OR REPLACE INTO Jobs (Id, Type, PlanFile, Project, Status, Provider, SessionId, StartedAt, CompletedAt, DurationSeconds, Cost, Tokens, StatusMessage)
                              VALUES (@id, @type, @planFile, @project, @status, @provider, @sessionId, @startedAt, @completedAt, @durationSeconds, @cost, @tokens, @statusMessage)
                              """;
            cmd.Parameters.AddWithValue("@id", job.Id);
            cmd.Parameters.AddWithValue("@type", job.Type);
            cmd.Parameters.AddWithValue("@planFile", job.PlanFile);
            cmd.Parameters.AddWithValue("@project", job.Project);
            cmd.Parameters.AddWithValue("@status", job.Status.ToString());
            cmd.Parameters.AddWithValue("@provider", job.Provider);
            cmd.Parameters.AddWithValue("@sessionId", (object?)job.SessionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@startedAt",
                job.StartedAt?.ToString("O", CultureInfo.InvariantCulture) ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@completedAt",
                job.CompletedAt?.ToString("O", CultureInfo.InvariantCulture) ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@durationSeconds", (object?)job.DurationSeconds ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cost", job.Cost.HasValue ? (double)job.Cost.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@tokens", (object?)job.Tokens ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@statusMessage", (object?)job.StatusMessage ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }

    public List<JobItem> GetRecentJobs(int limit = 100)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Jobs ORDER BY CompletedAt DESC LIMIT @limit";
            cmd.Parameters.AddWithValue("@limit", limit);

            var jobs = new List<JobItem>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                jobs.Add(new JobItem
                {
                    Id = reader.GetString(reader.GetOrdinal("Id")),
                    Type = reader.GetString(reader.GetOrdinal("Type")),
                    PlanFile = reader.GetString(reader.GetOrdinal("PlanFile")),
                    Project = reader.GetString(reader.GetOrdinal("Project")),
                    Status = Enum.Parse<JobStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                    Provider = reader.GetString(reader.GetOrdinal("Provider")),
                    SessionId = reader.IsDBNull(reader.GetOrdinal("SessionId"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("SessionId")),
                    StartedAt = reader.IsDBNull(reader.GetOrdinal("StartedAt"))
                        ? null
                        : DateTime.Parse(reader.GetString(reader.GetOrdinal("StartedAt")), CultureInfo.InvariantCulture,
                            DateTimeStyles.RoundtripKind),
                    CompletedAt = reader.IsDBNull(reader.GetOrdinal("CompletedAt"))
                        ? null
                        : DateTime.Parse(reader.GetString(reader.GetOrdinal("CompletedAt")),
                            CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DurationSeconds = reader.IsDBNull(reader.GetOrdinal("DurationSeconds"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("DurationSeconds")),
                    Cost = reader.IsDBNull(reader.GetOrdinal("Cost"))
                        ? null
                        : (decimal)reader.GetDouble(reader.GetOrdinal("Cost")),
                    Tokens = reader.IsDBNull(reader.GetOrdinal("Tokens"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("Tokens")),
                    StatusMessage = reader.IsDBNull(reader.GetOrdinal("StatusMessage"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("StatusMessage"))
                });
            return jobs;
        }
    }

    public void PurgeOldJobs(int keepCount = 500)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              DELETE FROM Jobs
                              WHERE Id NOT IN (
                                  SELECT Id FROM Jobs
                                  ORDER BY CompletedAt DESC
                                  LIMIT @keepCount
                              )
                              """;
            cmd.Parameters.AddWithValue("@keepCount", keepCount);
            cmd.ExecuteNonQuery();
        }
    }

    public long GetDatabaseSize()
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT page_count * page_size FROM pragma_page_count(), pragma_page_size()";
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt64(result, CultureInfo.InvariantCulture) : 0;
        }
    }

    public DateTime GetLastSyncTime()
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT Value FROM SyncMetadata WHERE Key = 'LastSyncTime'";
            var result = cmd.ExecuteScalar();
            if (result is string s && DateTime.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal, out var dt))
                return dt;
            return DateTime.MinValue;
        }
    }

    public void SetLastSyncTime(DateTime time)
    {
        lock (_lock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              INSERT INTO SyncMetadata (Key, Value) VALUES ('LastSyncTime', @value)
                              ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value
                              """;
            cmd.Parameters.AddWithValue("@value", time.ToString("O", CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
    }

    public void RebuildFtsIndex()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "INSERT INTO PlanSearch(PlanSearch) VALUES('delete-all');";
        cmd.ExecuteNonQuery();

        cmd.CommandText = """
                          INSERT INTO PlanSearch(rowid, Title, LatestRevisionContent, Project, InitialPrompt)
                          SELECT Id, Title, LatestRevisionContent, Project, InitialPrompt
                          FROM Plans;
                          """;
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private static void ValidateIdentifier(string name)
    {
        if (!AllowedTableColumns.Contains(name))
            throw new ArgumentException($"Invalid SQL identifier: {name}");
    }

    /// <summary>
    ///     Builds a PlanFile for a single plan query. Fetches child tables individually.
    ///     For bulk queries, use BuildPlanFileFromRow with pre-fetched child data instead.
    ///     Must be called within _lock.
    /// </summary>
    private PlanFile? BuildPlanFile(SqliteDataReader reader)
    {
        var ordinals = GetPlanRowOrdinals(reader);
        var row = ReadPlanRow(reader, ordinals);
        return BuildPlanFileFromRow(row.Id, row.Title, row.Project, row.Level, row.State,
            row.FolderPath, row.FolderName, row.YamlRaw, row.RevisionCount, row.LatestContent,
            row.Created, row.Updated, row.InitialPrompt,
            GetListForPlan(row.Id, "Repos", "RepoPath"),
            GetListForPlan(row.Id, "Commits", "CommitHash"),
            GetListForPlan(row.Id, "PullRequests", "PrUrl"),
            GetVerificationsForPlan(row.Id),
            GetListForPlan(row.Id, "RelatedPlans", "RelatedPlanPath"),
            GetListForPlan(row.Id, "DependsOn", "DependsOnPlanPath"));
    }

    private static PlanRowOrdinals GetPlanRowOrdinals(SqliteDataReader reader)
    {
        return new PlanRowOrdinals(
            reader.GetOrdinal("Id"),
            reader.GetOrdinal("Title"),
            reader.GetOrdinal("Project"),
            reader.GetOrdinal("Level"),
            reader.GetOrdinal("State"),
            reader.GetOrdinal("FolderPath"),
            reader.GetOrdinal("FolderName"),
            reader.GetOrdinal("YamlRaw"),
            reader.GetOrdinal("RevisionCount"),
            reader.GetOrdinal("LatestRevisionContent"),
            reader.GetOrdinal("Created"),
            reader.GetOrdinal("Updated"),
            reader.GetOrdinal("InitialPrompt")
        );
    }

    private static (int Id, string Title, string Project, string Level, string State,
        string FolderPath, string FolderName, string YamlRaw, int RevisionCount,
        string LatestContent, string Created, string Updated, string? InitialPrompt)
        ReadPlanRow(SqliteDataReader reader, PlanRowOrdinals o)
    {
        return (
            Id: reader.GetInt32(o.Id),
            Title: reader.GetString(o.Title),
            Project: reader.GetString(o.Project),
            Level: reader.GetString(o.Level),
            State: reader.GetString(o.State),
            FolderPath: reader.GetString(o.FolderPath),
            FolderName: reader.GetString(o.FolderName),
            YamlRaw: reader.GetString(o.YamlRaw),
            RevisionCount: reader.GetInt32(o.RevisionCount),
            LatestContent: reader.GetString(o.LatestContent),
            Created: reader.GetString(o.Created),
            Updated: reader.GetString(o.Updated),
            InitialPrompt: reader.GetStringOrNull(o.InitialPrompt)
        );
    }

    private static PlanFile? BuildPlanFileFromRow(int planId, string title, string project, string level,
        string state, string folderPath, string folderName, string yamlRaw, int revisionCount,
        string latestContent, string createdStr, string updatedStr, string? initialPrompt,
        List<string> repos, List<string> commits, List<string> prs,
        List<PlanVerificationEntry> verifications, List<string> relatedPlans, List<string> dependsOn)
    {
        if (!Enum.TryParse<PlanStatus>(state, true, out var status))
            status = PlanStatus.Draft;

        var created = DateTime.Parse(createdStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        var updated = DateTime.Parse(updatedStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

        var metadata = new PlanMetadata(planId, project, level, title, status,
            repos, commits, prs, verifications, relatedPlans, dependsOn, created, updated, initialPrompt);

        return new PlanFile(metadata, latestContent, folderPath, yamlRaw, revisionCount);
    }

    private List<string> GetListForPlan(int planId, string table, string column)
    {
        ValidateIdentifier(table);
        ValidateIdentifier(column);
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT {column} FROM {table} WHERE PlanId = @planId";
        cmd.Parameters.AddWithValue("@planId", planId);

        var list = new List<string>();
        using var reader = cmd.ExecuteReader();
        var columnOrdinal = -1;
        while (reader.Read())
        {
            if (columnOrdinal == -1)
                columnOrdinal = reader.GetOrdinal(column);
            list.Add(reader.GetString(columnOrdinal));
        }

        return list;
    }

    private List<PlanVerificationEntry> GetVerificationsForPlan(int planId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT Name, Status FROM Verifications WHERE PlanId = @planId";
        cmd.Parameters.AddWithValue("@planId", planId);

        var list = new List<PlanVerificationEntry>();
        using var reader = cmd.ExecuteReader();
        int nameOrdinal = -1, statusOrdinal = -1;
        while (reader.Read())
        {
            if (nameOrdinal == -1)
            {
                nameOrdinal = reader.GetOrdinal("Name");
                statusOrdinal = reader.GetOrdinal("Status");
            }

            list.Add(new PlanVerificationEntry
            {
                Name = reader.GetString(nameOrdinal),
                Status = reader.GetString(statusOrdinal)
            });
        }

        return list;
    }

    private Dictionary<int, List<string>> BatchGetList(List<int> planIds, string table, string column)
    {
        ValidateIdentifier(table);
        ValidateIdentifier(column);
        var result = new Dictionary<int, List<string>>();
        if (planIds.Count == 0) return result;

        using var cmd = _connection.CreateCommand();
        var idList = string.Join(",", planIds);
        cmd.CommandText = $"SELECT PlanId, {column} FROM {table} WHERE PlanId IN ({idList})";

        using var reader = cmd.ExecuteReader();
        int planIdOrdinal = -1, columnOrdinal = -1;
        while (reader.Read())
        {
            if (planIdOrdinal == -1)
            {
                planIdOrdinal = reader.GetOrdinal("PlanId");
                columnOrdinal = reader.GetOrdinal(column);
            }

            var planId = reader.GetInt32(planIdOrdinal);
            if (!result.TryGetValue(planId, out var list))
            {
                list = new List<string>();
                result[planId] = list;
            }

            list.Add(reader.GetString(columnOrdinal));
        }

        return result;
    }

    private Dictionary<int, List<PlanVerificationEntry>> BatchGetVerifications(List<int> planIds)
    {
        var result = new Dictionary<int, List<PlanVerificationEntry>>();
        if (planIds.Count == 0) return result;

        using var cmd = _connection.CreateCommand();
        var idList = string.Join(",", planIds);
        cmd.CommandText = $"SELECT PlanId, Name, Status FROM Verifications WHERE PlanId IN ({idList})";

        using var reader = cmd.ExecuteReader();
        int planIdOrdinal = -1, nameOrdinal = -1, statusOrdinal = -1;
        while (reader.Read())
        {
            if (planIdOrdinal == -1)
            {
                planIdOrdinal = reader.GetOrdinal("PlanId");
                nameOrdinal = reader.GetOrdinal("Name");
                statusOrdinal = reader.GetOrdinal("Status");
            }

            var planId = reader.GetInt32(planIdOrdinal);
            if (!result.TryGetValue(planId, out var list))
            {
                list = new List<PlanVerificationEntry>();
                result[planId] = list;
            }

            list.Add(new PlanVerificationEntry
            {
                Name = reader.GetString(nameOrdinal),
                Status = reader.GetString(statusOrdinal)
            });
        }

        return result;
    }

    private List<PlanFile> ExecuteSearchQuery(SqliteCommand cmd)
    {
        var planIds = new List<int>();
        var rawPlans = new List<(int Id, string Title, string Project, string Level, string State,
            string FolderPath, string FolderName, string YamlRaw, int RevisionCount,
            string LatestContent, string Created, string Updated, string? InitialPrompt)>();

        using var reader = cmd.ExecuteReader();
        PlanRowOrdinals? ordinals = null;
        while (reader.Read())
        {
            ordinals ??= GetPlanRowOrdinals(reader);
            var row = ReadPlanRow(reader, ordinals.Value);
            planIds.Add(row.Id);
            rawPlans.Add(row);
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
                row.Created, row.Updated, row.InitialPrompt,
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

    private void UpsertPlanInternal(PlanFile plan, bool forceOverwrite = false)
    {
        var updateGuard = forceOverwrite ? "" : "WHERE excluded.Updated >= Plans.Updated";
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"""
                           INSERT INTO Plans (Id, Title, Project, Level, State, FolderPath, FolderName,
                                              YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt)
                           VALUES (@id, @title, @project, @level, @state, @folderPath, @folderName,
                                   @yamlRaw, @revisionCount, @latestContent, @created, @updated, @initialPrompt)
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
                               Updated = excluded.Updated,
                               InitialPrompt = excluded.InitialPrompt
                           {updateGuard}
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
        cmd.Parameters.AddWithValue("@initialPrompt", plan.InitialPrompt ?? (object)DBNull.Value);

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
        ValidateIdentifier(table);
        ValidateIdentifier(column);
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

    private readonly record struct PlanRowOrdinals(
        int Id,
        int Title,
        int Project,
        int Level,
        int State,
        int FolderPath,
        int FolderName,
        int YamlRaw,
        int RevisionCount,
        int LatestContent,
        int Created,
        int Updated,
        int InitialPrompt);
}