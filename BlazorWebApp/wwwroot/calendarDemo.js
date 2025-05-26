window.calendarDemo = {
    init: function (id, events) {
        const el = document.getElementById(id);
        if (!el || typeof FullCalendar === 'undefined') return;
        const calendar = new FullCalendar.Calendar(el, {
            initialView: 'dayGridMonth',
            events: events,
            height: '100%',           // match parent container’s height :contentReference[oaicite:1]{index=1}
            contentHeight: 'auto',    // let content determine scrolling if needed :contentReference[oaicite:2]{index=2}
            expandRows: true,         // grow month‐view rows to fill height :contentReference[oaicite:3]{index=3}
            handleWindowResize: true, // re-render when the window/container resizes :contentReference[oaicite:4]{index=4}
        });
        calendar.render();
    }
};
