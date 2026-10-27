using System.Text.RegularExpressions;
using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using CallCalendar.DAL.Entity;
using CallCalendar.DAL.Interface;

namespace CallCalendar.BLL.Service;

public sealed class EventTypeService(IEventTypeRepository repository) : IEventTypeService
{
    private const int MaxIdLength = 64;
    private const int MaxTitleLength = 100;
    private const int MaxDescriptionLength = 1000;
    private const int MinDurationMinutes = 15;
    private const int MaxDurationMinutes = 240;
    private const int DurationStepMinutes = 15;

    private static readonly Regex SlugPattern =
        new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.CultureInvariant);

    public IReadOnlyList<EventType> GetAll() =>
        repository.GetAll()
            .Select(ToModel)
            .ToList();

    public CreateEventTypeResult Create(CreateEventTypeCommand command)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            return CreateEventTypeResult.Invalid(errors);
        }

        var entity = new EventTypeEntity
        {
            Id = command.Id!,
            Title = command.Title!.Trim(),
            Description = command.Description ?? string.Empty,
            DurationMinutes = command.DurationMinutes,
        };

        return repository.TryAdd(entity)
            ? CreateEventTypeResult.Success(ToModel(entity))
            : CreateEventTypeResult.Duplicate();
    }

    private static Dictionary<string, string[]> Validate(CreateEventTypeCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        var id = command.Id ?? string.Empty;
        if (id.Length is 0 or > MaxIdLength || !SlugPattern.IsMatch(id))
        {
            errors["id"] = ["Id — slug из строчных латинских букв, цифр и дефисов длиной 1–64."];
        }

        var title = command.Title?.Trim() ?? string.Empty;
        if (title.Length == 0)
        {
            errors["title"] = ["Название обязательно."];
        }
        else if (title.Length > MaxTitleLength)
        {
            errors["title"] = [$"Название не длиннее {MaxTitleLength} символов."];
        }

        if ((command.Description?.Length ?? 0) > MaxDescriptionLength)
        {
            errors["description"] = [$"Описание не длиннее {MaxDescriptionLength} символов."];
        }

        if (command.DurationMinutes is < MinDurationMinutes or > MaxDurationMinutes
            || command.DurationMinutes % DurationStepMinutes != 0)
        {
            errors["durationMinutes"] =
                [$"Длительность от {MinDurationMinutes} до {MaxDurationMinutes} минут и кратна {DurationStepMinutes}."];
        }

        return errors;
    }

    private static EventType ToModel(EventTypeEntity entity) =>
        new(entity.Id, entity.Title, entity.Description, entity.DurationMinutes);
}
