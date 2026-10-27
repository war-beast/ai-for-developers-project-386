namespace CallCalendar.BLL.Model;

public sealed record CreateBookingResult(
    Booking? Created,
    IReadOnlyDictionary<string, string[]> Errors,
    bool EventTypeNotFound,
    bool SlotNotAvailable,
    bool SlotTaken)
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    public static CreateBookingResult Success(Booking booking) =>
        new(booking, NoErrors, false, false, false);

    public static CreateBookingResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(null, errors, false, false, false);

    public static CreateBookingResult EventTypeMissing() =>
        new(null, NoErrors, true, false, false);

    public static CreateBookingResult SlotUnavailable() =>
        new(null, NoErrors, false, true, false);

    public static CreateBookingResult SlotAlreadyTaken() =>
        new(null, NoErrors, false, false, true);
}
