using CallCalendar.Api.ProblemResults;
using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api;

[ApiController]
[Route("api/event-types")]
public sealed class EventTypesController(
    IEventTypeService eventTypeService,
    ISlotEngine slotEngine,
    IProblemResultFactory problemFactory) : ControllerBase
{
    /// <summary>Список типов встреч в порядке создания.</summary>
    [HttpGet]
    public IReadOnlyList<EventType> List() => eventTypeService.GetAll();

    [HttpPost]
    public IActionResult Create([FromBody] CreateEventTypeCommand command)
    {
        var result = eventTypeService.Create(command);

        if (result.Created is not null)
        {
            return StatusCode(StatusCodes.Status201Created, result.Created);
        }

        if (result.SlugTaken)
        {
            return problemFactory.Create(ProblemCodes.EventTypeExists)
                .GetProblemResult(command.Id);
        }

        return problemFactory.Create(ProblemCodes.ValidationFailed)
            .GetProblemResult(errors: result.Errors);
    }

    /// <summary>Свободные слоты типа встречи на всё окно записи.</summary>
    [HttpGet("{eventTypeId}/slots")]
    public IActionResult Slots(string eventTypeId)
    {
        var result = slotEngine.GetWindow(eventTypeId);

        return result.Window is not null
            ? Ok(result.Window)
            : problemFactory.Create(ProblemCodes.EventTypeNotFound)
                .GetProblemResult(eventTypeId);
    }
}
