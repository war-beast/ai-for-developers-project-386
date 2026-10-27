using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api.ProblemResults;

internal sealed class SlotTakenProblem : IProblemResultStrategy
{
    public ObjectResult GetProblemResult(
        string? subjectId = null,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        Problems.Result(
            StatusCodes.Status409Conflict,
            ProblemCodes.SlotTaken,
            "Время уже занято",
            "Выбранный слот только что заняли. Выберите другое время.");
}
