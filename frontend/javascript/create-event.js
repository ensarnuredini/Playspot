import { apiPost, isLoggedIn } from './core/api.js';

// ═══════════════════════════════════════════════════════════
//  PlaySpot — Create Event Integration
//  Overrides the inline publishEvent() to actually POST to API
// ═══════════════════════════════════════════════════════════

// Override the inline publishEvent function
window.publishEvent = async function() {
    if (!isLoggedIn()) {
        window.location.hash = 'auth';
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
            window.location.hash = `event-detail?id=${result.data.id}`;
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
        window.location.hash = 'auth';
    }
});

console.log('✅ PlaySpot Create Event module loaded');

// ── Map Picker Logic ──
let mapInstance = null;
let mapMarker = null;
let currentMapLatLng = { lat: 42.0003, lng: 21.4116 }; // Default to Skopje approx

async function reverseGeocodeAndUpdate(lat, lng) {
    try {
        const response = await fetch(`https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json`);
        const data = await response.json();
        
        // Extract a sensible location name
        let address = data.display_name || `${lat.toFixed(4)}, ${lng.toFixed(4)}`;
        if (data.address) {
            address = data.address.amenity || data.address.leisure || data.address.road || data.display_name;
            if (data.address.city && address !== data.address.city) {
                address += `, ${data.address.city}`;
            }
        }
        
        document.getElementById('selected-address').textContent = address;
        document.getElementById('event-address').value = address;
        if (typeof updatePreview === 'function') {
            updatePreview('location', address);
        }
    } catch (e) {
        console.error('Reverse geocoding failed', e);
        const address = `${lat.toFixed(4)}, ${lng.toFixed(4)}`;
        document.getElementById('selected-address').textContent = address;
        document.getElementById('event-address').value = address;
        if (typeof updatePreview === 'function') {
            updatePreview('location', address);
        }
    }
}

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

        // Geolocation API integration
        if ("geolocation" in navigator) {
            navigator.geolocation.getCurrentPosition(async (position) => {
                const lat = position.coords.latitude;
                const lng = position.coords.longitude;
                currentMapLatLng = { lat, lng };
                
                // Update map view and reusable marker
                mapInstance.setView([lat, lng], 15);
                mapMarker.setLatLng(currentMapLatLng);
                
                // Reverse geocode to update UI
                await reverseGeocodeAndUpdate(lat, lng);
            }, (error) => {
                console.warn("Geolocation failed or denied:", error);
            }, {
                enableHighAccuracy: true
            });
        }
    } else {
        setTimeout(() => mapInstance.invalidateSize(), 100);
    }
};

window.closeMapPicker = function() {
    document.getElementById('map-modal').style.display = 'none';
};

window.confirmMapLocation = async function() {
    closeMapPicker();
    
    // Assumes eventData is globally available from the inline HTML script
    if (typeof eventData !== 'undefined') {
        eventData.latitude = currentMapLatLng.lat;
        eventData.longitude = currentMapLatLng.lng;
    }
    
    await reverseGeocodeAndUpdate(currentMapLatLng.lat, currentMapLatLng.lng);
};

// ── State ──
window.currentStep = 1;
window.eventData = {
    sport: '', sportIcon: '', sportBand: '',
    title: '', location: '', date: '', time: '',
    players: 10, skill: '', latitude: 0, longitude: 0
};

// ── Step navigation ──
window.goToStep = function(step) {
    document.getElementById(`step-${window.currentStep}`).classList.remove('active');
    document.querySelectorAll('.step-item').forEach((item, i) => {
        const n = i + 1;
        item.classList.remove('active', 'completed');
        if (n < step)  item.classList.add('completed');
        if (n === step) item.classList.add('active');
    });
    window.currentStep = step;
    document.getElementById(`step-${window.currentStep}`).classList.add('active');
    if (step === 4) window.populateReview();
    window.scrollTo({ top: 0, behavior: 'smooth' });
};

// ── Sport selection ──
window.selectSport = function(el, name, icon, band) {
    document.querySelectorAll('.sport-option').forEach(o => o.classList.remove('selected'));
    el.classList.add('selected');
    window.eventData.sport = name;
    window.eventData.sportIcon = icon;
    window.eventData.sportBand = band;

    document.getElementById('preview-sport-badge').textContent = `${icon} ${name}`;
    const bandEl = document.getElementById('preview-band');
    bandEl.className = `preview-card-band ${band}`;
};

// ── Skill level selection ──
window.selectSkill = function(el, level) {
    document.querySelectorAll('.skill-option').forEach(o => o.classList.remove('selected'));
    el.classList.add('selected');
    window.eventData.skill = level;
    document.getElementById('preview-skill').textContent = level;
};

// ── Player count stepper ──
window.changeCount = function(delta) {
    window.eventData.players = Math.max(2, Math.min(50, window.eventData.players + delta));
    document.getElementById('player-count').textContent = window.eventData.players;
    document.getElementById('count-minus').disabled = window.eventData.players <= 2;
    document.getElementById('count-plus').disabled  = window.eventData.players >= 50;
    document.getElementById('preview-players').textContent = `${window.eventData.players} max players`;
};

// ── Live preview updates ──
window.updatePreview = function(field, value) {
    window.eventData[field] = value;

    if (field === 'title') {
        const el = document.getElementById('preview-title');
        if (value.trim()) {
            el.textContent = value;
            el.classList.remove('empty');
        } else {
            el.textContent = 'Your event title will appear here';
            el.classList.add('empty');
        }
    }

    if (field === 'location') {
        document.getElementById('preview-location').textContent = value || 'Location not set';
    }

    if (field === 'date' || field === 'time') {
        const d = document.getElementById('event-date').value;
        const t = document.getElementById('event-time').value;
        const formatted = d && t
        ? `${new Date(d).toLocaleDateString('en-GB', { weekday:'short', day:'numeric', month:'short' })} · ${t}`
        : d ? new Date(d).toLocaleDateString('en-GB', { weekday:'short', day:'numeric', month:'short' })
        : 'Date & time not set';
        document.getElementById('preview-date').textContent = formatted;
    }
};

// ── Populate review step ──
window.populateReview = function() {
    document.getElementById('review-sport').textContent    = window.eventData.sport    || '—';
    document.getElementById('review-title').textContent    = window.eventData.title    || '—';
    document.getElementById('review-location').textContent = window.eventData.location || '—';
    document.getElementById('review-players').textContent  = window.eventData.players;
    document.getElementById('review-skill').textContent    = window.eventData.skill    || '—';
    const d = document.getElementById('event-date').value;
    const t = document.getElementById('event-time').value;
    document.getElementById('review-datetime').textContent = d && t ? `${d} at ${t}` : '—';
};

// ── Mobile menu ──
window.toggleMenu = function() {
    const links = document.querySelector('.navbar-links');
    links.style.display = links.style.display === 'flex' ? 'none' : 'flex';
};

export function init(params) {
    const today = new Date().toISOString().split('T')[0];
    const dateInput = document.getElementById('event-date');
    if (dateInput) {
        dateInput.min = today;
    }
}
