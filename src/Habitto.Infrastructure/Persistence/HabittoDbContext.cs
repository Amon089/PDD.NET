using Habitto.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Habitto.Infrastructure.Persistence;

public class HabittoDbContext : DbContext, Habitto.Domain.Interfaces.IUnitOfWork
{
    public HabittoDbContext(DbContextOptions<HabittoDbContext> options) : base(options) { }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Habitto.Infrastructure.Services.InAppNotification> InAppNotifications => Set<Habitto.Infrastructure.Services.InAppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HabittoDbContext).Assembly);
    }
}
