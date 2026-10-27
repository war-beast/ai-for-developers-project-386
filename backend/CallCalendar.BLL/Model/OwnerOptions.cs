namespace CallCalendar.BLL.Model;

public sealed class OwnerOptions
{
    public string TimeZone { get; set; } = "Europe/Moscow";

    public TimeOnly WorkdayStart { get; set; } = new(9, 0);

    public TimeOnly WorkdayEnd { get; set; } = new(18, 0);

    public DayOfWeek[] WorkDays { get; set; } =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];
}
