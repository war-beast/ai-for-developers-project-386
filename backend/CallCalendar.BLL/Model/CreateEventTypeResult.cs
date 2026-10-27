namespace CallCalendar.BLL.Model;

public sealed record CreateEventTypeResult(
    EventType? Created,
    IReadOnlyDictionary<string, string[]> Errors,
    bool SlugTaken)
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    public static CreateEventTypeResult Success(EventType eventType) =>
        new(eventType, NoErrors, false);

    public static CreateEventTypeResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(null, errors, false);

    public static CreateEventTypeResult Duplicate() =>
        new(null, NoErrors, true);
}
