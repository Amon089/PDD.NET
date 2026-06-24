using Habitto.Application.Identity;
using Habitto.Application.Reports;
using Habitto.Application.Wishlist;
using Habitto.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Habitto.Api.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize] // Guardar de forma permanente requiere login, según el enunciado
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;
    public WishlistController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Add(AddToWishlistCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Remove([FromQuery] RemoveFromWishlistCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWishlistQuery(userId), ct);
        return Ok(result);
    }
}

[ApiController]
[Route("api/identity")]
[Authorize]
public class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;
    public IdentityController(IMediator mediator) => _mediator = mediator;

    public sealed record VerifyIdentityRequest(Guid UserId, byte[] DocumentImage);

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(VerifyIdentityRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyIdentityCommand(request.UserId, request.DocumentImage), ct);
        return Ok(result);
    }
}

[ApiController]
[Route("api/reports")]
[Authorize] // Solo dueños autenticados generan reportes financieros
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IExcelReportExporter _exporter;

    public ReportsController(IMediator mediator, IExcelReportExporter exporter)
    {
        _mediator = mediator;
        _exporter = exporter;
    }

    [HttpGet("owner/{ownerId:guid}/excel")]
    public async Task<IActionResult> ExportExcel(Guid ownerId, [FromQuery] Guid? propertyId, CancellationToken ct)
    {
        var rows = await _mediator.Send(new GetOwnerBookingReportQuery(ownerId, propertyId), ct);
        var fileBytes = _exporter.Export(rows);

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"reporte-reservas-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }
    
    [HttpGet("owner/{ownerId:guid}")]
    public async Task<IActionResult> GetReport(
        Guid ownerId,
        [FromQuery] Guid? propertyId,
        CancellationToken ct)
    {
        var rows = await _mediator.Send(
            new GetOwnerBookingReportQuery(ownerId, propertyId),
            ct);

        return Ok(rows);
    }
}
