using WadiAlAin.Application.Common.Models;
using WadiAlAin.Application.DTOs.Auth;
using WadiAlAin.Domain.Entities;

namespace WadiAlAin.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<Result<UserDto>> GetCurrentUserAsync(int userId);
        Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<Result<UserSignatureDto?>> GetSignatureAsync(int userId);
        Task<Result<bool>> SaveSignatureAsync(int userId, UserSignatureDto dto);
        Task<Result<bool>> DeleteSignatureAsync(int userId);
        Task<IEnumerable<dynamic>> GetAllUsersAsync(int? branchId);
        Task<Result<int>> SaveUserAsync(System.Text.Json.JsonElement body);
        Task<Result<bool>> ToggleUserActiveAsync(int id, System.Text.Json.JsonElement body);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Result<bool>> SaveRoleAsync(System.Text.Json.JsonElement body);
        Task<Result<bool>> DeleteRoleAsync(int id);
    }
}
