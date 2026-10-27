using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CallCalendar.Tests;

/// <summary>Тело, которое не удалось разобрать биндингом, — это тоже validation_failed.</summary>
public class InvalidBodyApiTests
{
    [Fact]
    public async Task MalformedJsonReturnsValidationProblem()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var content = new StringContent("{", Encoding.UTF8, "application/json");

        var response = await client.PostAsync(
            "/api/event-types",
            content,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = await ReadProblem(response);
        Assert.Equal("validation_failed", problem.RootElement.GetProperty("code")
            .GetString());
        Assert.True(problem.RootElement.GetProperty("errors")
            .EnumerateObject()
            .Any());
    }

    [Fact]
    public async Task WrongFieldTypeReturnsValidationProblemKeyedByFormField()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var content = new StringContent(
            """{"id":"intro-call","title":123,"description":"","durationMinutes":30}""",
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            "/api/event-types",
            content,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var problem = await ReadProblem(response);
        Assert.Equal("validation_failed", problem.RootElement.GetProperty("code")
            .GetString());

        var errors = problem.RootElement.GetProperty("errors");
        Assert.NotEmpty(errors.GetProperty("title")
            .EnumerateArray());
        Assert.All(errors.EnumerateObject(), error => Assert.DoesNotContain('$', error.Name));
    }

    [Fact]
    public async Task WrongStartTypeReturnsValidationProblemKeyedByStart()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var content = new StringContent(
            """{"eventTypeId":"intro-call","start":"не дата","guestName":"Гость","guestEmail":"guest@example.com"}""",
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            "/api/bookings",
            content,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var problem = await ReadProblem(response);
        Assert.Equal("validation_failed", problem.RootElement.GetProperty("code")
            .GetString());
        Assert.NotEmpty(problem.RootElement.GetProperty("errors")
            .GetProperty("start")
            .EnumerateArray());
    }

    private static async Task<JsonDocument> ReadProblem(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken));
}
