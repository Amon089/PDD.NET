namespace Habitto.Domain.ValueObjects;

/// <summary>
/// Value Object inmutable que representa un rango de fechas de una reserva.
/// Encapsula la regla de negocio de solapamiento: dos rangos se solapan
/// si y solo si StartA < EndB y StartB < EndA (regla estándar de intervalos).
/// Usamos límites [Start, End) -> el check-out del mismo día que el check-in
/// de otra reserva NO se considera solapamiento (rotación same-day).
/// </summary>
public sealed class DateRange : IEquatable<DateRange>
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Create(DateOnly start, DateOnly end)
    {
        if (end <= start)
            throw new ArgumentException(
                "La fecha de salida debe ser posterior a la fecha de llegada.",
                nameof(end));

        return new DateRange(start, end);
    }

    /// <summary>
    /// Determina si este rango se solapa con otro.
    /// Regla: (StartA &lt; EndB) AND (StartB &lt; EndA)
    /// </summary>
    public bool Overlaps(DateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Start < other.End && other.Start < End;
    }

    public int Nights => End.DayNumber - Start.DayNumber;

    public bool Equals(DateRange? other)
    {
        if (other is null) return false;
        return Start == other.Start && End == other.End;
    }

    public override bool Equals(object? obj) => Equals(obj as DateRange);

    public override int GetHashCode() => HashCode.Combine(Start, End);

    public override string ToString() => $"{Start:yyyy-MM-dd} -> {End:yyyy-MM-dd}";
}
