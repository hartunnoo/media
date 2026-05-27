namespace Media.Domain.Enums;

public enum MediaStatus
{
    Queued = 1,
    Processing = 2,
    Available = 3,
    Quarantined = 4,
    Failed = 5,
    SoftDeleted = 6
}
