using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Interface;

namespace CallCalendar.DAL.Repository;

/// <summary>
/// Записи в памяти процесса: сбрасываются при рестарте, как и типы встреч.
/// </summary>
public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly Lock _lock = new();
    private readonly List<BookingEntity> _bookings = [];

    public IReadOnlyList<BookingEntity> GetAll()
    {
        lock (_lock)
        {
            return [.. _bookings];
        }
    }

    public void Add(BookingEntity booking)
    {
        lock (_lock)
        {
            _bookings.Add(booking);
        }
    }
}
