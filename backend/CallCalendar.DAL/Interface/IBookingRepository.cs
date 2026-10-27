using CallCalendar.DAL.Entity;

namespace CallCalendar.DAL.Interface;

public interface IBookingRepository
{
    IReadOnlyList<BookingEntity> GetAll();

    /// <summary>Добавляет запись, если она не пересекается ни с одной существующей.</summary>
    bool TryAdd(BookingEntity booking);
}
