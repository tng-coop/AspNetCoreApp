window.calendarDemo = {
    init: function(id, events) {
        const el = document.getElementById(id);
        if (!el || typeof FullCalendar === 'undefined') return;
        const calendar = new FullCalendar.Calendar(el, {
            initialView: 'dayGridMonth',
            events: events
        });
        calendar.render();
    }
};
