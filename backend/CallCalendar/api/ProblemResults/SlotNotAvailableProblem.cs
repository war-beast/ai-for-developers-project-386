using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api.ProblemResults;

internal sealed class SlotNotAvailableProblem : IProblemResultStrategy
{
    public ObjectResult GetProblemResult(
        string? subjectId = null,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        Problems.Result(
            StatusCodes.Status400BadRequest,
            ProblemCodes.SlotNotAvailable,
            "Слот недоступен",
            "Выберите свободный слот из окна записи.");
}
