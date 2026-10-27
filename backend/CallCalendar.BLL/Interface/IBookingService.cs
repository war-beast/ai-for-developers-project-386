using CallCalendar.BLL.Model;

namespace CallCalendar.BLL.Interface;

public interface IBookingService
{
    CreateBookingResult Create(CreateBookingCommand command);
}
