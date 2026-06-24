using Habitto.Domain.Interfaces;
using MediatR;

namespace Habitto.Application.Reports;

public sealed class GetOwnerBookingReportQueryHandler
    : IRequestHandler<GetOwnerBookingReportQuery, IReadOnlyList<BookingReportRow>>
{
    private readonly IBookingRepository _bookings;
    private readonly IUserRepository _users;
    private readonly IPropertyRepository _properties;

    public GetOwnerBookingReportQueryHandler(
        IBookingRepository bookings, IUserRepository users, IPropertyRepository properties)
    {
        _bookings = bookings;
        _users = users;
        _properties = properties;
    }

    public async Task<IReadOnlyList<BookingReportRow>> Handle(GetOwnerBookingReportQuery request, CancellationToken ct)
    {
        var bookings = await _bookings.GetByOwnerAsync(request.OwnerId, request.PropertyId, ct);
        var rows = new List<BookingReportRow>(bookings.Count);

        foreach (var booking in bookings)
        {
            var guest = await _users.GetByIdAsync(booking.UserId, ct);
            var property = await _properties.GetByIdWithBookingsAsync(booking.PropertyId, ct);

            rows.Add(new BookingReportRow(
                booking.Stay.Start,
                booking.Stay.End,
                booking.TotalPrice,
                guest?.FullName ?? "N/A",
                guest?.Email ?? "N/A",
                property?.Title ?? "N/A"));
        }

        return rows;
    }
}
