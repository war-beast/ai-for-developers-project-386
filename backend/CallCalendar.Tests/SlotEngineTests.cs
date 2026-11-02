using CallCalendar.BLL.Model;
using CallCalendar.BLL.Service;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Repository;
using Microsoft.Extensions.Options;

namespace CallCalendar.Tests;

public class SlotEngineTests
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

        var result = CreateEngine()
            .GetWindow("intro-call");

        var window = Assert.IsType<SlotWindow>(result.Window);
        Assert.Equal("Europe/Moscow", window.TimeZone);
        Assert.Equal(new DateOnly(2026, 10, 28), window.WindowStart);
        Assert.Equal(new DateOnly(2026, 11, 10), window.WindowEnd);
        Assert.Equal(30, window.StepMinutes);
        Assert.Equal(14, window.Days.Count);
        Assert.Equal(
            Enumerable.Range(1, 14)
                .Select(day => new DateOnly(2026, 10, 27).AddDays(day)),
            window.Days.Select(day => day.Date));
    }

    [Fact]
    public void WeekendDaysHaveNoSlots()
    {
        AddEventType("intro-call", 30);

        var window = CreateEngine()
            .GetWindow("intro-call").Window!;

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

        var slots = CreateEngine()
            .GetWindow("intro-call").Window!.Days[0].Slots;

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

        var window = CreateEngine()
            .GetWindow("too-long").Window;

        Assert.NotNull(window);
        Assert.Equal(14, window.Days.Count);
        Assert.All(window.Days, day => Assert.Empty(day.Slots));
    }

    [Fact]
    public void OccupiedIntervalIsExcluded()
    {
        AddEventType("intro-call", 30);
        _bookings.TryAdd(new BookingEntity
        {
            Id = Guid.NewGuid(),
            EventTypeId = "other-type",
            Start = new DateTimeOffset(2026, 10, 28, 10, 0, 0, MoscowOffset),
            End = new DateTimeOffset(2026, 10, 28, 10, 30, 0, MoscowOffset),
            GuestName = "Гость",
            GuestEmail = "guest@example.com",
            CreatedAt = Now,
        });

        var slots = CreateEngine()
            .GetWindow("intro-call").Window!.Days[0].Slots;

        Assert.DoesNotContain(slots, slot => slot.Start == new DateTimeOffset(2026, 10, 28, 10, 0, 0, MoscowOffset));
        Assert.Contains(slots, slot => slot.Start == new DateTimeOffset(2026, 10, 28, 9, 30, 0, MoscowOffset));
        Assert.Contains(slots, slot => slot.Start == new DateTimeOffset(2026, 10, 28, 10, 30, 0, MoscowOffset));
    }

    [Fact]
    public void UnknownEventTypeIsReportedAsNotFound()
    {
        var result = CreateEngine()
            .GetWindow("missing");

        Assert.Null(result.Window);
        Assert.True(result.EventTypeNotFound);
    }

    [Fact]
    public void ValidGridStartIsAccepted()
    {
        Assert.True(CreateEngine()
            .IsValidStart(At(28, 10, 0), 30));
    }

    [Theory]
    [InlineData(10, 28, 10, 15)] // не по шагу сетки
    [InlineData(10, 27, 15, 0)] // сегодня — до начала окна записи
    [InlineData(10, 31, 9, 0)] // суббота — не рабочий день
    [InlineData(11, 11, 9, 0)] // за пределами окна записи
    public void StartOutsideTheGridIsRejected(int month, int day, int hour, int minute)
    {
        Assert.False(CreateEngine()
            .IsValidStart(
                new DateTimeOffset(2026, month, day, hour, minute, 0, MoscowOffset),
                30));
    }

    private static DateTimeOffset At(int day, int hour, int minute) =>
        new(2026, 10, day, hour, minute, 0, MoscowOffset);

    private void AddEventType(string id, int durationMinutes) =>
        _eventTypes.TryAdd(new EventTypeEntity
        {
            Id = id,
            Title = id,
            Description = string.Empty,
            DurationMinutes = durationMinutes,
        });

    private SlotEngine CreateEngine() =>
        new(_eventTypes, _bookings, Options.Create(new OwnerOptions()), new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
