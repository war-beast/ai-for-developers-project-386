using CallCalendar.BLL.Model;
using CallCalendar.DAL.Entity;

namespace CallCalendar.Tests;

/// <summary>Агрегат «Запись» владеет инвариантом пересечения: поверх — нельзя, встык — можно.</summary>
public class BookingTests
{
    // Среда, 28 октября 2026, 10:00 по Москве.
    private static readonly DateTimeOffset Ten = new(2026, 10, 28, 10, 0, 0, TimeSpan.FromHours(3));

    private static readonly EventTypeEntity IntroCall = new()
    {
        Id = "intro-call",
        Title = "Знакомство",
        Description = string.Empty,
        DurationMinutes = 30,
    };

    [Fact]
    public void OverlappingExistingBookingIsRejected()
    {
        var booking = Booking.TryCreate(
            IntroCall,
            Ten.AddMinutes(30),
            "Гость",
            "guest@example.com",
            null,
            [Existing(Ten, Ten.AddMinutes(60))],
            Ten);

        Assert.Null(booking);
    }

    [Fact]
    public void BookingRightAfterExistingEndIsAllowed()
    {
        var booking = Booking.TryCreate(
            IntroCall,
            Ten.AddMinutes(60),
            "Гость",
            "guest@example.com",
            null,
            [Existing(Ten, Ten.AddMinutes(60))],
            Ten);

        Assert.NotNull(booking);
    }

    [Fact]
    public void EndIsDerivedFromTypeDuration()
    {
        var booking = Booking.TryCreate(
            IntroCall,
            Ten,
            "Гость",
            "guest@example.com",
            null,
            [],
            Ten);

        Assert.NotNull(booking);
        Assert.Equal(Ten.AddMinutes(30), booking.End);
    }

    [Fact]
    public void GuestFieldsAreNormalized()
    {
        var booking = Booking.TryCreate(
            IntroCall,
            Ten,
            "  Гость  ",
            "  guest@example.com  ",
            "   ",
            [],
            Ten);

        Assert.NotNull(booking);
        Assert.Equal("Гость", booking.GuestName);
        Assert.Equal("guest@example.com", booking.GuestEmail);
        Assert.Null(booking.Comment);
    }

    [Fact]
    public void TypeIdentityIsCarriedAndIdCreatedAtAreStamped()
    {
        var booking = Booking.TryCreate(
            IntroCall,
            Ten,
            "Гость",
            "guest@example.com",
            "Перезвоните",
            [],
            Ten);

        Assert.NotNull(booking);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal("intro-call", booking.EventTypeId);
        Assert.Equal("Знакомство", booking.EventTypeTitle);
        Assert.Equal(Ten, booking.CreatedAt);
    }

    private static BookingEntity Existing(DateTimeOffset start, DateTimeOffset end) => new()
    {
        Id = Guid.NewGuid(),
        EventTypeId = "hour-call",
        Start = start,
        End = end,
        GuestName = "Гость",
        GuestEmail = "guest@example.com",
        CreatedAt = start,
    };
}
