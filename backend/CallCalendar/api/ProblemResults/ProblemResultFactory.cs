namespace CallCalendar.Api.ProblemResults;

internal sealed class ProblemResultFactory : IProblemResultFactory
{
    public IProblemResultStrategy Create(string code) =>
        code switch
        {
            ProblemCodes.EventTypeNotFound => new EventTypeNotFoundProblem(),
            ProblemCodes.EventTypeExists => new EventTypeExistsProblem(),
            ProblemCodes.SlotTaken => new SlotTakenProblem(),
            ProblemCodes.SlotNotAvailable => new SlotNotAvailableProblem(),
            ProblemCodes.ValidationFailed => new ValidationFailedProblem(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "Неизвестный код ошибки."),
        };
}
