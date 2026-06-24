using Habitto.Application.Common;

namespace Habitto.Infrastructure.Services;

/// <summary>
/// Implementación MOCK de KYC. Simula el procesamiento de la cédula sin
/// llamar a ningún proveedor real. Útil para demo y para no depender de
/// credenciales de servicios de IA durante la prueba académica.
///
/// Regla simulada: aprueba si la imagen recibida pesa más de 0 bytes
/// (simplemente valida que llegó algo) y rechaza aleatoriamente 1 de cada 5
/// veces, para que el flujo de "rechazado" también sea demostrable.
/// </summary>
public class MockIdentityVerificationService : IIdentityVerificationService
{
    private static readonly Random _random = new();

    public Task<IdentityExtractionResult> VerifyAsync(byte[] documentImage, CancellationToken ct = default)
    {
        if (documentImage.Length == 0)
            return Task.FromResult(new IdentityExtractionResult("N/A", "N/A", "N/A", default, false));

        var approved = _random.Next(0, 5) != 0;

        var result = new IdentityExtractionResult(
            FirstName: "Juan",
            LastName: "Pérez",
            DocumentNumber: "1000000000",
            DateOfBirth: new DateOnly(1995, 1, 1),
            Approved: approved);

        return Task.FromResult(result);
    }
}
