using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CallCalendar.BLL.Model;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Interface;
using CallCalendar.DAL.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CallCalendar.Tests;

public class SlotsApiTests
{
    [Fact]
    public async Task SlotsReturnsWindowWithMetadataAndFreeSlots()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync(
            "/api/event-types",
            new CreateEventTypeCommand("intro-call", "Знакомство", "Короткий звонок", 45),
            TestContext.Current.CancellationToken);

        var response = await client.GetAsync(
            "/api/event-types/intro-call/slots",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
        var root = json.RootElement;

        Assert.Equal("Europe/Moscow", root.GetProperty("timeZone").GetString());
        Assert.Equal(30, root.GetProperty("stepMinutes").GetInt32());

        var moscow = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        var tomorrow = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, moscow).DateTime).AddDays(1);

        Assert.Equal(
            tomorrow.ToString("yyyy-MM-dd"),
            root.GetProperty("windowStart").GetString());
        Assert.Equal(
            tomorrow.AddDays(13).ToString("yyyy-MM-dd"),
            root.GetProperty("windowEnd").GetString());

        var days = root.GetProperty("days");
        Assert.Equal(14, days.GetArrayLength());

        var firstSlot = days.EnumerateArray()
            .Select(day => day.GetProperty("slots"))
            .First(slots => slots.GetArrayLength() > 0)
            .EnumerateArray()
            .First();

        Assert.Equal("T09:00:00+03:00", firstSlot.GetProperty("start").GetString()![10..]);
    }

    [Fact]
    public async Task SlotsReturnsNotFoundForUnknownEventType()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/event-types/missing/slots",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal("event_type_not_found", problem.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task SlotsReturnsEmptyWindowForTypeLongerThanWorkday()
    {
        var eventTypes = new InMemoryEventTypeRepository();
        eventTypes.TryAdd(new EventTypeEntity
        {
            Id = "too-long",
            Title = "Слишком длинная встреча",
            Description = string.Empty,
            DurationMinutes = 600,
        });

        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureServices(
                services => services.AddSingleton<IEventTypeRepository>(eventTypes)));
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/event-types/too-long/slots",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
        var days = json.RootElement.GetProperty("days");
        Assert.Equal(14, days.GetArrayLength());
        Assert.All(
            days.EnumerateArray(),
            day => Assert.Equal(0, day.GetProperty("slots").GetArrayLength()));
    }
}
