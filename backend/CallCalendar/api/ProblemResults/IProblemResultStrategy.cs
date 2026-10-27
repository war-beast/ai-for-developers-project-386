using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api.ProblemResults;

/// <summary>Стратегия ответа об ошибке в формате ProblemDetails.</summary>
public interface IProblemResultStrategy
{
    ObjectResult GetProblemResult(
        string? subjectId = null,
        IReadOnlyDictionary<string, string[]>? errors = null);
}
