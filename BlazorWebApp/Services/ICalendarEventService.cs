using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public interface ICalendarEventService
    {
        Task<List<CalendarEventDto>> ListAsync();
    }
}
