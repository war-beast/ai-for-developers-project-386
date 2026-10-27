using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api.ProblemResults;

internal sealed class ValidationFailedProblem : IProblemResultStrategy
{
    public ObjectResult GetProblemResult(
        string? subjectId = null,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        Problems.Result(
            StatusCodes.Status400BadRequest,
            ProblemCodes.ValidationFailed,
            "Поля заполнены неверно",
            "Проверьте выделенные поля.",
            errors);
}
