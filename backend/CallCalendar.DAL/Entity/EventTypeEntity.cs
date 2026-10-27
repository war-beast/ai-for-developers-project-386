namespace CallCalendar.DAL.Entity;

public sealed class EventTypeEntity
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required int DurationMinutes { get; init; }
}
