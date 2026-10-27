using CallCalendar.BLL.Model;

namespace CallCalendar.BLL.Interface;

public interface ISlotService
{
    SlotWindowResult GetWindow(string eventTypeId);
}
