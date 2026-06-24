namespace Habitto.Domain.Entities;

public class WishlistItem
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PropertyId { get; private set; }
    public DateTime AddedAtUtc { get; private set; }

    private WishlistItem() { } // EF Core

    public WishlistItem(Guid userId, Guid propertyId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PropertyId = propertyId;
        AddedAtUtc = DateTime.UtcNow;
    }
}
