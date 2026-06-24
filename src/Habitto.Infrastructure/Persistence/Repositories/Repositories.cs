using Habitto.Domain.Entities;
using Habitto.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Habitto.Infrastructure.Persistence.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly HabittoDbContext _db;
    public PropertyRepository(HabittoDbContext db) => _db = db;

    public Task<Property?> GetByIdWithBookingsAsync(Guid id, CancellationToken ct = default)
        => _db.Properties.Include(p => p.Bookings).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Property>> SearchAsync(string? city, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var query = _db.Properties.Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => p.City.Contains(city));

        var properties = await query.Include(p => p.Bookings).ToListAsync(ct);

        if (from is null || to is null)
            return properties;

        // Filtrado de disponibilidad en memoria reutilizando la misma regla
        // de solapamiento del dominio (DateRange.Overlaps), para no duplicar
        // la lógica de negocio en la capa de infraestructura.
        var requested = Habitto.Domain.ValueObjects.DateRange.Create(from.Value, to.Value);
        return properties
            .Where(p => !p.Bookings.Any(b =>
                b.Status != Habitto.Domain.ValueObjects.BookingStatus.Cancelled &&
                b.Stay.Overlaps(requested)))
            .ToList();
    }

    public Task AddAsync(Property property, CancellationToken ct = default)
    {
        _db.Properties.Add(property);
        return Task.CompletedTask;
    }
}

public class BookingRepository : IBookingRepository
{
    private readonly HabittoDbContext _db;
    public BookingRepository(HabittoDbContext db) => _db = db;

    public async Task<IReadOnlyList<Booking>> GetByOwnerAsync(Guid ownerId, Guid? propertyId, CancellationToken ct = default)
    {
        var propertyIds = await _db.Properties
            .Where(p => p.OwnerId == ownerId && (propertyId == null || p.Id == propertyId))
            .Select(p => p.Id)
            .ToListAsync(ct);

        return await _db.Bookings
            .Where(b => propertyIds.Contains(b.PropertyId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Bookings
            .Where(b => b.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<bool> HasAnyBookingByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Bookings
            .AnyAsync(b => b.UserId == userId, ct);
    }
}

public class UserRepository : IUserRepository
{
    private readonly HabittoDbContext _db;
    public UserRepository(HabittoDbContext db) => _db = db;

    public Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task AddAsync(AppUser user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }
}

public class WishlistRepository : IWishlistRepository
{
    private readonly HabittoDbContext _db;
    public WishlistRepository(HabittoDbContext db) => _db = db;

    public Task<IReadOnlyList<WishlistItem>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => _db.WishlistItems.Where(w => w.UserId == userId)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<WishlistItem>)t.Result, ct);

    public Task AddAsync(WishlistItem item, CancellationToken ct = default)
    {
        _db.WishlistItems.Add(item);
        return Task.CompletedTask;
    }

    public async Task RemoveAsync(Guid userId, Guid propertyId, CancellationToken ct = default)
    {
        var item = await _db.WishlistItems.FirstOrDefaultAsync(
            w => w.UserId == userId && w.PropertyId == propertyId, ct);
        if (item is not null)
            _db.WishlistItems.Remove(item);
    }
}
