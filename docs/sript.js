document.addEventListener('DOMContentLoaded', function () {
    const events = [];

    const eventList = document.getElementById('events');
    const createEventForm = document.getElementById('create-event');
    const registrationForm = document.getElementById('registration-form');
    const registerEventForm = document.getElementById('register-event');

    console.log('eventList:', eventList);
    console.log('createEventForm:', createEventForm);
    console.log('registrationForm:', registrationForm);
    console.log('registerEventForm:', registerEventForm);

    if (!eventList || !createEventForm || !registrationForm || !registerEventForm) {
        console.error('One or more elements are not found in the DOM.');
        return;
    }

    createEventForm.addEventListener('submit', function (e) {
        e.preventDefault();

        const eventName = document.getElementById('event-name').value;
        const eventDate = document.getElementById('event-date').value;

        const event = {
            name: eventName,
            date: eventDate,
            participants: []
        };

        events.push(event);
        renderEvents();
        createEventForm.reset();
    });

    registerEventForm.addEventListener('submit', function (e) {
        e.preventDefault();

        const participantName = document.getElementById('participant-name').value;
        const participantEmail = document.getElementById('participant-email').value;
        const eventIndex = registerEventForm.dataset.eventIndex;

        const participant = {
            name: participantName,
            email: participantEmail
        };

        events[eventIndex].participants.push(participant);
        alert('登録が完了しました!');
        registrationForm.style.display = 'none';
        registerEventForm.reset();
    });

    function renderEvents() {
        eventList.innerHTML = '';
        events.forEach((event, index) => {
            const li = document.createElement('li');
            li.innerHTML = `
                <strong>${event.name}</strong><br>
                日付: ${event.date}<br>
                <button onclick="registerForEvent(${index})">登録する</button>
            `;
            eventList.appendChild(li);
        });
    }

    window.registerForEvent = function (eventIndex) {
        registrationForm.style.display = 'block';
        registerEventForm.dataset.eventIndex = eventIndex;
    }
});