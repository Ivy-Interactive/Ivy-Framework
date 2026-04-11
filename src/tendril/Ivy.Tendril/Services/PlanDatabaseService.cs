using System.Globalization;
using System.Text.RegularExpressions;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Database;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Ivy.Tendril.Services;

public class PlanDatabaseService : IPlanDatabaseService
{
    private static readonly HashSet<string> AllowedTableColumns = new(StringComparer.Ordinal)
    {
        "Repos", "RepoPath", "Commits", "CommitHash", "PullRequests", "PrUrl",
        "RelatedPlans", "RelatedPlanPath", "DependsOn", "DependsOnPlanPath", "Verifications"
    };

    private readonly SqliteConnection _connection;
    private readonly ILogger<PlanDatabaseService> _logger;
    private readonly ReaderWriterLockSlim _lock = new();

    public PlanDatabaseService(string databasePath, ILogger<PlanDatabaseService> logger)
    {
        _logger = logger;
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
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
            _logger.LogWarning("Database corruption detected, recreating: {Path}", databasePath);
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
        _logger.LogInformation("Database migrations applied");
    }

    public List<PlanFile> GetPlans(PlanStatus? statusFilter = null)
    {
        _lock.EnterReadLock();
        try
        {
            var sql = """
                      SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                             YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt, SourceUrl
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
                string LatestContent, string Created, string Updated, string? InitialPrompt, string? SourceUrl)>();

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
                    row.Created, row.Updated, row.InitialPrompt, row.SourceUrl,
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
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public PlanFile? GetPlanByFolder(string folderPath)
    {
        _lock.EnterReadLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                                     YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt, SourceUrl
                              FROM Plans WHERE FolderPath = @folderPath
                              """;
            cmd.Parameters.AddWithValue("@folderPath", folderPath);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return BuildPlanFile(reader);

            return null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public PlanFile? GetPlanById(int planId)
    {
        _lock.EnterReadLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                                     YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt, SourceUrl
                              FROM Plans WHERE Id = @id
                              """;
            cmd.Parameters.AddWithValue("@id", planId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return BuildPlanFile(reader);

            return null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public PlanReaderService.PlanCountSnapshot ComputePlanCounts()
    {
        _lock.EnterReadLock();
        try
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
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public DashboardStats GetDashboardData(string? projectFilter)
    {
        _lock.EnterReadLock();
        try
        {
            var cutoff = DateTime.UtcNow.Date.AddDays(-6).ToString("yyyy-MM-dd");
            var pf = projectFilter != null ? " AND (Project = @project OR Project LIKE @projectPattern)" : "";
            var pfAlias = projectFilter != null ? " AND (p.Project = @project OR p.Project LIKE @projectPattern)" : "";
            var pfAlias2 = projectFilter != null ? " AND (p2.Project = @project2 OR p2.Project LIKE @projectPattern2)" : "";

            // Query 1: Status counts + avg cost
            int totalCount, draftCount, inProgressCount, reviewCount, completedCount, failedCount;
            decimal avgCost;
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = $"""
                    SELECT
                        COUNT(*) AS TotalCount,
                        COALESCE(SUM(CASE WHEN State IN ('Draft', 'Blocked') THEN 1 ELSE 0 END), 0),
                        COALESCE(SUM(CASE WHEN State IN ('Building', 'Executing', 'Updating') THEN 1 ELSE 0 END), 0),
                        COALESCE(SUM(CASE WHEN State = 'ReadyForReview' THEN 1 ELSE 0 END), 0),
                        COALESCE(SUM(CASE WHEN State = 'Completed' THEN 1 ELSE 0 END), 0),
                        COALESCE(SUM(CASE WHEN State = 'Failed' THEN 1 ELSE 0 END), 0),
                        (SELECT CASE WHEN COUNT(DISTINCT p2.Id) > 0
                            THEN COALESCE(SUM(c2.Cost), 0) / COUNT(DISTINCT p2.Id) ELSE 0 END
                         FROM Costs c2 JOIN Plans p2 ON p2.Id = c2.PlanId
                         WHERE p2.State IN ('Completed', 'Failed', 'ReadyForReview') {pfAlias2}
                        ) AS AvgCost
                    FROM Plans WHERE 1=1 {pf}
                    """;
                if (projectFilter != null)
                {
                    cmd.Parameters.AddWithValue("@project", projectFilter);
                    cmd.Parameters.AddWithValue("@projectPattern", $"%{projectFilter}%");
                    cmd.Parameters.AddWithValue("@project2", projectFilter);
                    cmd.Parameters.AddWithValue("@projectPattern2", $"%{projectFilter}%");
                }

                using var r = cmd.ExecuteReader();
                r.Read();
                totalCount = r.GetInt32(0);
                draftCount = r.GetInt32(1);
                inProgressCount = r.GetInt32(2);
                reviewCount = r.GetInt32(3);
                completedCount = r.GetInt32(4);
                failedCount = r.GetInt32(5);
                avgCost = Convert.ToDecimal(r.GetValue(6), CultureInfo.InvariantCulture);
            }

            // Query 2: All daily stats in one pass
            var dailyCreated = new Dictionary<string, int>();
            var dailyCompleted = new Dictionary<string, int>();
            var dailyFailed = new Dictionary<string, int>();
            var dailyPrs = new Dictionary<string, int>();
            var dailyCosts = new Dictionary<string, decimal>();
            var dailyTokens = new Dictionary<string, int>();

            using (var cmd = _connection.CreateCommand())
            {
                // Build day list for IN clause
                var days = new List<string>();
                for (var i = 0; i < 7; i++)
                    days.Add(DateTime.UtcNow.Date.AddDays(-i).ToString("yyyy-MM-dd"));
                var dayParams = string.Join(",", days.Select((_, idx) => $"@day{idx}"));

                cmd.CommandText = $"""
                    WITH cte_created AS (
                        SELECT DATE(Created) AS d, COUNT(*) AS cnt FROM Plans
                        WHERE Created >= @cutoff {pf} GROUP BY DATE(Created)
                    ),
                    cte_completed_failed AS (
                        SELECT DATE(Updated) AS d, State, COUNT(*) AS cnt FROM Plans
                        WHERE Updated >= @cutoff AND State IN ('Completed', 'Failed') {pf}
                        GROUP BY DATE(Updated), State
                    ),
                    cte_prs AS (
                        SELECT DATE(p.Updated) AS d, COUNT(*) AS cnt
                        FROM PullRequests pr JOIN Plans p ON p.Id = pr.PlanId
                        WHERE p.Updated >= @cutoff AND p.State = 'Completed' {pfAlias}
                        GROUP BY DATE(p.Updated)
                    ),
                    cte_costs AS (
                        SELECT DATE(p.Updated) AS d, SUM(c.Cost) AS cost, SUM(c.Tokens) AS tokens
                        FROM Costs c JOIN Plans p ON p.Id = c.PlanId
                        WHERE p.Updated >= @cutoff AND p.State IN ('Completed', 'Failed', 'ReadyForReview') {pfAlias}
                        GROUP BY DATE(p.Updated)
                    ),
                    cte_days(day) AS (
                        VALUES {string.Join(",", days.Select((_, idx) => $"(@day{idx})"))}
                    )
                    SELECT
                        cte_days.day,
                        COALESCE(cr.cnt, 0) AS Created,
                        COALESCE(co.cnt, 0) AS Completed,
                        COALESCE(pr.cnt, 0) AS PrsMerged,
                        COALESCE(fa.cnt, 0) AS Failed,
                        COALESCE(cs.cost, 0) AS Cost,
                        COALESCE(cs.tokens, 0) AS Tokens
                    FROM cte_days
                    LEFT JOIN cte_created cr ON cr.d = cte_days.day
                    LEFT JOIN cte_completed_failed co ON co.d = cte_days.day AND co.State = 'Completed'
                    LEFT JOIN cte_completed_failed fa ON fa.d = cte_days.day AND fa.State = 'Failed'
                    LEFT JOIN cte_prs pr ON pr.d = cte_days.day
                    LEFT JOIN cte_costs cs ON cs.d = cte_days.day
                    ORDER BY cte_days.day DESC
                    """;

                cmd.Parameters.AddWithValue("@cutoff", cutoff);
                if (projectFilter != null)
                {
                    cmd.Parameters.AddWithValue("@project", projectFilter);
                    cmd.Parameters.AddWithValue("@projectPattern", $"%{projectFilter}%");
                }
                for (var i = 0; i < days.Count; i++)
                    cmd.Parameters.AddWithValue($"@day{i}", days[i]);

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var day = r.GetString(0);
                    dailyCreated[day] = r.GetInt32(1);
                    dailyCompleted[day] = r.GetInt32(2);
                    dailyPrs[day] = r.GetInt32(3);
                    dailyFailed[day] = r.GetInt32(4);
                    dailyCosts[day] = Convert.ToDecimal(r.GetValue(5), CultureInfo.InvariantCulture);
                    dailyTokens[day] = Convert.ToInt32(r.GetValue(6), CultureInfo.InvariantCulture);
                }
            }

            // Build daily stats for last 7 days
            var dailyStats = new List<DashboardDayStats>();
            for (var i = 0; i < 7; i++)
            {
                var day = DateTime.UtcNow.Date.AddDays(-i);
                var key = day.ToString("yyyy-MM-dd");
                dailyStats.Add(new DashboardDayStats(
                    day,
                    dailyCreated.GetValueOrDefault(key),
                    dailyCompleted.GetValueOrDefault(key),
                    dailyPrs.GetValueOrDefault(key),
                    dailyFailed.GetValueOrDefault(key),
                    dailyCosts.GetValueOrDefault(key),
                    dailyTokens.GetValueOrDefault(key)
                ));
            }

            // Query 3: Project counts (always unfiltered for the stacked progress bar)
            var projectCounts = new List<ProjectCount>();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Project, COUNT(*) FROM Plans GROUP BY Project ORDER BY COUNT(*) DESC";
                using var r = cmd.ExecuteReader();
                while (r.Read()) projectCounts.Add(new ProjectCount(r.GetString(0), r.GetInt32(1)));
            }

            return new DashboardStats(
                totalCount, draftCount, inProgressCount, reviewCount, completedCount, failedCount,
                avgCost, dailyStats, projectCounts);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public decimal GetPlanTotalCost(int planId)
    {
        _lock.EnterReadLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(SUM(Cost), 0) FROM Costs WHERE PlanId = @planId";
            cmd.Parameters.AddWithValue("@planId", planId);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToDecimal(result, CultureInfo.InvariantCulture) : 0m;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int GetPlanTotalTokens(int planId)
    {
        _lock.EnterReadLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(SUM(Tokens), 0) FROM Costs WHERE PlanId = @planId";
            cmd.Parameters.AddWithValue("@planId", planId);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result, CultureInfo.InvariantCulture) : 0;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public List<HourlyTokenBurn> GetHourlyTokenBurn(int days = 7, string? projectFilter = null)
    {
        _lock.EnterReadLock();
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);

            using var cmd = _connection.CreateCommand();
            var projectClause = projectFilter != null ? " AND (p.Project = @project OR p.Project LIKE @projectPattern)" : "";
            cmd.CommandText = $"""
                               SELECT
                                   strftime('%Y-%m-%d %H:00:00', COALESCE(c.LogTimestamp, p.Updated)) as Hour,
                                   p.Project,
                                   SUM(c.Cost) as TotalCost,
                                   SUM(c.Tokens) as TotalTokens
                               FROM Costs c
                               JOIN Plans p ON p.Id = c.PlanId
                               WHERE COALESCE(c.LogTimestamp, p.Updated) >= @cutoff{projectClause}
                               GROUP BY Hour, p.Project
                               ORDER BY Hour, p.Project
                               """;
            cmd.Parameters.AddWithValue("@cutoff", cutoff.ToString("O", CultureInfo.InvariantCulture));
            if (projectFilter != null)
            {
                cmd.Parameters.AddWithValue("@project", projectFilter);
                cmd.Parameters.AddWithValue("@projectPattern", $"%{projectFilter}%");
            }

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
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public List<Recommendation> GetRecommendations()
    {
        _lock.EnterReadLock();
        try
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
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int GetPendingRecommendationsCount()
    {
        _lock.EnterReadLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Recommendations WHERE State = 'Pending'";
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    internal static string SanitizeFts5Query(string query)
    {
        // Replace path separators with spaces
        var sanitized = query.Replace('/', ' ').Replace('\\', ' ');

        // Remove grouping and prefix chars that cause parse errors
        sanitized = sanitized.Replace("(", "").Replace(")", "").Replace("^", "");

        // Remove bare asterisks (keep prefix wildcards like "button*")
        sanitized = Regex.Replace(sanitized, @"(?<!\w)\*", "");

        // Balance double quotes: if odd count, replace with spaces to separate adjacent words
        if (sanitized.Count(c => c == '"') % 2 != 0)
            sanitized = sanitized.Replace("\"", " ");

        // Collapse whitespace and trim
        sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();

        return sanitized;
    }

    public List<PlanFile> SearchPlans(string query)
    {
        _lock.EnterReadLock();
        try
        {
            // Try FTS5 first with sanitized query
            var sanitizedQuery = SanitizeFts5Query(query);

            List<PlanFile> plans = [];

            if (!string.IsNullOrWhiteSpace(sanitizedQuery))
            {
                using var ftsCmd = _connection.CreateCommand();
                ftsCmd.CommandText = """
                                     SELECT p.Id, p.Title, p.Project, p.Level, p.State, p.FolderPath, p.FolderName,
                                            p.YamlRaw, p.RevisionCount, p.LatestRevisionContent, p.Created, p.Updated, p.InitialPrompt, p.SourceUrl
                                     FROM Plans p
                                     INNER JOIN PlanSearch fts ON fts.rowid = p.Id
                                     WHERE PlanSearch MATCH @query
                                     ORDER BY rank, p.Id
                                     """;
                ftsCmd.Parameters.AddWithValue("@query", sanitizedQuery);

                try
                {
                    plans = ExecuteSearchQuery(ftsCmd);
                }
                catch (Microsoft.Data.Sqlite.SqliteException)
                {
                    // FTS5 syntax error — safety net for edge cases the sanitizer doesn't handle
                    plans = [];
                }
            }

            // If FTS5 returns no results, fall back to LIKE for substring matching
            if (plans.Count == 0)
            {
                var search = $"%{query}%";
                using var likeCmd = _connection.CreateCommand();
                likeCmd.CommandText = """
                                      SELECT Id, Title, Project, Level, State, FolderPath, FolderName,
                                             YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt, SourceUrl
                                      FROM Plans
                                      WHERE Title LIKE @search OR LatestRevisionContent LIKE @search
                                            OR CAST(Id AS TEXT) LIKE @search OR Project LIKE @search
                                            OR SourceUrl LIKE @search OR InitialPrompt LIKE @search
                                      ORDER BY Id
                                      """;
                likeCmd.Parameters.AddWithValue("@search", search);
                plans = ExecuteSearchQuery(likeCmd);
            }

            return plans;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void UpsertPlan(PlanFile plan)
    {
        _lock.EnterWriteLock();
        try
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
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void DeletePlan(int planId)
    {
        _lock.EnterWriteLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Plans WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", planId);
            cmd.ExecuteNonQuery();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UpdatePlanState(int planId, PlanStatus state)
    {
        _lock.EnterWriteLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "UPDATE Plans SET State = @state, Updated = @updated WHERE Id = @id";
            cmd.Parameters.AddWithValue("@state", state.ToString());
            cmd.Parameters.AddWithValue("@updated", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("@id", planId);
            cmd.ExecuteNonQuery();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UpdatePlanContent(int planId, string latestRevisionContent, int revisionCount)
    {
        _lock.EnterWriteLock();
        try
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
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UpdateRecommendationState(int planId, string recommendationTitle, string newState,
        string? declineReason)
    {
        _lock.EnterWriteLock();
        try
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
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UpsertCosts(int planId, List<CostEntry> costs)
    {
        _lock.EnterWriteLock();
        try
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
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UpsertRecommendations(int planId, string folderName, List<RecommendationYaml> recommendations,
        string project, string planTitle, DateTime updated, PlanStatus status)
    {
        _lock.EnterWriteLock();
        try
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
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void BulkUpsertPlans(List<PlanFile> plans, bool forceOverwrite = false)
    {
        _lock.EnterWriteLock();
        try
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
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UpsertJob(JobItem job)
    {
        _lock.EnterWriteLock();
        try
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
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public List<JobItem> GetRecentJobs(int limit = 100)
    {
        _lock.EnterReadLock();
        try
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
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void PurgeOldJobs(int keepCount = 500)
    {
        _lock.EnterWriteLock();
        try
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
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void DeleteJob(string id)
    {
        _lock.EnterWriteLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Jobs WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public long GetDatabaseSize()
    {
        _lock.EnterReadLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT page_count * page_size FROM pragma_page_count(), pragma_page_size()";
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt64(result, CultureInfo.InvariantCulture) : 0;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public DateTime GetLastSyncTime()
    {
        _lock.EnterReadLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT Value FROM SyncMetadata WHERE Key = 'LastSyncTime'";
            var result = cmd.ExecuteScalar();
            if (result is string s && DateTime.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal, out var dt))
                return dt;
            return DateTime.MinValue;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void SetLastSyncTime(DateTime time)
    {
        _lock.EnterWriteLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              INSERT INTO SyncMetadata (Key, Value) VALUES ('LastSyncTime', @value)
                              ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value
                              """;
            cmd.Parameters.AddWithValue("@value", time.ToString("O", CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Dictionary<string, string> GetAllPrStatuses()
    {
        _lock.EnterReadLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT PrUrl, Status FROM PrStatuses";
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result[reader.GetString(0)] = reader.GetString(1);
            return result;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void UpsertPrStatus(string prUrl, string owner, string repo, string status, DateTime lastChecked)
    {
        _lock.EnterWriteLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                              INSERT INTO PrStatuses (PrUrl, Owner, Repo, Status, LastChecked)
                              VALUES (@url, @owner, @repo, @status, @checked)
                              ON CONFLICT(PrUrl) DO UPDATE SET
                                  Status = excluded.Status,
                                  LastChecked = excluded.LastChecked
                              """;
            cmd.Parameters.AddWithValue("@url", prUrl);
            cmd.Parameters.AddWithValue("@owner", owner);
            cmd.Parameters.AddWithValue("@repo", repo);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@checked", lastChecked.ToString("O", CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public List<string> GetNonMergedPrUrls()
    {
        _lock.EnterReadLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT PrUrl FROM PrStatuses WHERE Status != 'Merged'";
            var result = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(reader.GetString(0));
            return result;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void RebuildFtsIndex()
    {
        _lock.EnterWriteLock();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO PlanSearch(PlanSearch) VALUES('delete-all');";
            cmd.ExecuteNonQuery();

            cmd.CommandText = """
                              INSERT INTO PlanSearch(rowid, Title, LatestRevisionContent, Project, InitialPrompt, SourceUrl)
                              SELECT Id, Title, LatestRevisionContent, Project, InitialPrompt, SourceUrl
                              FROM Plans;
                              """;
            cmd.ExecuteNonQuery();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
        _lock.Dispose();
    }

    private static void ValidateIdentifier(string name)
    {
        if (!AllowedTableColumns.Contains(name))
            throw new ArgumentException($"Invalid SQL identifier: {name}");
    }

    /// <summary>
    ///     Builds a PlanFile for a single plan query. Fetches child tables individually.
    ///     For bulk queries, use BuildPlanFileFromRow with pre-fetched child data instead.
    ///     Must be called within a read or write lock.
    /// </summary>
    private PlanFile? BuildPlanFile(SqliteDataReader reader)
    {
        var ordinals = GetPlanRowOrdinals(reader);
        var row = ReadPlanRow(reader, ordinals);
        return BuildPlanFileFromRow(row.Id, row.Title, row.Project, row.Level, row.State,
            row.FolderPath, row.FolderName, row.YamlRaw, row.RevisionCount, row.LatestContent,
            row.Created, row.Updated, row.InitialPrompt, row.SourceUrl,
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
            reader.GetOrdinal("InitialPrompt"),
            reader.GetOrdinal("SourceUrl")
        );
    }

    private static (int Id, string Title, string Project, string Level, string State,
        string FolderPath, string FolderName, string YamlRaw, int RevisionCount,
        string LatestContent, string Created, string Updated, string? InitialPrompt, string? SourceUrl)
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
            InitialPrompt: reader.GetStringOrNull(o.InitialPrompt),
            SourceUrl: reader.GetStringOrNull(o.SourceUrl)
        );
    }

    private static PlanFile? BuildPlanFileFromRow(int planId, string title, string project, string level,
        string state, string folderPath, string folderName, string yamlRaw, int revisionCount,
        string latestContent, string createdStr, string updatedStr, string? initialPrompt, string? sourceUrl,
        List<string> repos, List<string> commits, List<string> prs,
        List<PlanVerificationEntry> verifications, List<string> relatedPlans, List<string> dependsOn)
    {
        if (!Enum.TryParse<PlanStatus>(state, true, out var status))
            status = PlanStatus.Draft;

        var created = DateTime.Parse(createdStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        var updated = DateTime.Parse(updatedStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

        var metadata = new PlanMetadata(planId, project, level, title, status,
            repos, commits, prs, verifications, relatedPlans, dependsOn, created, updated, initialPrompt, sourceUrl);

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
            string LatestContent, string Created, string Updated, string? InitialPrompt, string? SourceUrl)>();

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
                row.Created, row.Updated, row.InitialPrompt, row.SourceUrl,
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
                                              YamlRaw, RevisionCount, LatestRevisionContent, Created, Updated, InitialPrompt, SourceUrl)
                           VALUES (@id, @title, @project, @level, @state, @folderPath, @folderName,
                                   @yamlRaw, @revisionCount, @latestContent, @created, @updated, @initialPrompt, @sourceUrl)
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
                               InitialPrompt = excluded.InitialPrompt,
                               SourceUrl = excluded.SourceUrl
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
        cmd.Parameters.AddWithValue("@sourceUrl", plan.SourceUrl ?? (object)DBNull.Value);

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
        int InitialPrompt,
        int SourceUrl);
}
