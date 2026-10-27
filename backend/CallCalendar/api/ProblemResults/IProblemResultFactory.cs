namespace CallCalendar.Api.ProblemResults;

/// <summary>Фабричный метод: выбирает стратегию ответа по коду ошибки.</summary>
public interface IProblemResultFactory
{
    IProblemResultStrategy Create(string code);
}
