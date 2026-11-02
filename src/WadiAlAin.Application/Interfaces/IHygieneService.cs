using WadiAlAin.Application.Common.Models;

using WadiAlAin.Application.DTOs.Common;

namespace WadiAlAin.Application.Interfaces
{
    public interface IHygieneService
    {
        Task<IEnumerable<dynamic>> GetReportsAsync(int? branchId);
        Task<Result<dynamic>> GetReportDetailsAsync(int id, int? effectiveBranchId = null);
        Task<Result<int>> SaveReportAsync(System.Text.Json.JsonElement reportData);
        Task<Result<bool>> UpdateReportStatusAsync(int id, UpdateStatusRequest dto);
        Task<Result<bool>> DeleteReportAsync(int id);
        Task<IEnumerable<dynamic>> GetChecklistsAsync();
        Task<IEnumerable<dynamic>> GetHygieneLogsAsync(int? reportId, int? branchId);
    }
}
