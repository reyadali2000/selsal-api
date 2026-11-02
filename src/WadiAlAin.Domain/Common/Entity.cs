namespace WadiAlAin.Domain.Common
{
    public abstract class Entity
    {
        public int Id { get; set; }
    }

    public abstract class AuditableEntity : Entity
    {
        public string? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}