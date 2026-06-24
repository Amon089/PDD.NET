using Habitto.Application.Common;
using Microsoft.Extensions.Logging;

namespace Habitto.Infrastructure.Services;

/// <summary>
/// Despachador omnicanal mockeado. En vez de integrar Laravel/Node como pide
/// el enunciado de forma opcional, se modela el contrato con
/// INotificationService y aquí se resuelve con dos canales simulados:
/// Email -> log estructurado (simula el envío real).
/// InApp -> se persiste como notificación en BD (visible para el usuario).
///
/// Sustituir esto por SendGrid/Twilio/un microservicio real implica SOLO
/// crear una nueva clase que implemente INotificationService y registrarla
/// en Program.cs. Cero cambios en Application o Domain.
/// </summary>
public class MockNotificationService : INotificationService
{
    private readonly ILogger<MockNotificationService> _logger;
    private readonly Persistence.HabittoDbContext _db;

    public MockNotificationService(ILogger<MockNotificationService> logger, Persistence.HabittoDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task SendAsync(NotificationMessage message, CancellationToken ct = default)
    {
        switch (message.Channel)
        {
            case NotificationChannel.Email:
                _logger.LogInformation(
                    "[EMAIL MOCK] Para usuario {UserId} | Asunto: {Subject} | {Body}",
                    message.UserId, message.Subject, message.Body);
                break;

            case NotificationChannel.InApp:
                _db.Set<InAppNotification>().Add(new InAppNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = message.UserId,
                    Subject = message.Subject,
                    Body = message.Body,
                    CreatedAtUtc = DateTime.UtcNow,
                    IsRead = false
                });
                await _db.SaveChangesAsync(ct);
                break;
        }
    }
}

/// <summary>Entidad simple de persistencia para notificaciones in-app.</summary>
public class InAppNotification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; }
}
