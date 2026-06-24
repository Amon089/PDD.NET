using Habitto.Application.Common;
using Habitto.Domain.Interfaces;
using MediatR;

namespace Habitto.Application.Identity;

public sealed record VerifyIdentityCommand(Guid UserId, byte[] DocumentImage) : IRequest<IdentityExtractionResult>;

public sealed class VerifyIdentityCommandHandler : IRequestHandler<VerifyIdentityCommand, IdentityExtractionResult>
{
    private readonly IUserRepository _users;
    private readonly IIdentityVerificationService _verification;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyIdentityCommandHandler(
        IUserRepository users,
        IIdentityVerificationService verification,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _verification = verification;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<IdentityExtractionResult> Handle(VerifyIdentityCommand request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        user.MarkIdentityPending();

        var result = await _verification.VerifyAsync(request.DocumentImage, ct);

        if (result.Approved)
            user.ApproveIdentity();
        else
            user.RejectIdentity();

        await _unitOfWork.SaveChangesAsync(ct);

        await _notifications.SendAsync(new NotificationMessage(
            request.UserId,
            NotificationChannel.InApp,
            "Resultado de validación de identidad",
            result.Approved ? "Tu identidad fue aprobada." : "Tu identidad fue rechazada."), ct);

        // NOTA DE SEGURIDAD (requerimiento explícito del cliente): la imagen
        // del documento NUNCA se persiste en este flujo. Solo se procesa en
        // memoria y se descarta tras la extracción. Ver README, sección
        // "Privacidad y seguridad de datos".
        return result;
    }
}
