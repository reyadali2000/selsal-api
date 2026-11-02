using WadiAlAin.Application.Common.Models;

namespace WadiAlAin.Application.Interfaces
{
    public interface INotificationsService
    {
        Task<IEnumerable<dynamic>> GetNotificationsAsync(string? targetRole, int? branchId, bool? isRead);
        Task<Result<int>> GetUnreadCountAsync(string? targetRole, int? branchId);
        Task<Result<int>> CreateNotificationAsync(System.Text.Json.JsonElement notifData);
        Task<Result<bool>> MarkAsReadAsync(int id);
        Task<Result<bool>> MarkAllAsReadAsync(string? targetRole, int? branchId);
        Task<Result<bool>> DeleteNotificationAsync(int id);
    }
}
