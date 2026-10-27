namespace CallCalendar.BLL.Model;

public sealed record EventType(string Id, string Title, string Description, int DurationMinutes);
