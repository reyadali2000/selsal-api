using WadiAlAin.Domain.Common;

namespace WadiAlAin.Domain.Entities
{
    public class WaterQualityReport : AuditableEntity
    {
        public int BranchId { get; set; } = 1;
        public string BranchName { get; set; } = "مصنع أرتل";
        public int? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public string? ReportCategory { get; set; }
        public string? WaterType { get; set; }
        public string? BatchNumber { get; set; }
        public bool? IsCompliant { get; set; }
        public string? GeneralNotes { get; set; }
        public string ApprovalStatus { get; set; } = "Pending_QM";
        public int? ApprovedBy { get; set; }
        public string? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? WaterSource { get; set; }
        public string? WellNumber { get; set; }
        public string? SampleTaker { get; set; }
        public bool? ManagerNotified { get; set; }
        public bool? SourceSuspended { get; set; }
        public string? NcrNumber { get; set; }
        public string? QmApprovedBy { get; set; }
        public string? QmApprovedAt { get; set; }
        public string? GmApprovedBy { get; set; }
        public string? GmApprovedAt { get; set; }
        public string? RejectedBy { get; set; }
        public string? ProductName { get; set; }
        public string? BottleSize { get; set; }
        public string? ProductionDate { get; set; }
        public string? SamplingTime { get; set; }
        public string? SamplingLocation { get; set; }
        public string? ProducedQuantity { get; set; }
        public bool? IsIsolated { get; set; }
        public bool? IsHoldIssued { get; set; }
        public bool? IsRejectIssued { get; set; }
        public bool MicroResultsPending { get; set; }
        public string? MicroEnteredBy { get; set; }
        public string? MicroEnteredAt { get; set; }
        public string? Stage1QmApprovedBy { get; set; }
        public string? Stage1QmApprovedAt { get; set; }
        public string? Stage1GmApprovedBy { get; set; }
        public string? Stage1GmApprovedAt { get; set; }
        public string? MicroQmApprovedBy { get; set; }
        public string? MicroQmApprovedAt { get; set; }
        public string? MicroGmApprovedBy { get; set; }
        public string? MicroGmApprovedAt { get; set; }
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
        public string? GmDecisionAt { get; set; }
        public string? GmDecisionNotes { get; set; }
        public string? MicroRejectionReason { get; set; }
        public string? MicroRejectedBy { get; set; }
        public string? MicroRejectedAt { get; set; }
        public string? MicroRejectionAction { get; set; }
    }

    public class WaterQualityResult : Entity
    {
        public int LogId { get; set; }
        public int? ParameterId { get; set; }
        public decimal? NumericResult { get; set; }
        public string? TextResult { get; set; }
        public bool? IsCompliant { get; set; }
        public string? Notes { get; set; }
    }
}
