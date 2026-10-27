namespace CallCalendar.Api.ProblemResults;

/// <summary>Коды ошибок API — единый источник для контроллеров, фабрики и стратегий.</summary>
internal static class ProblemCodes
{
    public const string EventTypeNotFound = "event_type_not_found";
    public const string EventTypeExists = "event_type_exists";
    public const string SlotTaken = "slot_taken";
    public const string SlotNotAvailable = "slot_not_available";
    public const string ValidationFailed = "validation_failed";
}
