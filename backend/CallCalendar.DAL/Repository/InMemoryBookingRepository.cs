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

    public bool TryAdd(BookingEntity booking)
    {
        lock (_lock)
        {
            // Полуинтервалы: запись сразу после окончания предыдущей пересечением не считается.
            if (_bookings.Exists(existing => booking.Start < existing.End && existing.Start < booking.End))
            {
                return false;
            }

            _bookings.Add(booking);
            return true;
        }
    }
}
