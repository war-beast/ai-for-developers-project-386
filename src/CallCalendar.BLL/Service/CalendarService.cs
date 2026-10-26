using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;

namespace CallCalendar.BLL.Service;

public class CalendarService : ICalendarService
{
    public IReadOnlyList<CallSlot> GetSlots() =>
    [
        new CallSlot { Id = 1, Title = "Демо-звонок", StartsAt = DateTimeOffset.Parse("2026-10-27T10:00:00+03:00") },
        new CallSlot { Id = 2, Title = "Консультация", StartsAt = DateTimeOffset.Parse("2026-10-27T12:00:00+03:00") },
    ];
}
