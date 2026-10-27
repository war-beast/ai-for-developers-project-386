using CallCalendar.Api.ProblemResults;
using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Api;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(
    IBookingService bookingService,
    IProblemResultFactory problemFactory) : ControllerBase
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
            return problemFactory.Create(ProblemCodes.EventTypeNotFound)
                .GetProblemResult(command.EventTypeId);
        }

        if (result.SlotTaken)
        {
            return problemFactory.Create(ProblemCodes.SlotTaken)
                .GetProblemResult();
        }

        if (result.SlotNotAvailable)
        {
            return problemFactory.Create(ProblemCodes.SlotNotAvailable)
                .GetProblemResult();
        }

        return problemFactory.Create(ProblemCodes.ValidationFailed)
            .GetProblemResult(errors: result.Errors);
    }
}
