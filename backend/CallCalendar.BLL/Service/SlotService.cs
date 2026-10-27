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
    private const int WindowDays = 14;
    private const int SlotStepMinutes = 30;

    public SlotWindowResult GetWindow(string eventTypeId)
    {
        var eventType = eventTypeRepository.GetById(eventTypeId);
        if (eventType is null)
        {
            return SlotWindowResult.NotFound();
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.TimeZone);
        var today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).DateTime);

        var windowStart = today.AddDays(1);
        var windowEnd = today.AddDays(WindowDays);
        var bookings = bookingRepository.GetAll();

        var days = Enumerable
            .Range(1, WindowDays)
            .Select(offset => BuildDay(windowStart.AddDays(offset - 1), eventType.DurationMinutes, timeZone, bookings))
            .ToList();

        return SlotWindowResult.Success(new SlotWindow(
            options.TimeZone,
            windowStart,
            windowEnd,
            SlotStepMinutes,
            days));
    }

    private SlotDay BuildDay(
        DateOnly date,
        int durationMinutes,
        TimeZoneInfo timeZone,
        IReadOnlyList<BookingEntity> bookings)
    {
        if (!options.WorkDays.Contains(date.DayOfWeek))
        {
            return new SlotDay(date, []);
        }

        var dayStart = date.ToDateTime(options.WorkdayStart);
        var dayEnd = date.ToDateTime(options.WorkdayEnd);
        var slots = new List<Slot>();

        for (var start = dayStart; start.AddMinutes(durationMinutes) <= dayEnd; start = start.AddMinutes(SlotStepMinutes))
        {
            var slotStart = new DateTimeOffset(start, timeZone.GetUtcOffset(start));
            var slotEnd = slotStart.AddMinutes(durationMinutes);

            if (bookings.Any(booking => slotStart < booking.End && booking.Start < slotEnd))
            {
                continue;
            }

            slots.Add(new Slot(slotStart, slotEnd));
        }

        return new SlotDay(date, slots);
    }
}
