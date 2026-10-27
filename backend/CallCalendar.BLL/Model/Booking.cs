using System.Text.Json.Serialization;

namespace CallCalendar.BLL.Model;

public sealed record Booking(
    Guid Id,
    string EventTypeId,
    string EventTypeTitle,
    DateTimeOffset Start,
    DateTimeOffset End,
    string GuestName,
    string GuestEmail,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Comment,
    DateTimeOffset CreatedAt);
