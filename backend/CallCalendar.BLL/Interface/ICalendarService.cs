using CallCalendar.BLL.Model;

namespace CallCalendar.BLL.Interface;

public interface ICalendarService
{
    IReadOnlyList<CallSlot> GetSlots();
}
