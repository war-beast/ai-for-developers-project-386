namespace CallCalendar.BLL.Model;

/// <summary>Входные данные создания; null означает отсутствующее поле из JSON.</summary>
public sealed record CreateEventTypeCommand(
    string? Id,
    string? Title,
    string? Description,
    int DurationMinutes);
