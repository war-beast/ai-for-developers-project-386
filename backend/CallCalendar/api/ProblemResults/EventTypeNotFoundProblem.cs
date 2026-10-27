using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api.ProblemResults;

internal sealed class EventTypeNotFoundProblem : IProblemResultStrategy
{
    public ObjectResult GetProblemResult(
        string? subjectId = null,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        Problems.Result(
            StatusCodes.Status404NotFound,
            ProblemCodes.EventTypeNotFound,
            "Тип встречи не найден",
            $"Тип встречи с id «{subjectId}» не найден.");
}
