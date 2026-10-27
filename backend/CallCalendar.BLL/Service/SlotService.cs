using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Interface;

namespace CallCalendar.BLL.Service;

public sealed class SlotService(
    IEventTypeRepository eventTypeRepository,
    IBookingRepository bookingRepository,
    OwnerOptions options,
    TimeProvider timeProvider) : ISlotService
{
    public SlotWindowResult GetWindow(string eventTypeId)
    {
        var eventType = eventTypeRepository.GetById(eventTypeId);
        if (eventType is null)
        {
            return SlotWindowResult.NotFound();
        }

        var timeZone = SlotGrid.TimeZone(options);
        var (windowStart, windowEnd) = SlotGrid.Window(options, timeProvider);
        var bookings = bookingRepository.GetAll();

        var days = Enumerable
            .Range(0, SlotGrid.WindowDays)
            .Select(offset => BuildDay(windowStart.AddDays(offset), eventType.DurationMinutes, timeZone, bookings))
            .ToList();

        return SlotWindowResult.Success(new SlotWindow(
            options.TimeZone,
            windowStart,
            windowEnd,
            SlotGrid.StepMinutes,
            days));
    }

    private SlotDay BuildDay(
        DateOnly date,
        int durationMinutes,
        TimeZoneInfo timeZone,
        IReadOnlyList<BookingEntity> bookings)
    {
        var slots = SlotGrid
            .Slots(date, durationMinutes, options, timeZone)
            .Where(slot => !bookings.Any(booking => slot.Start < booking.End && booking.Start < slot.End))
            .ToList();

        return new SlotDay(date, slots);
    }
}
