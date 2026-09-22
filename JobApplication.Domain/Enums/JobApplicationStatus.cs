namespace JobApplication.Domain.Enums
{
    public enum JobApplicationStatus
    {
        Applied = 0,
        UnderReview = 1,
        InterView = 2,
        Accepted = 3,
        Rejected = 4,

        // WARNING: must be appended at the end — the enum is persisted as an int.
        // Inserting it in the middle would shift every following value and
        // corrupt the existing data.
        Cancelled = 5
    }
}
