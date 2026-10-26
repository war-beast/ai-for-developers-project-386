using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using Microsoft.AspNetCore.Mvc;

namespace CallCalendar.Controllers;

[ApiController]
[Route("api/calendar")]
public class CalendarController(ICalendarService calendarService) : ControllerBase
{
    [HttpGet("slots")]
    public ActionResult<IReadOnlyList<CallSlot>> GetSlots() =>
        Ok(calendarService.GetSlots());
}
