using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using CallCalendar.DAL;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Interface;
using Microsoft.Extensions.Options;

namespace CallCalendar.BLL.Service;

/// <summary>Единственный владелец правил слота: сетка, окно записи и занятые интервалы.</summary>
public sealed class SlotEngine(
    IEventTypeRepository eventTypeRepository,
    IBookingRepository bookingRepository,
    IOptions<OwnerOptions> ownerOptions,
    TimeProvider timeProvider) : ISlotEngine
{
    private readonly OwnerOptions options = ownerOptions.Value;

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

    public bool IsValidStart(DateTimeOffset start, int durationMinutes) =>
        SlotGrid.IsSlot(start, durationMinutes, options, timeProvider);

    private SlotDay BuildDay(
        DateOnly date,
        int durationMinutes,
        TimeZoneInfo timeZone,
        IReadOnlyList<BookingEntity> bookings)
    {
        var slots = SlotGrid
            .Slots(date, durationMinutes, options, timeZone)
            .Where(slot => !bookings.Any(booking => BookingInterval.Overlaps(
                slot.Start, slot.End, booking.Start, booking.End)))
            .ToList();

        return new SlotDay(date, slots);
    }
}
