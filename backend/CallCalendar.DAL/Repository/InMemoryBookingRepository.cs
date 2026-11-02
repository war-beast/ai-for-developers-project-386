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

    public IReadOnlyList<BookingEntity> GetOverlapping(DateTimeOffset start, DateTimeOffset end)
    {
        lock (_lock)
        {
            return [.. _bookings.Where(booking => BookingInterval.Overlaps(
                start, end, booking.Start, booking.End))];
        }
    }

    public bool Add(BookingEntity booking)
    {
        lock (_lock)
        {
            if (_bookings.Exists(existing => BookingInterval.Overlaps(
                    booking.Start, booking.End, existing.Start, existing.End)))
            {
                return false;
            }

            _bookings.Add(booking);
            return true;
        }
    }
}
