using WadiAlAin.Application.Common.Models;
using WadiAlAin.Domain.Entities;

namespace WadiAlAin.Application.Interfaces
{
    public interface IAuditService
    {
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? entityType, int? entityId);
        Task LogActionAsync(string entityType, int entityId, string action, int? userId, string? userName, string? oldValue = null, string? newValue = null);
        Task<IEnumerable<dynamic>> GetArchivedReportsAsync(int? logId, string? logType);
        Task<Result<int>> ArchiveReportAsync(System.Text.Json.JsonElement archiveData);
    }
}
