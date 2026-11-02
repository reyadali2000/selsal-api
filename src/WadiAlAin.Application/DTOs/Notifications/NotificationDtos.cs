namespace WadiAlAin.Application.DTOs.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int? ReportId { get; set; }
        public string? ReportCategory { get; set; }
        public string? TargetRole { get; set; }
        public bool IsRead { get; set; }
        public string? CreatedAt { get; set; }
    }

    public class CreateNotificationRequest
    {
        public int? BranchId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int? ReportId { get; set; }
        public string? ReportCategory { get; set; }
        public string? TargetRole { get; set; }
    }
}
