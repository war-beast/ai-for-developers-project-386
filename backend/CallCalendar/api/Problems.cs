using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api;

/// <summary>Тело ошибки в формате ProblemDetails — общее для всех контроллеров.</summary>
internal static class Problems
{
    public static ObjectResult Result(
        int statusCode,
        string code,
        string title,
        string detail,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        var problem = new ProblemDetails
        {
            Type = "about:blank",
            Title = title,
            Status = statusCode,
            Detail = detail,
        };
        problem.Extensions["code"] = code;
        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" },
        };
    }
}
