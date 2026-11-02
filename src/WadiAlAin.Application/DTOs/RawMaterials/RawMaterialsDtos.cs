namespace WadiAlAin.Application.DTOs.RawMaterials
{
    public class RawMaterialInspectionDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public string ReceiptDate { get; set; } = string.Empty;
        public string? MaterialType { get; set; } // Preforms, Caps, Labels, Cartons, ShrinkFilm
        public string? MaterialName { get; set; }
        public string? SupplierName { get; set; }
        public string? BatchNumber { get; set; }
        public decimal? QuantityReceived { get; set; }
        public string? Unit { get; set; }
        public string? CertificateOfAnalysisNumber { get; set; }
        public bool HasCoaAttached { get; set; }
        public string InspectionStatus { get; set; } = "Accepted"; // Accepted, Rejected, Concession
        public string? RejectionReason { get; set; }
        public string? ActionTaken { get; set; }
        public string ApprovalStatus { get; set; } = "Pending_QM";
        public string? QmApprovedBy { get; set; }
        public string? QmApprovedAt { get; set; }
        public string? GmApprovedBy { get; set; }
        public string? GmApprovedAt { get; set; }
        public string? CreatedAt { get; set; }
        public List<RawMaterialItemInspectionDto> Items { get; set; } = new();
    }

    public class RawMaterialItemInspectionDto
    {
        public int Id { get; set; }
        public int InspectionId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string? StandardSpecification { get; set; }
        public string? ActualReading { get; set; }
        public bool IsConforming { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class SupplierEvaluationDto
    {
        public int Id { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? MaterialSupplied { get; set; }
        public string EvaluationPeriod { get; set; } = string.Empty; // e.g., 2026-Q1
        public decimal? QualityScore { get; set; }
        public decimal? DeliveryScore { get; set; }
        public decimal? DocumentationScore { get; set; }
        public decimal? TotalScore { get; set; }
        public string RatingGrade { get; set; } = "A"; // A, B, C, D
        public string? ApprovedStatus { get; set; } = "Approved";
        public string? Notes { get; set; }
        public string? EvaluatedBy { get; set; }
        public string? CreatedAt { get; set; }
    }
}
