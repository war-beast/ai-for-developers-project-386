namespace CallCalendar.BLL.Model;

/// <summary>Входные данные записи; null означает отсутствующее поле из JSON.</summary>
public sealed record CreateBookingCommand(
    string? EventTypeId,
    DateTimeOffset? Start,
    string? GuestName,
    string? GuestEmail,
    string? Comment);
