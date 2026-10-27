using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CallCalendar.BLL.Model;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CallCalendar.Tests;

public class BookingsApiTests
{
    [Fact]
    public async Task CreateReturnsBookingAndRemovesSlotFromWindow()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        await CreateEventType(client, "intro-call", "Знакомство", 30);

        var start = await FirstFreeSlot(client, "intro-call");
        var response = await client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingCommand("intro-call", start, "Гость", "guest@example.com", "Перезвоните"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var booking = await response.Content.ReadFromJsonAsync<Booking>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(booking);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal("intro-call", booking.EventTypeId);
        Assert.Equal("Знакомство", booking.EventTypeTitle);
        Assert.Equal(start, booking.Start);
        Assert.Equal(start.AddMinutes(30), booking.End);
        Assert.Equal("Гость", booking.GuestName);
        Assert.Equal("guest@example.com", booking.GuestEmail);
        Assert.Equal("Перезвоните", booking.Comment);

        var window = await client.GetFromJsonAsync<SlotWindow>(
            "/api/event-types/intro-call/slots",
            TestContext.Current.CancellationToken);
        Assert.DoesNotContain(window!.Days.SelectMany(day => day.Slots), slot => slot.Start == start);
    }

    [Fact]
    public async Task OverlappingBookingReturnsSlotTaken()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        await CreateEventType(client, "hour-call", "Час", 60);
        await CreateEventType(client, "intro-call", "Знакомство", 30);
        var start = await FirstFreeSlot(client, "hour-call");
        await Book(client, "hour-call", start);

        var response = await client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingCommand("intro-call", start.AddMinutes(30), "Гость", "guest@example.com", null),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(
            "slot_taken",
            await ProblemCode(response));
    }

    [Fact]
    public async Task StartOutsideTheSlotGridReturnsSlotNotAvailable()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        await CreateEventType(client, "intro-call", "Знакомство", 30);
        var slot = await FirstFreeSlot(client, "intro-call");
        var offGrid = new DateTimeOffset(
            slot.Year, slot.Month, slot.Day, 10, 15, 0, TimeSpan.FromHours(3));

        var response = await client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingCommand("intro-call", offGrid, "Гость", "guest@example.com", null),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("slot_not_available", await ProblemCode(response));
    }

    [Fact]
    public async Task UnknownEventTypeReturnsNotFound()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingCommand("missing", DateTimeOffset.UtcNow.AddDays(3), "Гость", "guest@example.com", null),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("event_type_not_found", await ProblemCode(response));
    }

    [Fact]
    public async Task InvalidFieldsReturnValidationProblem()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        await CreateEventType(client, "intro-call", "Знакомство", 30);
        var start = await FirstFreeSlot(client, "intro-call");

        var response = await client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingCommand("intro-call", start, " ", "не-email", new string('К', 501)),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal("validation_failed", problem.RootElement.GetProperty("code")
            .GetString());
        var errors = problem.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("guestName", out _));
        Assert.True(errors.TryGetProperty("guestEmail", out _));
        Assert.True(errors.TryGetProperty("comment", out _));
    }

    [Fact]
    public async Task ConcurrentBookingsForOneSlotCreateExactlyOne()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        await CreateEventType(client, "intro-call", "Знакомство", 30);
        var start = await FirstFreeSlot(client, "intro-call");

        var responses = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ =>
                client.PostAsJsonAsync(
                    "/api/bookings",
                    new CreateBookingCommand("intro-call", start, "Гость", "guest@example.com", null),
                    TestContext.Current.CancellationToken)));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Equal(9, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task ListReturnsUpcomingBookings()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        await CreateEventType(client, "intro-call", "Знакомство", 30);
        var start = await FirstFreeSlot(client, "intro-call");
        await Book(client, "intro-call", start);

        var response = await client.GetAsync(
            "/api/bookings",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>(
            TestContext.Current.CancellationToken);
        var booking = Assert.Single(bookings!);
        Assert.Equal("intro-call", booking.EventTypeId);
        Assert.Equal("Знакомство", booking.EventTypeTitle);
        Assert.Equal(start, booking.Start);
        Assert.Equal("Гость", booking.GuestName);
        Assert.Equal("guest@example.com", booking.GuestEmail);
    }

    [Fact]
    public async Task ListIsSortedByStart()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        await CreateEventType(client, "intro-call", "Знакомство", 30);
        var slots = await FreeSlots(client, "intro-call");
        await Book(client, "intro-call", slots[1]);
        await Book(client, "intro-call", slots[0]);

        var bookings = await client.GetFromJsonAsync<List<Booking>>(
            "/api/bookings",
            TestContext.Current.CancellationToken);

        Assert.Equal([slots[0], slots[1]], bookings!.Select(booking => booking.Start));
    }

    [Fact]
    public async Task ListIsEmptyBeforeAnyBooking()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/bookings",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>(
            TestContext.Current.CancellationToken);
        Assert.Empty(bookings!);
    }

    private static async Task CreateEventType(
        HttpClient client,
        string id,
        string title,
        int durationMinutes)
    {
        var response = await client.PostAsJsonAsync(
            "/api/event-types",
            new CreateEventTypeCommand(id, title, string.Empty, durationMinutes),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<DateTimeOffset> FirstFreeSlot(HttpClient client, string eventTypeId) =>
        (await FreeSlots(client, eventTypeId))[0];

    private static async Task<IReadOnlyList<DateTimeOffset>> FreeSlots(
        HttpClient client,
        string eventTypeId)
    {
        var window = await client.GetFromJsonAsync<SlotWindow>(
            $"/api/event-types/{eventTypeId}/slots",
            TestContext.Current.CancellationToken);
        return window!.Days.SelectMany(day => day.Slots)
            .Select(slot => slot.Start)
            .ToList();
    }

    private static async Task Book(HttpClient client, string eventTypeId, DateTimeOffset start)
    {
        var response = await client.PostAsJsonAsync(
            "/api/bookings",
            new CreateBookingCommand(eventTypeId, start, "Гость", "guest@example.com", null),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<string?> ProblemCode(HttpResponseMessage response)
    {
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
        return problem.RootElement.GetProperty("code")
            .GetString();
    }
}
