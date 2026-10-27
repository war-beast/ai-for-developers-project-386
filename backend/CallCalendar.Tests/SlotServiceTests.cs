using CallCalendar.BLL.Model;
using CallCalendar.BLL.Service;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Repository;

namespace CallCalendar.Tests;

public class SlotServiceTests
{
    // Вторник, 27 октября 2026, 13:00 по Москве.
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 10, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan MoscowOffset = TimeSpan.FromHours(3);

    private readonly InMemoryEventTypeRepository _eventTypes = new();
    private readonly InMemoryBookingRepository _bookings = new();

    [Fact]
    public void WindowStartsTomorrowAndSpansFourteenDays()
    {
        AddEventType("intro-call", 30);

        var result = CreateService().GetWindow("intro-call");

        var window = Assert.IsType<SlotWindow>(result.Window);
        Assert.Equal("Europe/Moscow", window.TimeZone);
        Assert.Equal(new DateOnly(2026, 10, 28), window.WindowStart);
        Assert.Equal(new DateOnly(2026, 11, 10), window.WindowEnd);
        Assert.Equal(30, window.StepMinutes);
        Assert.Equal(14, window.Days.Count);
        Assert.Equal(
            Enumerable.Range(1, 14).Select(day => new DateOnly(2026, 10, 27).AddDays(day)),
            window.Days.Select(day => day.Date));
    }

    [Fact]
    public void WeekendDaysHaveNoSlots()
    {
        AddEventType("intro-call", 30);

        var window = CreateService().GetWindow("intro-call").Window!;

        Assert.Empty(window.Days.Single(day => day.Date == new DateOnly(2026, 10, 31)).Slots);
        Assert.Empty(window.Days.Single(day => day.Date == new DateOnly(2026, 11, 1)).Slots);
        Assert.NotEmpty(window.Days.Single(day => day.Date == new DateOnly(2026, 10, 28)).Slots);
    }

    [Theory]
    [InlineData(30, 17, 30)]
    [InlineData(45, 17, 0)]
    [InlineData(60, 17, 0)]
    [InlineData(90, 16, 30)]
    public void LastStartFitsIntoWorkday(int durationMinutes, int expectedHour, int expectedMinute)
    {
        AddEventType("intro-call", durationMinutes);

        var slots = CreateService().GetWindow("intro-call").Window!.Days[0].Slots;

        Assert.Equal(
            new DateTimeOffset(2026, 10, 28, 9, 0, 0, MoscowOffset),
            slots[0].Start);
        Assert.Equal(
            new DateTimeOffset(2026, 10, 28, expectedHour, expectedMinute, 0, MoscowOffset),
            slots[^1].Start);
        Assert.All(
            slots,
            slot => Assert.True(
                slot.End <= new DateTimeOffset(2026, 10, 28, 18, 0, 0, MoscowOffset),
                $"Слот {slot.Start} заканчивается в {slot.End}"));
    }

    [Fact]
    public void TypeLongerThanWorkdayGivesEmptyDays()
    {
        AddEventType("too-long", 600);

        var window = CreateService().GetWindow("too-long").Window;

        Assert.NotNull(window);
        Assert.Equal(14, window.Days.Count);
        Assert.All(window.Days, day => Assert.Empty(day.Slots));
    }

    [Fact]
    public void OccupiedIntervalIsExcluded()
    {
        AddEventType("intro-call", 30);
        _bookings.Add(new BookingEntity
        {
            Id = Guid.NewGuid(),
            EventTypeId = "other-type",
            Start = new DateTimeOffset(2026, 10, 28, 10, 0, 0, MoscowOffset),
            End = new DateTimeOffset(2026, 10, 28, 10, 30, 0, MoscowOffset),
            GuestName = "Гость",
            GuestEmail = "guest@example.com",
            CreatedAt = Now,
        });

        var slots = CreateService().GetWindow("intro-call").Window!.Days[0].Slots;

        Assert.DoesNotContain(slots, slot => slot.Start == new DateTimeOffset(2026, 10, 28, 10, 0, 0, MoscowOffset));
        Assert.Contains(slots, slot => slot.Start == new DateTimeOffset(2026, 10, 28, 9, 30, 0, MoscowOffset));
        Assert.Contains(slots, slot => slot.Start == new DateTimeOffset(2026, 10, 28, 10, 30, 0, MoscowOffset));
    }

    [Fact]
    public void UnknownEventTypeIsReportedAsNotFound()
    {
        var result = CreateService().GetWindow("missing");

        Assert.Null(result.Window);
        Assert.True(result.EventTypeNotFound);
    }

    private void AddEventType(string id, int durationMinutes) =>
        _eventTypes.TryAdd(new EventTypeEntity
        {
            Id = id,
            Title = id,
            Description = string.Empty,
            DurationMinutes = durationMinutes,
        });

    private SlotService CreateService() =>
        new(_eventTypes, _bookings, new OwnerOptions(), new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
