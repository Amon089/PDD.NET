using Habitto.Domain.Exceptions;
using Habitto.Domain.ValueObjects;

namespace Habitto.Domain.Entities;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid UserId { get; private set; }
    public DateRange Stay { get; private set; } = null!;
    public decimal TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }

    private Booking() { } // EF Core

    internal Booking(Guid propertyId, Guid userId, DateRange stay, decimal totalPrice)
    {
        Id = Guid.NewGuid();
        PropertyId = propertyId;
        UserId = userId;
        Stay = stay;
        TotalPrice = totalPrice;
        Status = BookingStatus.Confirmed;
    }

    public DateTime CheckInUtc => StayPolicy.Default.CheckInDateTime(Stay.Start);
    public DateTime CheckOutUtc => StayPolicy.Default.CheckOutDateTime(Stay.End);

    public void Cancel()
    {
        if (Status == BookingStatus.Completed)
            throw new InvalidOperationException("No se puede cancelar una reserva ya completada.");

        Status = BookingStatus.Cancelled;
    }
}

public static class PropertyBookingExtensions
{
    /// <summary>
    /// Punto único de creación de reservas para un inmueble. Aquí vive la
    /// invariante de "no double-booking": se valida contra TODAS las reservas
    /// activas (no canceladas) del inmueble antes de crear la nueva.
    ///
    /// Se modela como extensión sobre el aggregate root (Property) en vez de
    /// un método estático suelto, para dejar explícito que la regla pertenece
    /// al agregado Property+Bookings, no a Booking de forma aislada.
    /// </summary>
    public static Booking CreateBooking(this Property property, Guid userId, DateRange stay, decimal totalPrice)
    {
        var hasOverlap = property.Bookings
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Any(b => b.Stay.Overlaps(stay));

        if (hasOverlap)
            throw new BookingOverlapException(property.Id, stay.Start, stay.End);

        var booking = new Booking(property.Id, userId, stay, totalPrice);
        property.AddBooking(booking);
        return booking;
    }
}
