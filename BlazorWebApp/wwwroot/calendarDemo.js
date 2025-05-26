window.calendarDemo = {
    init: function (id, events, culture) {
        const el = document.getElementById(id);
        if (!el || typeof FullCalendar === 'undefined') return;

        // Determine the locale: explicit parameter wins, else fall back to blazorCulture helper.
        const locale = culture || (window.blazorCulture && window.blazorCulture.get && window.blazorCulture.get());

        const calendar = new FullCalendar.Calendar(el, {
            initialView: 'dayGridMonth',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay,listMonth'
            },
            events: events,
            locale: locale,
            height: '100%',           // match parent container’s height
            contentHeight: 'auto',    // let content determine scrolling if needed
            expandRows: true,         // grow month‐view rows to fill height
            handleWindowResize: true, // re-render when the window/container resizes
        });
        calendar.render();
    }
};
