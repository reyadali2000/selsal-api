namespace WadiAlAin.Application.DTOs.DynamicReports
{
    public class ReportCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? IconName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ReportTemplateDto
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
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
        public List<ReportSectionDto> Sections { get; set; } = new();
    }

    public class ReportSectionDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsCollapsible { get; set; } = true;
        public string? VisibilityJson { get; set; }
        public List<ReportFieldDto> Fields { get; set; } = new();
    }

    public class ReportFieldDto
    {
        public int Id { get; set; }
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

    public class ReportRecordDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string? ReportName { get; set; }
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
        public string? CreatedBy { get; set; }
        public string? CreatedAt { get; set; }
        public Dictionary<string, object?> FieldValues { get; set; } = new();
        public List<ReportApprovalActionDto> Approvals { get; set; } = new();
    }

    public class ReportApprovalActionDto
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public int? StepId { get; set; }
        public string? StepName { get; set; }
        public string Action { get; set; } = string.Empty; // approve, reject
        public string? ActorName { get; set; }
        public int? ActorId { get; set; }
        public string? ActorPosition { get; set; }
        public string? Role { get; set; }
        public string? ActedAt { get; set; }
        public string? Comments { get; set; }
        public string? PreviousStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? SignatureHash { get; set; }
        public string? SignatureType { get; set; }
    }

    public class ApprovalDecisionRequest
    {
        public string Action { get; set; } = "approve"; // approve, reject
        public int? StepId { get; set; }
        public string? StepName { get; set; }
        public string? Comments { get; set; }
        public string? SignatureHash { get; set; }
        public string? SignatureType { get; set; }
    }
}
