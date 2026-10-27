using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(IBookingService bookingService) : ControllerBase
{
    /// <summary>Предстоящие записи всех типов по возрастанию начала.</summary>
    [HttpGet]
    public IReadOnlyList<Booking> List() => bookingService.GetUpcoming();

    /// <summary>Забронировать свободный слот.</summary>
    [HttpPost]
    public IActionResult Create([FromBody] CreateBookingCommand command)
    {
        var result = bookingService.Create(command);

        if (result.Created is not null)
        {
            return StatusCode(StatusCodes.Status201Created, result.Created);
        }

        if (result.EventTypeNotFound)
        {
            return Problems.Result(
                StatusCodes.Status404NotFound,
                "event_type_not_found",
                "Тип встречи не найден",
                $"Тип встречи с id «{command.EventTypeId}» не найден.");
        }

        if (result.SlotTaken)
        {
            return Problems.Result(
                StatusCodes.Status409Conflict,
                "slot_taken",
                "Время уже занято",
                "Выбранный слот только что заняли. Выберите другое время.");
        }

        if (result.SlotNotAvailable)
        {
            return Problems.Result(
                StatusCodes.Status400BadRequest,
                "slot_not_available",
                "Слот недоступен",
                "Выберите свободный слот из окна записи.");
        }

        return Problems.Result(
            StatusCodes.Status400BadRequest,
            "validation_failed",
            "Поля заполнены неверно",
            "Проверьте выделенные поля.",
            result.Errors);
    }
}
