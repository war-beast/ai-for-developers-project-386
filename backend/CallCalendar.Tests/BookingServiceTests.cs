using CallCalendar.BLL.Model;
using CallCalendar.BLL.Service;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Interface;
using CallCalendar.DAL.Repository;
using Microsoft.Extensions.Options;
using Moq;

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

        var result = CreateService()
            .Create(new CreateBookingCommand(
                "intro-call",
                At(28, 10, 0),
                "Гость",
                "не-email",
                new string('К', 501)));

        Assert.Null(result.Created);
        Assert.False(result.Errors.ContainsKey("guestName"));
        Assert.True(result.Errors.ContainsKey("guestEmail"));
        Assert.True(result.Errors.ContainsKey("comment"));
    }

    [Fact]
    public void LongGuestNameIsAccepted()
    {
        AddEventType("intro-call", 30);
        var name = new string('Г', 200);

        var result = CreateService()
            .Create(new CreateBookingCommand(
                "intro-call",
                At(28, 10, 0),
                $"  {name}  ",
                "guest@example.com",
                null));

        var booking = Assert.IsType<Booking>(result.Created);
        Assert.Equal(name, booking.GuestName);
    }

    [Fact]
    public void EmptyGuestNameAndEmailAreRejected()
    {
        AddEventType("intro-call", 30);

        var result = CreateService()
            .Create(new CreateBookingCommand(
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

        var result = CreateService()
            .Create(new CreateBookingCommand(
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

        var result = CreateService()
            .Create(new CreateBookingCommand(
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

    [Fact]
    public void UpcomingSkipsFinishedMeetingsAndKeepsOngoingOne()
    {
        AddEventType("intro-call", 30, "Знакомство");
        AddBooking("intro-call", At(27, 11, 0), At(27, 11, 30));
        AddBooking("intro-call", At(27, 12, 45), At(27, 13, 15));

        var upcoming = CreateService()
            .GetUpcoming();

        var booking = Assert.Single(upcoming);
        Assert.Equal(At(27, 12, 45), booking.Start);
    }

    [Fact]
    public void UpcomingIsSortedByStart()
    {
        AddEventType("intro-call", 30, "Знакомство");
        AddBooking("intro-call", At(28, 12, 0), At(28, 12, 30));
        AddBooking("intro-call", At(28, 9, 0), At(28, 9, 30));
        AddBooking("intro-call", At(27, 12, 45), At(27, 13, 15));

        var upcoming = CreateService()
            .GetUpcoming();

        Assert.Equal(
            [At(27, 12, 45), At(28, 9, 0), At(28, 12, 0)],
            upcoming.Select(booking => booking.Start));
    }

    [Fact]
    public void UpcomingCarriesTypeTitleAndGuestDetails()
    {
        AddEventType("intro-call", 30, "Знакомство");
        AddBooking("intro-call", At(28, 10, 0), At(28, 10, 30));

        var booking = Assert.Single(CreateService()
            .GetUpcoming());

        Assert.Equal("intro-call", booking.EventTypeId);
        Assert.Equal("Знакомство", booking.EventTypeTitle);
        Assert.Equal("Гость", booking.GuestName);
        Assert.Equal("guest@example.com", booking.GuestEmail);
        Assert.Equal("Перезвоните", booking.Comment);
        Assert.Equal(At(28, 10, 30), booking.End);
    }

    [Fact]
    public void UpcomingLoadsEventTypesOnceInsteadOfPerBooking()
    {
        // Строгий мок: GetById на каждую запись — это N+1, обращение упадёт.
        var eventTypes = new Mock<IEventTypeRepository>(MockBehavior.Strict);
        eventTypes
            .Setup(repository => repository.GetAll())
            .Returns([
                new EventTypeEntity
                {
                    Id = "intro-call",
                    Title = "Знакомство",
                    Description = string.Empty,
                    DurationMinutes = 30,
                },
            ]);
        AddBooking("intro-call", At(28, 10, 0), At(28, 10, 30));
        var service = new BookingService(
            eventTypes.Object,
            _bookings,
            CreateEngine(),
            new FixedTimeProvider(Now));

        var booking = Assert.Single(service.GetUpcoming());

        Assert.Equal("Знакомство", booking.EventTypeTitle);
    }

    private void AddBooking(string eventTypeId, DateTimeOffset start, DateTimeOffset end) =>
        _bookings.TryAdd(new BookingEntity
        {
            Id = Guid.NewGuid(),
            EventTypeId = eventTypeId,
            Start = start,
            End = end,
            GuestName = "Гость",
            GuestEmail = "guest@example.com",
            Comment = "Перезвоните",
            CreatedAt = Now,
        });

    private CreateBookingResult Create(string eventTypeId, DateTimeOffset start) =>
        CreateService()
            .Create(new CreateBookingCommand(
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
        new(_eventTypes, _bookings, CreateEngine(), new FixedTimeProvider(Now));

    private SlotEngine CreateEngine() =>
        new(_eventTypes, _bookings, Options.Create(new OwnerOptions()), new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
