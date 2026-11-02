using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Repository;

namespace CallCalendar.Tests;

/// <summary>Запрос конфликтов и атомарный guard вставки в хранилище записей.</summary>
public class InMemoryBookingRepositoryTests
{
    // Среда, 28 октября 2026, 10:00 по Москве.
    private static readonly DateTimeOffset Ten = new(2026, 10, 28, 10, 0, 0, TimeSpan.FromHours(3));

    private readonly InMemoryBookingRepository _repository = new();

    [Fact]
    public void GetOverlappingSkipsBackToBackAndReturnsConflicts()
    {
        Assert.True(_repository.Add(Existing(-30, 30)));
        Assert.True(_repository.Add(Existing(0, 60)));

        var overlapping = _repository.GetOverlapping(Ten, Ten.AddMinutes(30));

        var booking = Assert.Single(overlapping);
        Assert.Equal(Ten, booking.Start);
    }

    [Fact]
    public void GetOverlappingAsOpenRayReturnsOnlyUpcoming()
    {
        Assert.True(_repository.Add(Existing(-60, 60)));
        Assert.True(_repository.Add(Existing(0, 60)));

        var upcoming = _repository.GetOverlapping(Ten, DateTimeOffset.MaxValue);

        var booking = Assert.Single(upcoming);
        Assert.Equal(Ten, booking.Start);
    }

    [Fact]
    public void AddRejectsOverlappingAndAcceptsBackToBack()
    {
        Assert.True(_repository.Add(Existing(0, 60)));
        Assert.False(_repository.Add(Existing(30, 30)));
        Assert.True(_repository.Add(Existing(60, 30)));

        var stored = _repository.GetOverlapping(Ten.AddHours(-1), Ten.AddHours(3));
        Assert.Equal(2, stored.Count);
    }

    private static BookingEntity Existing(int startMinute, int durationMinutes) => new()
    {
        Id = Guid.NewGuid(),
        EventTypeId = "intro-call",
        Start = Ten.AddMinutes(startMinute),
        End = Ten.AddMinutes(startMinute + durationMinutes),
        GuestName = "Гость",
        GuestEmail = "guest@example.com",
        CreatedAt = Ten,
    };
}
