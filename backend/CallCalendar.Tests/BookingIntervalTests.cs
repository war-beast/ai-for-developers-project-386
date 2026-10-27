using CallCalendar.DAL;

namespace CallCalendar.Tests;

/// <summary>Записи занимают полуинтервал [start, end): встык — не пересечение.</summary>
public class BookingIntervalTests
{
    private static readonly DateTimeOffset Ten = new(2026, 10, 28, 10, 0, 0, TimeSpan.FromHours(3));

    [Theory]
    [InlineData(0, 60, 60, 120, false)] // запись сразу после окончания
    [InlineData(0, 60, 120, 180, false)]
    [InlineData(60, 120, 0, 60, false)]
    [InlineData(0, 60, 30, 90, true)]
    [InlineData(0, 60, 10, 20, true)]
    [InlineData(10, 20, 0, 60, true)]
    [InlineData(0, 60, 0, 60, true)]
    public void OverlapsFollowsHalfOpenIntervals(
        int firstStart,
        int firstEnd,
        int secondStart,
        int secondEnd,
        bool expected) =>
        Assert.Equal(
            expected,
            BookingInterval.Overlaps(
                Ten.AddMinutes(firstStart),
                Ten.AddMinutes(firstEnd),
                Ten.AddMinutes(secondStart),
                Ten.AddMinutes(secondEnd)));
}
