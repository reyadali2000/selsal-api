using Dapper;
using WadiAlAin.Application.Common.Interfaces;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;
using WadiAlAin.Domain.Entities;

namespace WadiAlAin.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly IDbConnectionFactory _db;

        public AuditService(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? entityType, int? entityId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT id, entity_type AS EntityType, entity_id AS EntityId, action, user_id AS UserId, user_name AS UserName, old_value_json AS OldValueJson, new_value_json AS NewValueJson, ip_address AS IpAddress, device_info AS DeviceInfo, created_at AS CreatedAt FROM audit_logs WHERE 1=1";
            if (!string.IsNullOrWhiteSpace(entityType)) sql += " AND entity_type = @EntityType";
            if (entityId.HasValue) sql += " AND entity_id = @EntityId";
            sql += " ORDER BY id DESC";

            return await conn.QueryAsync<AuditLog>(sql, new { EntityType = entityType, EntityId = entityId });
        }

        public async Task LogActionAsync(string entityType, int entityId, string action, int? userId, string? userName, string? oldValue = null, string? newValue = null)
        {
            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(@"
                INSERT INTO audit_logs (entity_type, entity_id, action, user_id, user_name, old_value_json, new_value_json, created_at)
                VALUES (@EntityType, @EntityId, @Action, @UserId, @UserName, @OldValue, @NewValue, @CreatedAt)",
                new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    UserId = userId,
                    UserName = userName,
                    OldValue = oldValue,
                    NewValue = newValue,
                    CreatedAt = DateTime.UtcNow.ToString("o")
                });
        }

        public async Task<IEnumerable<dynamic>> GetArchivedReportsAsync(int? logId, string? logType)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT * FROM ArchivedReports WHERE 1=1";
            if (logId.HasValue) sql += " AND log_id = @LogId";
            if (!string.IsNullOrWhiteSpace(logType)) sql += " AND log_type = @LogType";
            sql += " ORDER BY id DESC";
            return await conn.QueryAsync<dynamic>(sql, new { LogId = logId, LogType = logType });
        }

        public async Task<Result<int>> ArchiveReportAsync(System.Text.Json.JsonElement archiveData)
        {
            using var conn = _db.CreateConnection();
            var id = await conn.QuerySingleAsync<int>(@"
                INSERT INTO ArchivedReports (log_id, log_type, generated_by, approved_by, file_url, file_hash, archived_at)
                VALUES (@LogId, @LogType, @GeneratedBy, @ApprovedBy, @FileUrl, @FileHash, @ArchivedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);",
                new
                {
                    LogId = archiveData.TryGetProperty("logId", out var li) && li.ValueKind == System.Text.Json.JsonValueKind.Number ? li.GetInt32() : 0,
                    LogType = archiveData.TryGetProperty("logType", out var lt) ? lt.GetString() : null,
                    GeneratedBy = archiveData.TryGetProperty("generatedBy", out var gb) ? gb.GetString() : null,
                    ApprovedBy = archiveData.TryGetProperty("approvedBy", out var ab) ? ab.GetString() : null,
                    FileUrl = archiveData.TryGetProperty("fileUrl", out var fu) ? fu.GetString() : null,
                    FileHash = archiveData.TryGetProperty("fileHash", out var fh) ? fh.GetString() : null,
                    ArchivedAt = DateTime.UtcNow.ToString("o")
                });

            return Result<int>.Ok(id, "تم أرشفة التقرير بنجاح.");
        }
    }
}
