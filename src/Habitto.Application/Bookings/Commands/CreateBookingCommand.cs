using Habitto.Application.Common;
using Habitto.Domain.Entities;
using Habitto.Domain.Exceptions;
using Habitto.Domain.Interfaces;
using Habitto.Domain.ValueObjects;
using MediatR;

namespace Habitto.Application.Bookings.Commands;

public sealed record CreateBookingCommand(
    Guid UserId,
    Guid PropertyId,
    DateOnly CheckIn,
    DateOnly CheckOut) : IRequest<CreateBookingResult>;

public sealed record CreateBookingResult(
    Guid BookingId,
    decimal TotalPrice,
    DateTime CheckInUtc,
    DateTime CheckOutUtc);

public sealed class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, CreateBookingResult>
{
    private readonly IPropertyRepository _properties;
    private readonly IUserRepository _users;
    private readonly IBookingRepository _bookings;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(
        IPropertyRepository properties,
        IUserRepository users,
        IBookingRepository bookings,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _properties = properties;
        _users = users;
        _bookings = bookings;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateBookingResult> Handle(
        CreateBookingCommand request,
        CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        // Primera reserva requiere identidad aprobada.
        var isFirstBooking =
            !await _bookings.HasAnyBookingByUserAsync(
                request.UserId,
                ct);

        if (isFirstBooking && !user.CanCompleteFirstBooking())
            throw new IdentityNotVerifiedException(request.UserId);

        var property = await _properties.GetByIdWithBookingsAsync(
            request.PropertyId,
            ct)
            ?? throw new InvalidOperationException("Inmueble no encontrado.");

        var stay = DateRange.Create(
            request.CheckIn,
            request.CheckOut);

        var totalPrice =
            property.NightlyRate * stay.Nights;

        // Valida no double-booking dentro del agregado.
        var booking =
            property.CreateBooking(
                request.UserId,
                stay,
                totalPrice);

        await _unitOfWork.SaveChangesAsync(ct);

        await _notifications.SendAsync(
            new NotificationMessage(
                request.UserId,
                NotificationChannel.Email,
                "Reserva confirmada",
                $"Tu reserva del {stay.Start:dd/MM/yyyy} al {stay.End:dd/MM/yyyy} ha sido confirmada."),
            ct);

        return new CreateBookingResult(
            booking.Id,
            booking.TotalPrice,
            booking.CheckInUtc,
            booking.CheckOutUtc);
    }
}