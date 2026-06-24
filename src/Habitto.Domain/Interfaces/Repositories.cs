using Habitto.Domain.Entities;

namespace Habitto.Domain.Interfaces;

public interface IPropertyRepository
{
    Task<Property?> GetByIdWithBookingsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Property>> SearchAsync(string? city, DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task AddAsync(Property property, CancellationToken ct = default);
}

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetByOwnerAsync(
        Guid ownerId,
        Guid? propertyId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Booking>> GetByUserAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<bool> HasAnyBookingByUserAsync(
        Guid userId,
        CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(AppUser user, CancellationToken ct = default);
}

public interface IWishlistRepository
{
    Task<IReadOnlyList<WishlistItem>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(WishlistItem item, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, Guid propertyId, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}