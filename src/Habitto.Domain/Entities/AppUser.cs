using Habitto.Domain.ValueObjects;

namespace Habitto.Domain.Entities;

public class AppUser
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public IdentityVerificationStatus IdentityStatus { get; private set; }

    private AppUser() { } // EF Core

    public AppUser(string email, string passwordHash, string fullName)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        IdentityStatus = IdentityVerificationStatus.NotStarted;
    }

    public void ApproveIdentity() => IdentityStatus = IdentityVerificationStatus.Approved;
    public void RejectIdentity() => IdentityStatus = IdentityVerificationStatus.Rejected;
    public void MarkIdentityPending() => IdentityStatus = IdentityVerificationStatus.Pending;

    public bool CanCompleteFirstBooking() => IdentityStatus == IdentityVerificationStatus.Approved;
}
