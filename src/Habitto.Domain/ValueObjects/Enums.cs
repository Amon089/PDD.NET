namespace Habitto.Domain.ValueObjects;

public enum IdentityVerificationStatus
{
    NotStarted = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum BookingStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}
