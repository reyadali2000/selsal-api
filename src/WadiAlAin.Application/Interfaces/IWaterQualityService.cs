using WadiAlAin.Application.Common.Models;
using WadiAlAin.Domain.Entities;

namespace WadiAlAin.Application.Interfaces
{
    public interface IWaterQualityService
    {
        Task<IEnumerable<WaterQualityReport>> GetLogsAsync(int? branchId, string? category, string? fromDate, string? toDate);
        Task<Result<dynamic>> GetLogByIdAsync(int id);
        Task<Result<int>> SaveLogAsync(System.Text.Json.JsonElement body);
        Task<Result<bool>> UpdateLogStatusAsync(int id, System.Text.Json.JsonElement body);
        Task<Result<bool>> SaveMicroResultsAsync(int id, System.Text.Json.JsonElement body);
        Task<Result<bool>> DeleteLogAsync(int id);
        Task<IEnumerable<dynamic>> GetDailyWaterReportsAsync();
        Task<Result<int>> SaveDailyWaterReportAsync(System.Text.Json.JsonElement reportData);
    }
}
