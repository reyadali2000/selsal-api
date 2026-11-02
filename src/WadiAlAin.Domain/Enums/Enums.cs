namespace WadiAlAin.Domain.Enums
{
    public enum ApprovalStatus
    {
        Pending_QM,
        Pending_GM,
        Approved,
        Rejected
    }

    public enum ReportStatus
    {
        Draft,
        Submitted,
        UnderReview,
        Approved,
        Rejected
    }

    public enum FieldType
    {
        Text,
        Number,
        Decimal,
        Dropdown,
        Checkbox,
        Date,
        Time,
        Signature,
        Photo
    }
}