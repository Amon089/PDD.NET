using Habitto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Habitto.Infrastructure.Persistence;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Latitude)
            .HasPrecision(18, 6);

        builder.Property(p => p.Longitude)
            .HasPrecision(18, 6);

        builder.Property(p => p.NightlyRate)
            .HasPrecision(18, 2);

        builder.HasMany(p => p.Bookings)
            .WithOne()
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata.FindNavigation(nameof(Property.Bookings))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(b => b.Stay, stay =>
        {
            stay.Property(s => s.Start)
                .HasColumnName("StayStart")
                .HasColumnType("date");

            stay.Property(s => s.End)
                .HasColumnName("StayEnd")
                .HasColumnType("date");
        });

        builder.Property(b => b.TotalPrice)
            .HasPrecision(18, 2);
    }
}

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.FullName)
            .HasMaxLength(200)
            .IsRequired();
    }
}

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.HasIndex(w => new { w.UserId, w.PropertyId })
            .IsUnique();
    }
}
