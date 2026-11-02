using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.DTOs.Common;

namespace WadiAlAin.Application.Interfaces
{
    public interface IRawMaterialsService
    {
        Task<IEnumerable<dynamic>> GetReportsAsync(int? branchId);
        Task<Result<dynamic>> GetReportDetailsAsync(int id);
        Task<Result<int>> SaveReportAsync(System.Text.Json.JsonElement reportData);
        Task<Result<bool>> UpdateReportStatusAsync(int id, UpdateStatusRequest dto);
        Task<Result<bool>> DeleteReportAsync(int id);
    }
}
