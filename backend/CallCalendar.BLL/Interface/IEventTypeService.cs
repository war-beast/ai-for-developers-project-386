using CallCalendar.BLL.Model;

namespace CallCalendar.BLL.Interface;

public interface IEventTypeService
{
    IReadOnlyList<EventType> GetAll();

    CreateEventTypeResult Create(CreateEventTypeCommand command);
}
