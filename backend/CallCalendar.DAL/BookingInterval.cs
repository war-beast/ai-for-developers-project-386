namespace CallCalendar.DAL;

/// <summary>Правило пересечения записей, общее для репозитория и выдачи слотов.</summary>
public static class BookingInterval
{
    /// <summary>
    /// Пересекаются ли полуинтервалы [start, end). Запись сразу после окончания
    /// предыдущей пересечением не считается.
    /// </summary>
    public static bool Overlaps(
        DateTimeOffset start,
        DateTimeOffset end,
        DateTimeOffset otherStart,
        DateTimeOffset otherEnd) =>
        start < otherEnd && otherStart < end;
}
