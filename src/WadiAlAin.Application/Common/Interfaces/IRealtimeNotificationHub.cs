namespace WadiAlAin.Application.Common.Interfaces
{
    public interface IRealtimeNotificationHub
    {
        Task BroadcastNotificationAsync(object notification, string? targetRole = null, int? branchId = null);
    }
}
