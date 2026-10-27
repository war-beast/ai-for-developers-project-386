using CallCalendar.Api.ProblemResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

    /// <summary>Ошибки биндинга тела: те же поля, что у валидации в BLL.</summary>
    public static ObjectResult ValidationFailed(ModelStateDictionary modelState) =>
        Result(
            StatusCodes.Status400BadRequest,
            ProblemCodes.ValidationFailed,
            "Поля заполнены неверно",
            "Проверьте выделенные поля.",
            modelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .GroupBy(entry => FieldName(entry.Key))
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .SelectMany(entry => entry.Value!.Errors)
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "Некорректное значение."
                            : error.ErrorMessage)
                        .ToArray()));

    /// <summary>«$.guestEmail» → «guestEmail»: ключи errors совпадают с полями формы.</summary>
    private static string FieldName(string key)
    {
        var name = key.TrimStart('$', '.').Split('.').Last();

        return name.Length == 0 ? "body" : char.ToLowerInvariant(name[0]) + name[1..];
    }
}
