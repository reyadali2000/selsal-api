using WadiAlAin.Domain.Common;

namespace WadiAlAin.Domain.Entities
{
    public class AuditLog : Entity
    {
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? OldValueJson { get; set; }
        public string? NewValueJson { get; set; }
        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }
        public string? CreatedAt { get; set; }
    }
}
