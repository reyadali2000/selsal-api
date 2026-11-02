namespace WadiAlAin.Application.DTOs.MasterData
{
    public class BranchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateBranchDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ShiftDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string SeasonName { get; set; } = "التوقيت الصيفي";
        public bool IsActive { get; set; } = true;
    }

    public class CreateShiftDto
    {
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string SeasonName { get; set; } = "التوقيت الصيفي";
        public bool IsActive { get; set; } = true;
    }

    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int? BranchId { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class PositionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public int? DepartmentId { get; set; }
        public int Level { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class DailyWorkerDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string NameKey { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedAt { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? VolumeLiters { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateProductDto
    {
        public int? BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? VolumeLiters { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class TestParameterDto
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? InputType { get; set; }
    }

    public class QualityStandardDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string? WaterType { get; set; }
        public int? ParameterId { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? ExpectedText { get; set; }
    }
}
