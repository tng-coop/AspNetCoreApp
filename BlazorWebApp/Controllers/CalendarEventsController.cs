using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Services;
using BlazorWebApp.Models;

namespace BlazorWebApp.Controllers
{
    [ApiController]
    [Route("api/calendar-events")]
    public class CalendarEventsController : ControllerBase
    {
        private readonly ICalendarEventService _service;
        public CalendarEventsController(ICalendarEventService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<CalendarEventDto>>> List()
        {
            var events = await _service.ListAsync();
            return Ok(events);
        }
    }
}
