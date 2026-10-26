using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CallCalendar.Tests;

public class HealthTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task HealthEndpointAnswersOk()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "ok",
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
