namespace WadiAlAin.Application.DTOs.Common
{
    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
        public string? RejectionReason { get; set; }
        public string? RejectionCategory { get; set; }
        public string? SeverityLevel { get; set; }
        public string? RejectionNotes { get; set; }
        public string? AffectedProducts { get; set; }
        public string? ImmediateAction { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? CorrectiveOwner { get; set; }
        public string? CorrectiveDueDate { get; set; }
        public string? GmDecision { get; set; }
        public string? GmDecisionBy { get; set; }
        public string? GmDecisionNotes { get; set; }
    }
}
