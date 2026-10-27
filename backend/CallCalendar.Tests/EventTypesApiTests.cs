using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CallCalendar.BLL.Model;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CallCalendar.Tests;

public class EventTypesApiTests
{
    [Fact]
    public async Task CreateReturnsCreatedEventTypeAndListContainsIt()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/event-types",
            new CreateEventTypeCommand("intro-call", "Знакомство", "Короткий звонок", 30),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<EventType>(
            TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.Equal("intro-call", created.Id);
        Assert.Equal("Знакомство", created.Title);
        Assert.Equal("Короткий звонок", created.Description);
        Assert.Equal(30, created.DurationMinutes);

        var list = await client.GetFromJsonAsync<EventType[]>(
            "/api/event-types",
            TestContext.Current.CancellationToken);
        Assert.NotNull(list);
        var listed = Assert.Single(list);
        Assert.Equal("intro-call", listed.Id);
    }

    [Fact]
    public async Task CreateRejectsInvalidFieldsWithProblemDetails()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/event-types",
            new CreateEventTypeCommand("Bad Slug", "   ", "", 20),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal("validation_failed", problem.RootElement.GetProperty("code")
            .GetString());

        var errors = problem.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("id", out _));
        Assert.True(errors.TryGetProperty("title", out _));
        Assert.True(errors.TryGetProperty("durationMinutes", out _));
    }

    [Fact]
    public async Task CreateRejectsDuplicateSlugWithConflict()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var command = new CreateEventTypeCommand("intro-call", "Знакомство", "", 30);

        await client.PostAsJsonAsync(
            "/api/event-types",
            command,
            TestContext.Current.CancellationToken);

        var response = await client.PostAsJsonAsync(
            "/api/event-types",
            command,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
        Assert.Equal("event_type_exists", problem.RootElement.GetProperty("code")
            .GetString());
    }
}
