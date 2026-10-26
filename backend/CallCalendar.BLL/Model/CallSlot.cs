namespace CallCalendar.BLL.Model;

public class CallSlot
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset StartsAt { get; set; }
}
