using CallCalendar.DAL.Entity;

namespace CallCalendar.DAL.Interface;

public interface IBookingRepository
{
    IReadOnlyList<BookingEntity> GetAll();

    void Add(BookingEntity booking);
}
