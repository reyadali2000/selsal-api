namespace WadiAlAin.Application.DTOs.Hygiene
{
    public class DailyWorkerHealthDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public string LogDate { get; set; } = string.Empty;
        public string? InspectorName { get; set; }
        public string? GeneralNotes { get; set; }
        public string ApprovalStatus { get; set; } = "Pending_QM";
        public string? QmApprovedBy { get; set; }
        public string? QmApprovedAt { get; set; }
        public string? CreatedAt { get; set; }
        public List<DailyWorkerHealthItemDto> Details { get; set; } = new();
    }

    public class DailyWorkerHealthItemDto
    {
        public int Id { get; set; }
        public int LogId { get; set; }
        public int? WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public bool IsCleanUniform { get; set; } = true;
        public bool IsNailsTrimmed { get; set; } = true;
        public bool IsNoWounds { get; set; } = true;
        public bool IsNoJewelry { get; set; } = true;
        public bool IsFitForWork { get; set; } = true;
        public string? ActionTaken { get; set; }
        public string? Notes { get; set; }
    }

    public class HygieneInspectionDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public string InspectionDate { get; set; } = string.Empty;
        public string? InspectionType { get; set; }
        public string? AreaName { get; set; }
        public string? InspectorName { get; set; }
        public decimal? ScorePercentage { get; set; }
        public bool IsCompliant { get; set; } = true;
        public string? CorrectiveActions { get; set; }
        public string ApprovalStatus { get; set; } = "Pending_QM";
        public string? QmApprovedBy { get; set; }
        public string? CreatedAt { get; set; }
    }

    public class PestControlLogDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public string LogDate { get; set; } = string.Empty;
        public string? ContractorName { get; set; }
        public string? TechnicianName { get; set; }
        public string? ChemicalsUsed { get; set; }
        public string? TargetPests { get; set; }
        public string? AreasTreated { get; set; }
        public string? FindingsNotes { get; set; }
        public string? Recommendations { get; set; }
        public string ApprovalStatus { get; set; } = "Pending_QM";
        public string? CreatedAt { get; set; }
    }

    public class CleaningSanitationLogDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? ShiftId { get; set; }
        public string LogDate { get; set; } = string.Empty;
        public string? LineOrArea { get; set; }
        public string? CleaningType { get; set; } // CIP, COP, Manual
        public string? ChemicalsUsed { get; set; }
        public decimal? Concentration { get; set; }
        public decimal? Temperature { get; set; }
        public int? ContactTimeMinutes { get; set; }
        public string? PerformedBy { get; set; }
        public string? VerifiedBy { get; set; }
        public bool IsEffective { get; set; } = true;
        public string ApprovalStatus { get; set; } = "Pending_QM";
        public string? CreatedAt { get; set; }
    }
}
