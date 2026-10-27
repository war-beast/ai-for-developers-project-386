using CallCalendar.BLL.Model;
using CallCalendar.BLL.Service;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Repository;

namespace CallCalendar.Tests;

public class BookingServiceTests
{
    // Вторник, 27 октября 2026, 13:00 по Москве.
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 10, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan MoscowOffset = TimeSpan.FromHours(3);

    private readonly InMemoryEventTypeRepository _eventTypes = new();
    private readonly InMemoryBookingRepository _bookings = new();

    [Fact]
    public void OverlappingBookingIsRejected()
    {
        AddEventType("hour-call", 60);
        AddEventType("intro-call", 30);
        var existing = Create("hour-call", At(28, 10, 0));
        Assert.NotNull(existing.Created);

        var overlapping = Create("intro-call", At(28, 10, 30));

        Assert.Null(overlapping.Created);
        Assert.True(overlapping.SlotTaken);
    }

    [Fact]
    public void BookingRightAfterPreviousEndIsAllowed()
    {
        AddEventType("hour-call", 60);
        AddEventType("intro-call", 30);
        Assert.NotNull(Create("hour-call", At(28, 10, 0)).Created);

        var rightAfter = Create("intro-call", At(28, 11, 0));

        var booking = Assert.IsType<Booking>(rightAfter.Created);
        Assert.Equal(At(28, 11, 0), booking.Start);
        Assert.Equal(At(28, 11, 30), booking.End);
    }

    [Fact]
    public void InvalidGuestFieldsAreReported()
    {
        AddEventType("intro-call", 30);

        var result = CreateService().Create(new CreateBookingCommand(
            "intro-call",
            At(28, 10, 0),
            new string('Г', 101),
            "не-email",
            new string('К', 501)));

        Assert.Null(result.Created);
        Assert.True(result.Errors.ContainsKey("guestName"));
        Assert.True(result.Errors.ContainsKey("guestEmail"));
        Assert.True(result.Errors.ContainsKey("comment"));
    }

    [Fact]
    public void EmptyGuestNameAndEmailAreRejected()
    {
        AddEventType("intro-call", 30);

        var result = CreateService().Create(new CreateBookingCommand(
            "intro-call",
            At(28, 10, 0),
            "   ",
            "",
            null));

        Assert.Null(result.Created);
        Assert.True(result.Errors.ContainsKey("guestName"));
        Assert.True(result.Errors.ContainsKey("guestEmail"));
    }

    [Theory]
    [InlineData(10, 28, 10, 15)]
    [InlineData(10, 27, 15, 0)]
    [InlineData(10, 31, 9, 0)]
    [InlineData(11, 11, 9, 0)]
    public void StartOutsideTheSlotGridIsNotAvailable(int month, int day, int hour, int minute)
    {
        AddEventType("intro-call", 30);

        var result = CreateService().Create(new CreateBookingCommand(
            "intro-call",
            new DateTimeOffset(2026, month, day, hour, minute, 0, MoscowOffset),
            "Гость",
            "guest@example.com",
            null));

        Assert.Null(result.Created);
        Assert.True(result.SlotNotAvailable);
    }

    [Fact]
    public void UnknownEventTypeIsNotFound()
    {
        var result = Create("missing", At(28, 10, 0));

        Assert.Null(result.Created);
        Assert.True(result.EventTypeNotFound);
        Assert.False(result.SlotTaken);
    }

    [Fact]
    public void CreatedBookingCarriesTypeAndGuestDetails()
    {
        AddEventType("intro-call", 30, "Знакомство");

        var result = CreateService().Create(new CreateBookingCommand(
            "intro-call",
            At(28, 10, 0),
            "  Гость  ",
            "  guest@example.com  ",
            "   "));

        var booking = Assert.IsType<Booking>(result.Created);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal("intro-call", booking.EventTypeId);
        Assert.Equal("Знакомство", booking.EventTypeTitle);
        Assert.Equal(At(28, 10, 0), booking.Start);
        Assert.Equal(At(28, 10, 30), booking.End);
        Assert.Equal("Гость", booking.GuestName);
        Assert.Equal("guest@example.com", booking.GuestEmail);
        Assert.Null(booking.Comment);
        Assert.Equal(Now, booking.CreatedAt);
    }

    private CreateBookingResult Create(string eventTypeId, DateTimeOffset start) =>
        CreateService().Create(new CreateBookingCommand(
            eventTypeId,
            start,
            "Гость",
            "guest@example.com",
            null));

    private static DateTimeOffset At(int day, int hour, int minute) =>
        new(2026, 10, day, hour, minute, 0, MoscowOffset);

    private void AddEventType(string id, int durationMinutes, string? title = null) =>
        _eventTypes.TryAdd(new EventTypeEntity
        {
            Id = id,
            Title = title ?? id,
            Description = string.Empty,
            DurationMinutes = durationMinutes,
        });

    private BookingService CreateService() =>
        new(_eventTypes, _bookings, new OwnerOptions(), new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
