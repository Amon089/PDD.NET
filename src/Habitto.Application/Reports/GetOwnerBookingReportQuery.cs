using MediatR;

namespace Habitto.Application.Reports;

public sealed record BookingReportRow(
    DateOnly CheckIn,
    DateOnly CheckOut,
    decimal PricePaid,
    string GuestFullName,
    string GuestEmail,
    string PropertyTitle);

public sealed record GetOwnerBookingReportQuery(Guid OwnerId, Guid? PropertyId)
    : IRequest<IReadOnlyList<BookingReportRow>>;
