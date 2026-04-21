// ═══════════════════════════════════════════════════════════
//  PlaySpot — Create Event Integration
//  Overrides the inline publishEvent() to actually POST to API
// ═══════════════════════════════════════════════════════════

// Override the inline publishEvent function
window.publishEvent = async function() {
    if (!isLoggedIn()) {
        window.location.href = 'auth.html';
        return;
    }

    const btn = document.getElementById('publish-btn');
    const errorEl = document.getElementById('publish-error');

    // Collect form data
    const sport = eventData.sport;
    const title = document.getElementById('event-title').value.trim();
    const description = document.getElementById('event-desc').value.trim();
    const location = document.getElementById('event-address').value.trim();
    const date = document.getElementById('event-date').value;
    const time = document.getElementById('event-time').value;
    const durationVal = document.getElementById('event-duration').value;
    const gender = document.getElementById('event-gender').value;
    const minAge = document.getElementById('age-min').value;
    const maxAge = document.getElementById('age-max').value;
    const approval = document.querySelector('input[name="approval"]:checked')?.value;

    // Validation
    if (!sport || !title || !location || !date || !time) {
        errorEl.textContent = 'Please fill in sport, title, location, date and time.';
        errorEl.classList.add('visible');
        return;
    }

    errorEl.classList.remove('visible');
    btn.classList.add('loading');
    btn.textContent = 'Publishing...';

    // Build ISO datetime
    const dateTime = new Date(`${date}T${time}:00`).toISOString();

    const payload = {
        title,
        sport,
        description,
        location,
        dateTime,
        totalSpots: eventData.players || 10,
        durationMinutes: durationVal && durationVal !== 'custom' ? parseInt(durationVal) : null,
        skillLevel: eventData.skill || 'All levels',
        gender: gender || 'all',
        minAge: minAge ? parseInt(minAge) : null,
        maxAge: maxAge ? parseInt(maxAge) : null,
        requiresApproval: approval === 'approval',
        latitude: 0,
        longitude: 0
    };

    try {
        const result = await apiPost('/event', payload);

        if (result && result.ok && result.data) {
            // Success — redirect to event detail page
            window.location.href = `event-detail.html?id=${result.data.id}`;
        } else {
            errorEl.textContent = 'Failed to create event. Please try again.';
            errorEl.classList.add('visible');
            btn.classList.remove('loading');
            btn.textContent = '🚀 Publish event';
        }
    } catch (err) {
        console.error('Publish error:', err);
        errorEl.textContent = 'Connection error. Is the server running?';
        errorEl.classList.add('visible');
        btn.classList.remove('loading');
        btn.textContent = '🚀 Publish event';
    }
};

// Redirect to auth if not logged in on this page
document.addEventListener('DOMContentLoaded', () => {
    if (!isLoggedIn()) {
        window.location.href = 'auth.html';
    }
});

console.log('✅ PlaySpot Create Event module loaded');
