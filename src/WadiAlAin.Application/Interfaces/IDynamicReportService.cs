using WadiAlAin.Application.Common.Models;

namespace WadiAlAin.Application.Interfaces
{
    public interface IDynamicReportService
    {
        Task<IEnumerable<dynamic>> GetCategoriesAsync();
        Task<IEnumerable<dynamic>> GetTemplatesAsync(int? categoryId);
        Task<Result<dynamic>> GetTemplateDetailsAsync(int id);
        Task<IEnumerable<dynamic>> GetRecordsAsync(int? reportId, int? branchId);
        Task<object> GetDynamicReportStatsAsync(string? timeframe, int? branchId);
        Task<Result<dynamic>> GetRecordDetailsAsync(int id, int? effectiveBranchId = null);
        Task<Result<int>> SaveRecordAsync(int userId, string username, string role, System.Text.Json.JsonElement recordData);
        Task<IEnumerable<dynamic>> GetWorkflowsAsync();
        Task<IEnumerable<dynamic>> GetRecordApprovalsAsync(int recordId);
        Task<IEnumerable<dynamic>> GetPendingRecordsForRoleAsync(string roleStatus, int? branchId);
        Task<IEnumerable<dynamic>> GetCorrectiveActionRecordsAsync(int? branchId);
        Task<Result<bool>> AddApprovalDecisionAsync(int id, int userId, string username, string role, System.Text.Json.JsonElement body);
        Task<Result<bool>> DeleteTemplateAsync(int id);
        Task<Result<bool>> DeleteRecordAsync(int id);
        Task<Result<bool>> UpdateTemplateAsync(int id, System.Text.Json.JsonElement templateData);
        Task<Result<int>> SaveTemplateAsync(System.Text.Json.JsonElement templateData);
    }
}
