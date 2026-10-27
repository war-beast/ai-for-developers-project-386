using CallCalendar.BLL.Model;

namespace CallCalendar.BLL.Service;

/// <summary>Правила сетки слотов, общие для выдачи слотов и бронирования.</summary>
internal static class SlotGrid
{
    public const int WindowDays = 14;
    public const int StepMinutes = 30;

    public static TimeZoneInfo TimeZone(OwnerOptions options) =>
        TimeZoneInfo.FindSystemTimeZoneById(options.TimeZone);

    public static (DateOnly Start, DateOnly End) Window(OwnerOptions options, TimeProvider timeProvider)
    {
        var today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), TimeZone(options)).DateTime);

        return (today.AddDays(1), today.AddDays(WindowDays));
    }

    public static IEnumerable<Slot> Slots(
        DateOnly date,
        int durationMinutes,
        OwnerOptions options,
        TimeZoneInfo timeZone)
    {
        if (!options.WorkDays.Contains(date.DayOfWeek))
        {
            yield break;
        }

        var dayStart = date.ToDateTime(options.WorkdayStart);
        var dayEnd = date.ToDateTime(options.WorkdayEnd);

        for (var start = dayStart;
             start.AddMinutes(durationMinutes) <= dayEnd;
             start = start.AddMinutes(StepMinutes))
        {
            var slotStart = new DateTimeOffset(start, timeZone.GetUtcOffset(start));
            yield return new Slot(slotStart, slotStart.AddMinutes(durationMinutes));
        }
    }

    /// <summary>Совпадает ли начало со слотом сетки текущего окна (без учёта записей).</summary>
    public static bool IsSlot(
        DateTimeOffset start,
        int durationMinutes,
        OwnerOptions options,
        TimeProvider timeProvider)
    {
        var timeZone = TimeZone(options);
        var (windowStart, windowEnd) = Window(options, timeProvider);
        var date = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(start, timeZone).DateTime);

        return date >= windowStart
            && date <= windowEnd
            && Slots(date, durationMinutes, options, timeZone)
                .Any(slot => slot.Start == start);
    }
}
