namespace CallCalendar.DAL.Entity;

public sealed class BookingEntity
{
    public required Guid Id { get; init; }

    public required string EventTypeId { get; init; }

    public required DateTimeOffset Start { get; init; }

    public required DateTimeOffset End { get; init; }

    public required string GuestName { get; init; }

    public required string GuestEmail { get; init; }

    public string? Comment { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
