namespace backend.Models.Enums;

public enum CallSessionStatus
{
    Queued,
    Ringing,
    Active,
    Transferred,
    Completed,
    Failed,
    Cancelled
}