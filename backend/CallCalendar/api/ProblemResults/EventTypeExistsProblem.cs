using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api.ProblemResults;

internal sealed class EventTypeExistsProblem : IProblemResultStrategy
{
    public ObjectResult GetProblemResult(
        string? subjectId = null,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        Problems.Result(
            StatusCodes.Status409Conflict,
            ProblemCodes.EventTypeExists,
            "Тип встречи уже существует",
            $"Тип встречи с id «{subjectId}» уже создан.");
}
