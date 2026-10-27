using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api;

[ApiController]
[Route("api/event-types")]
public sealed class EventTypesController(
    IEventTypeService eventTypeService,
    ISlotService slotService) : ControllerBase
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
            return Problems.Result(
                StatusCodes.Status409Conflict,
                "event_type_exists",
                "Тип встречи уже существует",
                $"Тип встречи с id «{command.Id}» уже создан.");
        }

        return Problems.Result(
            StatusCodes.Status400BadRequest,
            "validation_failed",
            "Поля заполнены неверно",
            "Проверьте выделенные поля.",
            result.Errors);
    }

    /// <summary>Свободные слоты типа встречи на всё окно записи.</summary>
    [HttpGet("{eventTypeId}/slots")]
    public IActionResult Slots(string eventTypeId)
    {
        var result = slotService.GetWindow(eventTypeId);

        return result.Window is not null
            ? Ok(result.Window)
            : Problems.Result(
                StatusCodes.Status404NotFound,
                "event_type_not_found",
                "Тип встречи не найден",
                $"Тип встречи с id «{eventTypeId}» не найден.");
    }
}
