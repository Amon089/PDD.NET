using Habitto.Application.Bookings.Commands;
using Habitto.Domain.Entities;
using Habitto.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Habitto.Api.Controllers;

public record CreatePropertyRequest(
    Guid OwnerId,
    string Title,
    string City,
    decimal Latitude,
    decimal Longitude,
    decimal NightlyRate);

[ApiController]
[Route("api/properties")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyRepository _properties;
    private readonly IUnitOfWork _unitOfWork;

    public PropertiesController(
        IPropertyRepository properties,
        IUnitOfWork unitOfWork)
    {
        _properties = properties;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string? city,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var results = await _properties.SearchAsync(city, from, to, ct);

        return Ok(results.Select(p => new
        {
            p.Id,
            p.Title,
            p.City,
            p.NightlyRate
        }));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
    {
        var property = await _properties.GetByIdWithBookingsAsync(id, ct);

        if (property is null)
            return NotFound();

        return Ok(new
        {
            property.Id,
            property.OwnerId,
            property.Title,
            property.City,
            property.Latitude,
            property.Longitude,
            property.NightlyRate,
            property.IsActive
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreatePropertyRequest request,
        CancellationToken ct)
    {
        var property = new Property(
            request.OwnerId,
            request.Title,
            request.City,
            request.Latitude,
            request.Longitude,
            request.NightlyRate);

        await _properties.AddAsync(property, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Ok(new
        {
            property.Id,
            property.Title,
            property.City
        });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
    {
        var property = await _properties.GetByIdWithBookingsAsync(id, ct);

        if (property is null)
            return NotFound();

        property.Deactivate();

        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IBookingRepository _bookings;

    public BookingsController(
        IMediator mediator,
        IBookingRepository bookings)
    {
        _mediator = mediator;
        _bookings = bookings;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateBookingCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(
        Guid userId,
        CancellationToken ct)
    {
        var bookings = await _bookings.GetByUserAsync(userId, ct);

        return Ok(bookings.Select(b => new
        {
            b.Id,
            b.PropertyId,
            b.UserId,
            CheckIn = b.Stay.Start,
            CheckOut = b.Stay.End,
            b.TotalPrice,
            Status = b.Status.ToString()
        }));
    }
}