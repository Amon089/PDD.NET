namespace Habitto.Domain.Entities;

public class Property
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public decimal NightlyRate { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Booking> _bookings = new();
    public IReadOnlyCollection<Booking> Bookings => _bookings.AsReadOnly();

    private Property() { } // EF Core

    public Property(Guid ownerId, string title, string city, decimal latitude, decimal longitude, decimal nightlyRate)
    {
        if (nightlyRate <= 0)
            throw new ArgumentException("La tarifa nocturna debe ser mayor a cero.", nameof(nightlyRate));

        Id = Guid.NewGuid();
        OwnerId = ownerId;
        Title = title;
        City = city;
        Latitude = latitude;
        Longitude = longitude;
        NightlyRate = nightlyRate;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;

    internal void AddBooking(Booking booking) => _bookings.Add(booking);
}
