namespace Habitto.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public sealed class BookingOverlapException : DomainException
{
    public BookingOverlapException(Guid propertyId, DateOnly start, DateOnly end)
        : base($"El inmueble {propertyId} ya tiene una reserva que se solapa con el rango {start:yyyy-MM-dd} -> {end:yyyy-MM-dd}.")
    {
    }
}

public sealed class IdentityNotVerifiedException : DomainException
{
    public IdentityNotVerifiedException(Guid userId)
        : base($"El usuario {userId} no tiene una validación de identidad aprobada. No puede finalizar la reserva.")
    {
    }
}
