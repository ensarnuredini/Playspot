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

    // Preserve local time format without converting to UTC
    const dateTime = `${date}T${time}:00`;

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
        latitude: eventData.latitude || 0,
        longitude: eventData.longitude || 0
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

// ── Map Picker Logic ──
let mapInstance = null;
let mapMarker = null;
let currentMapLatLng = { lat: 42.0003, lng: 21.4116 }; // Default to Skopje approx

window.openMapPicker = function() {
    document.getElementById('map-modal').style.display = 'flex';
    if (!mapInstance) {
        mapInstance = L.map('event-map').setView([currentMapLatLng.lat, currentMapLatLng.lng], 13);
        L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
            attribution: '&copy; OpenStreetMap'
        }).addTo(mapInstance);

        mapMarker = L.marker([currentMapLatLng.lat, currentMapLatLng.lng], { draggable: true }).addTo(mapInstance);
        
        mapInstance.on('click', function(e) {
            currentMapLatLng = e.latlng;
            mapMarker.setLatLng(currentMapLatLng);
        });
        
        mapMarker.on('dragend', function(e) {
            currentMapLatLng = mapMarker.getLatLng();
        });
    } else {
        setTimeout(() => mapInstance.invalidateSize(), 100);
    }
};

window.closeMapPicker = function() {
    document.getElementById('map-modal').style.display = 'none';
};

window.confirmMapLocation = async function() {
    closeMapPicker();
    
    eventData.latitude = currentMapLatLng.lat;
    eventData.longitude = currentMapLatLng.lng;
    
    // Reverse geocoding using Nominatim
    try {
        const response = await fetch(`https://nominatim.openstreetmap.org/reverse?lat=${currentMapLatLng.lat}&lon=${currentMapLatLng.lng}&format=json`);
        const data = await response.json();
        
        // Extract a sensible location name
        let address = data.display_name || `${currentMapLatLng.lat.toFixed(4)}, ${currentMapLatLng.lng.toFixed(4)}`;
        if (data.address) {
            address = data.address.amenity || data.address.leisure || data.address.road || data.display_name;
            if (data.address.city && address !== data.address.city) {
                address += `, ${data.address.city}`;
            }
        }
        
        document.getElementById('selected-address').textContent = address;
        document.getElementById('event-address').value = address;
        updatePreview('location', address);

    } catch (e) {
        console.error('Reverse geocoding failed', e);
        const address = `${currentMapLatLng.lat.toFixed(4)}, ${currentMapLatLng.lng.toFixed(4)}`;
        document.getElementById('selected-address').textContent = address;
        document.getElementById('event-address').value = address;
        updatePreview('location', address);
    }
};
