namespace WadiAlAin.Application.DTOs.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserDto? User { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    public class UserDto
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? JobTitle { get; set; }
        public bool IsActive { get; set; }
        public long? AvatarColor { get; set; }
        public string? LastLogin { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UserSignatureDto
    {
        public int UserId { get; set; }
        public string SignatureType { get; set; } = "drawn_png";
        public string SignatureData { get; set; } = string.Empty;
        public string? SignatureHash { get; set; }
        public bool IsActive { get; set; } = true;
    }
}