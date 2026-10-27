namespace CallCalendar.BLL.Model;

public sealed record Slot(DateTimeOffset Start, DateTimeOffset End);

public sealed record SlotDay(DateOnly Date, IReadOnlyList<Slot> Slots);

public sealed record SlotWindow(
    string TimeZone,
    DateOnly WindowStart,
    DateOnly WindowEnd,
    int StepMinutes,
    IReadOnlyList<SlotDay> Days);

public sealed record SlotWindowResult(SlotWindow? Window, bool EventTypeNotFound)
{
    public static SlotWindowResult Success(SlotWindow window) => new(window, false);

    public static SlotWindowResult NotFound() => new(null, true);
}
