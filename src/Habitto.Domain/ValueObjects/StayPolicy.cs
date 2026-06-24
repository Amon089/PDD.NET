namespace Habitto.Domain.ValueObjects;

/// <summary>
/// Política estándar de horarios, fija a nivel de dominio (no configurable
/// por usuario ni por inmueble). Modelarla como VO -en vez de como literal
/// "2:00 PM" regado en el código- permite que si el negocio decide cambiarla
/// algún día, exista un único punto de verdad.
/// </summary>
public sealed class StayPolicy
{
    public static readonly TimeOnly CheckInTime = new(14, 0);
    public static readonly TimeOnly CheckOutTime = new(12, 0);

    public static StayPolicy Default { get; } = new();

    private StayPolicy() { }

    public DateTime CheckInDateTime(DateOnly date) => date.ToDateTime(CheckInTime);

    public DateTime CheckOutDateTime(DateOnly date) => date.ToDateTime(CheckOutTime);
}
