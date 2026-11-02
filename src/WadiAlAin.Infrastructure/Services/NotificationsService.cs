using System.Text.Json;
using Dapper;
using WadiAlAin.Application.Common.Interfaces;
using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.Interfaces;

namespace WadiAlAin.Infrastructure.Services
{
    public class NotificationsService : INotificationsService
    {
        private readonly IDbConnectionFactory _db;
        private readonly IRealtimeNotificationHub _realtimeHub;

        public NotificationsService(IDbConnectionFactory db, IRealtimeNotificationHub realtimeHub)
        {
            _db = db;
            _realtimeHub = realtimeHub;
        }

        public async Task<IEnumerable<dynamic>> GetNotificationsAsync(string? targetRole, int? branchId, bool? isRead)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT * FROM Notifications WHERE 1=1";
            if (!string.IsNullOrWhiteSpace(targetRole) && !targetRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)) 
            {
                sql += " AND (target_role = @TargetRole OR target_role = 'All')";
            }
            if (isRead.HasValue) sql += " AND is_read = @IsRead";
            if (branchId.HasValue && branchId.Value > 0) sql += " AND (branch_id = @BranchId OR branch_id IS NULL)";
            
            sql += " ORDER BY id DESC";
            return await conn.QueryAsync<dynamic>(sql, new { TargetRole = targetRole, IsRead = isRead, BranchId = branchId });
        }

        public async Task<Result<int>> GetUnreadCountAsync(string? targetRole, int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "SELECT COUNT(*) FROM Notifications WHERE is_read = 0";
            if (!string.IsNullOrWhiteSpace(targetRole) && !targetRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)) 
            {
                sql += " AND (target_role = @TargetRole OR target_role = 'All')";
            }
            if (branchId.HasValue && branchId.Value > 0) sql += " AND (branch_id = @BranchId OR branch_id IS NULL)";

            int count = await conn.ExecuteScalarAsync<int>(sql, new { TargetRole = targetRole, BranchId = branchId });
            return Result<int>.Ok(count);
        }

        public async Task<Result<int>> CreateNotificationAsync(JsonElement notifData)
        {
            using var conn = _db.CreateConnection();
            int? branchId = notifData.TryGetProperty("branchId", out var b) && b.ValueKind == JsonValueKind.Number ? b.GetInt32() : (int?)null;
            string? title = notifData.TryGetProperty("title", out var t) ? t.GetString() : null;
            string? body = notifData.TryGetProperty("body", out var bd) ? bd.GetString() : null;
            int? reportId = notifData.TryGetProperty("reportId", out var ri) && ri.ValueKind == JsonValueKind.Number ? ri.GetInt32() : (int?)null;
            string? reportCategory = notifData.TryGetProperty("reportCategory", out var rc) ? rc.GetString() : null;
            string? targetRole = notifData.TryGetProperty("targetRole", out var tr) ? tr.GetString() : null;
            string createdAt = DateTime.UtcNow.ToString("o");

            var id = await conn.QuerySingleAsync<int>(@"
                INSERT INTO Notifications (branch_id, title, body, report_id, report_category, target_role, is_read, created_at)
                VALUES (@BranchId, @Title, @Body, @ReportId, @ReportCategory, @TargetRole, 0, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);",
                new
                {
                    BranchId = branchId,
                    Title = title,
                    Body = body,
                    ReportId = reportId,
                    ReportCategory = reportCategory,
                    TargetRole = targetRole,
                    CreatedAt = createdAt
                });

            // بث الإشعار لحظياً لجميع الأجهزة المتصلة
            try
            {
                var notifObj = new
                {
                    id = id,
                    branchId = branchId,
                    title = title,
                    body = body,
                    reportId = reportId,
                    reportCategory = reportCategory,
                    targetRole = targetRole,
                    isRead = false,
                    createdAt = createdAt
                };
                await _realtimeHub.BroadcastNotificationAsync(notifObj, targetRole, branchId);
            }
            catch
            {
                // لا نوقف عملية الحفظ عند حدوث خطأ في البث اللحظي
            }

            return Result<int>.Ok(id, "تم إنشاء الإشعار بنجاح.");
        }

        public async Task<Result<bool>> MarkAsReadAsync(int id)
        {
            using var conn = _db.CreateConnection();
            int rows = await conn.ExecuteAsync("UPDATE Notifications SET is_read = 1 WHERE id = @Id", new { Id = id });
            if (rows > 0)
                return Result<bool>.Ok(true, "تم تحديث حالة الإشعار.");
            return Result<bool>.Fail("لم يتم العثور على الإشعار.");
        }

        public async Task<Result<bool>> MarkAllAsReadAsync(string? targetRole, int? branchId)
        {
            using var conn = _db.CreateConnection();
            string sql = "UPDATE Notifications SET is_read = 1 WHERE is_read = 0";
            if (!string.IsNullOrWhiteSpace(targetRole)) sql += " AND target_role = @TargetRole";
            if (branchId.HasValue && branchId.Value > 0) sql += " AND (branch_id = @BranchId OR branch_id IS NULL)";

            await conn.ExecuteAsync(sql, new { TargetRole = targetRole, BranchId = branchId });
            return Result<bool>.Ok(true, "تم تحديث جميع الإشعارات.");
        }

        public async Task<Result<bool>> DeleteNotificationAsync(int id)
        {
            using var conn = _db.CreateConnection();
            int rows = await conn.ExecuteAsync("DELETE FROM Notifications WHERE id = @Id", new { Id = id });
            if (rows > 0)
                return Result<bool>.Ok(true, "تم حذف الإشعار.");
            return Result<bool>.Fail("لم يتم العثور على الإشعار.");
        }
    }
}
