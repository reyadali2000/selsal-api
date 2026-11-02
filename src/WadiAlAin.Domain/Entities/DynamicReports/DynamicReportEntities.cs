using WadiAlAin.Domain.Common;

namespace WadiAlAin.Domain.Entities
{
    public class ReportTemplate : AuditableEntity
    {
        public int? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? IconName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string ApprovalType { get; set; } = "two_level";
        public string Orientation { get; set; } = "portrait";
        public string? DocNumber { get; set; }
        public string? VersionNumber { get; set; }
        public string? IssueDate { get; set; }
        public int? PrimaryDepartmentId { get; set; }
        public int? WorkflowId { get; set; }
        public string? Code { get; set; }
    }

    public class ReportSection : AuditableEntity
    {
        public int ReportId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsCollapsible { get; set; } = true;
        public string? VisibilityJson { get; set; }
    }

    public class ReportField : AuditableEntity
    {
        public int ReportId { get; set; }
        public int SectionId { get; set; }
        public string FieldKey { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Hint { get; set; }
        public string FieldType { get; set; } = "text";
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public string? DefaultValue { get; set; }
        public string? OptionsJson { get; set; }
        public string? ValidationJson { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? Unit { get; set; }
        public bool IsVisible { get; set; } = true;
        public int? ParameterId { get; set; }
        public int? StandardId { get; set; }
    }

    public class ReportRecord : AuditableEntity
    {
        public int ReportId { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public string? Status { get; set; }
        public string ApprovalStatus { get; set; } = "Pending_QM";
        public string? QmApprovedBy { get; set; }
        public string? QmApprovedAt { get; set; }
        public string? GmApprovedBy { get; set; }
        public string? GmApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? RejectedBy { get; set; }
        public string? RejectedAt { get; set; }
    }

    public class ReportValue : Entity
    {
        public int RecordId { get; set; }
        public int FieldId { get; set; }
        public string? Value { get; set; }
    }

    public class Workflow : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class WorkflowStep : Entity
    {
        public int WorkflowId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string? RequiredRole { get; set; }
        public int? RequiredPositionId { get; set; }
        public string ActionType { get; set; } = "approve";
        public bool IsOptional { get; set; }
        public string? CreatedAt { get; set; }
    }
}
