namespace Habitto.Application.Common;

public sealed record IdentityExtractionResult(
    string FirstName,
    string LastName,
    string DocumentNumber,
    DateOnly DateOfBirth,
    bool Approved);

/// <summary>
/// Puerto de aplicación para la validación de identidad (KYC). La implementación
/// real (Infrastructure.Services.MockIdentityVerificationService) hoy es un mock
/// determinístico/aleatorio. El diseño permite sustituirla por un proveedor real
/// (Azure Face API, AWS Textract, etc.) sin tocar Application ni Domain: solo se
/// registra una nueva implementación en el contenedor de DI.
/// </summary>
public interface IIdentityVerificationService
{
    Task<IdentityExtractionResult> VerifyAsync(byte[] documentImage, CancellationToken ct = default);
}

public enum NotificationChannel { Email, InApp }

public sealed record NotificationMessage(Guid UserId, NotificationChannel Channel, string Subject, string Body);

/// <summary>
/// Puerto de aplicación para notificaciones omnicanal. Hoy mockeado: Email se
/// loguea en consola/archivo, InApp se persiste en tabla. El contrato es el
/// mismo si en el futuro se conecta SendGrid/Twilio/un microservicio en Node.
/// </summary>
public interface INotificationService
{
    Task SendAsync(NotificationMessage message, CancellationToken ct = default);
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
