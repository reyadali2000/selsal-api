using WadiAlAin.Domain.Common;

namespace WadiAlAin.Domain.Entities
{
    public class User : Entity
    {
        public int? BranchId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Phone { get; set; }
        public string? JobTitle { get; set; }
        public bool IsActive { get; set; } = true;
        public long? AvatarColor { get; set; }
        public string? CreatedAt { get; set; }
        public string? LastLogin { get; set; }
    }

    public class Role : Entity
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long? Color { get; set; }
        public string? IconName { get; set; }
        public string PermissionsJson { get; set; } = "[]";
        public bool IsSystem { get; set; }
        public string? CreatedAt { get; set; }
    }

    public class UserSignature : Entity
    {
        public int UserId { get; set; }
        public string SignatureType { get; set; } = "drawn_png";
        public string SignatureData { get; set; } = string.Empty;
        public string? SignatureHash { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
    }
}
