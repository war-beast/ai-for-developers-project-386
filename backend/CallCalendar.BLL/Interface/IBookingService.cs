using CallCalendar.BLL.Model;

namespace CallCalendar.BLL.Interface;

public interface IBookingService
{
    CreateBookingResult Create(CreateBookingCommand command);

    /// <summary>Предстоящие записи всех типов по возрастанию начала.</summary>
    IReadOnlyList<Booking> GetUpcoming();
}
