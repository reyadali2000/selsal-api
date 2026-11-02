using WadiAlAin.Application.Common.Models;
using WadiAlAin.Domain.Entities;

namespace WadiAlAin.Application.Interfaces
{
    public interface IMasterDataService
    {
        Task<IEnumerable<Branch>> GetBranchesAsync();
        Task<IEnumerable<Shift>> GetShiftsAsync(int? branchId);
        Task<Result<Shift>> SaveShiftAsync(System.Text.Json.JsonElement body);
        Task<Result<bool>> ToggleShiftActiveAsync(int id, System.Text.Json.JsonElement body);
        Task<Result<bool>> DeleteShiftAsync(int id);
        Task<IEnumerable<Department>> GetDepartmentsAsync(int? branchId);
        Task<IEnumerable<Position>> GetPositionsAsync(int? departmentId);
        Task<IEnumerable<Product>> GetProductsAsync(int? branchId);
        Task<Result<Product>> SaveProductAsync(System.Text.Json.JsonElement body);
        Task<Result<bool>> ToggleProductActiveAsync(int id, System.Text.Json.JsonElement body);
        Task<Result<bool>> DeleteProductAsync(int id);
        Task<IEnumerable<TestParameter>> GetTestParametersAsync();
        Task<IEnumerable<QualityStandard>> GetQualityStandardsAsync(int? productId, string? waterType);
        Task<IEnumerable<DailyWorker>> GetDailyWorkersAsync(int? branchId);
        Task<Result<DailyWorker>> SaveDailyWorkerAsync(DailyWorker worker);
        Task<Result<bool>> ToggleDailyWorkerActiveAsync(int id, System.Text.Json.JsonElement body);
        Task<Result<bool>> DeleteDailyWorkerAsync(int id);
        Task<IEnumerable<Employee>> GetEmployeesAsync(int? branchId);
    }
}
