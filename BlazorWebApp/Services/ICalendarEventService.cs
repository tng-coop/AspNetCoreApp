using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorWebApp.Models;

namespace BlazorWebApp.Services
{
    public interface ICalendarEventService
    {
        Task<List<CalendarEventDto>> ListAsync();
        Task<List<CalendarEventDto>> ListByPublicationAsync(Guid publicationId);

        // Editing support
        Task<List<CalendarEventEditDto>> ListForPublicationAsync(Guid publicationId);
        Task<CalendarEventEditDto> CreateAsync(Guid publicationId, CalendarEventWriteDto dto);
        Task<CalendarEventEditDto> UpdateAsync(Guid id, CalendarEventWriteDto dto);
        Task DeleteAsync(Guid id);
    }
}
