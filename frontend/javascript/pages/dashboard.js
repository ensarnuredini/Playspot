import { isLoggedIn, apiGet } from '../core/api.js';
import { sportEmoji, formatEventDate } from '../core/ui.js';

// ═════════════════════════════════════════════════════════
//  DASHBOARD PAGE
// ═════════════════════════════════════════════════════════

let map;
let markers = [];

export async function init(params) {
    await initDashboardPage();
}

async function initDashboardPage() {
    if (!typeof isLoggedIn === 'function' || !isLoggedIn()) {
        window.location.hash = 'auth';
        return;
    }

    // Initialize map
    initMap();

    // Auto-locate user on load
    if ("geolocation" in navigator) {
        navigator.geolocation.getCurrentPosition(pos => {
            const lat = pos.coords.latitude;
            const lng = pos.coords.longitude;
            if (map) {
                map.setView([lat, lng], 13);
                // Optional: add a 'You are here' indicator
                L.circleMarker([lat, lng], {
                    radius: 8,
                    fillColor: "var(--acid)",
                    color: "#000",
                    weight: 1,
                    opacity: 1,
                    fillOpacity: 0.8
                }).addTo(map).bindPopup("You are here");
            }
        }, err => console.warn("Geolocation permission denied or failed."));
    }

    // Load initial events
    await loadDashboardEvents();
}

function initMap() {
    if (!window.L) {
        console.warn('Leaflet not loaded');
        return;
    }

    const mapContainer = document.getElementById('map-container');
    if (!mapContainer) return;

    // Clear placeholder
    mapContainer.innerHTML = '';
    mapContainer.innerHTML = `
    <!-- Map Control -->
    <div class="map-controls">
        <button class="map-control-btn" title="Zoom in" onclick="map.zoomIn()">+</button>
        <button class="map-control-btn" title="Zoom out" onclick="map.zoomOut()">−</button>
    </div>
    <!-- Locate Me -->
    <button class="map-locate-btn" onclick="locateMe()">
        📍 Near me
    </button>
    `;

    // Default to Skopje
    map = L.map('map-container', { zoomControl: false }).setView([41.9981, 21.4254], 13);
    
    L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; OpenStreetMap contributors &copy; CARTO'
    }).addTo(map);
}

async function loadDashboardEvents(filters = {}) {
    let queryArgs = [];
    if (filters.sport && filters.sport !== 'all') queryArgs.push(`sport=${filters.sport}`);
    if (filters.search) queryArgs.push(`search=${filters.search}`);
    if (filters.date && filters.date !== 'all') queryArgs.push(`dateFilter=${filters.date}`);
    
    const query = queryArgs.length > 0 ? '?' + queryArgs.join('&') : '';
    const events = await apiGet(`/event${query}`);
    
    renderSidebar(events);
    renderMapMarkers(events);
}

function renderSidebar(events) {
    const container = document.getElementById('event-list');
    const countEl = document.getElementById('event-count');

    if (container && events && events.length > 0) {
        container.innerHTML = events.map(ev => buildDashCard(ev)).join('');
        if (countEl) countEl.textContent = `${events.length} events found`;
    } else if (container) {
        container.innerHTML = '<div style="text-align:center;padding:40px;opacity:.5;">No events found.</div>';
        if (countEl) countEl.textContent = '0 events found';
    }
}

function buildDashCard(ev) {
    const emoji = typeof sportEmoji === 'function' ? sportEmoji(ev.sport) : '🏅';
    const spotsLeft = (ev.maxParticipants || ev.totalSpots || 10) - (ev.approvedParticipantCount || 0);
    const spotsClass = spotsLeft <= 2 ? 'urgent' : '';

    return `
        <div class="dash-event-card" id="card-${ev.id}" onclick="selectEvent(this, ${ev.id})" ondblclick="window.location.hash='event-detail?id=${ev.id}'">
            <div class="dash-event-top">
                <span class="dash-event-sport">${emoji} ${ev.sport}</span>
            </div>
            <div class="dash-event-title">${ev.title}</div>
            <div class="dash-event-meta">
                <div class="dash-event-meta-row">
                    <span class="dash-event-meta-icon">📅</span>
                    ${formatEventDate(ev.dateTime || ev.date)}
                </div>
                <div class="dash-event-meta-row">
                    <span class="dash-event-meta-icon">📍</span>
                    ${ev.location}
                </div>
                <div class="dash-event-meta-row">
                    <span class="dash-event-meta-icon">👤</span>
                    ${ev.skillLevel || 'All levels'}
                </div>
            </div>
            <div class="dash-event-footer">
                <span class="dash-spots ${spotsClass}"><strong>${spotsLeft}</strong> spot${spotsLeft !== 1 ? 's' : ''} left</span>
            </div>
        </div>`;
}

function renderMapMarkers(events) {
    if (!map) return;

    // clear existing
    markers.forEach(m => map.removeLayer(m));
    markers = [];

    if (!events || events.length === 0) return;

    events.forEach(ev => {
        // Fallback or use DB coordinates if they exist
        const lat = ev.latitude !== 0 ? ev.latitude : 41.9981 + (Math.random() - 0.5) * 0.05;
        const lng = ev.longitude !== 0 ? ev.longitude : 21.4254 + (Math.random() - 0.5) * 0.05;

        const emoji = typeof sportEmoji === 'function' ? sportEmoji(ev.sport) : '🏅';
        
        // Custom div icon
        const icon = L.divIcon({
            className: 'custom-map-icon',
            html: `<div class="map-pin-bubble" id="pin-${ev.id}">${emoji}</div><div class="map-pin-tail" id="pin-tail-${ev.id}"></div>`,
            iconSize: [40, 48],
            iconAnchor: [20, 48]
        });

        const marker = L.marker([lat, lng], { icon }).addTo(map);
        marker.on('click', () => {
            selectPin(ev.id);
        });

        markers.push(marker);
    });

    if (markers.length > 0) {
        const group = new L.featureGroup(markers);
        map.fitBounds(group.getBounds().pad(0.1));
    }
}

// ── Global Handlers for HTML

window.applyFilters = function() {
    const sportEl = document.querySelector('.filter-chip.active');
    let sport = 'all';
    if(sportEl) {
        const text = sportEl.innerText.toLowerCase();
        if(text.includes('football')) sport = 'football';
        else if(text.includes('basketball')) sport = 'basketball';
        else if(text.includes('tennis')) sport = 'tennis';
        else if(text.includes('volleyball')) sport = 'volleyball';
        else if(text.includes('running')) sport = 'running';
        else if(text.includes('cycling')) sport = 'cycling';
    }
    const searchInput = document.querySelector('.sidebar-search-input');
    const search = searchInput ? searchInput.value : '';
    
    const dateSelect = document.querySelector('.filter-select');
    const date = dateSelect ? dateSelect.value : 'all';

    loadDashboardEvents({ sport, search, date });
}

window.setFilter = function(el, sport) {
    document.querySelectorAll('.filter-chip').forEach(c => c.classList.remove('active'));
    el.classList.add('active');
    window.applyFilters();
}

window.setDateFilter = function(value) {
    window.applyFilters();
}

let dashSearchTimeout;
window.handleSearch = function(query) {
    clearTimeout(dashSearchTimeout);
    dashSearchTimeout = setTimeout(() => {
        window.applyFilters();
    }, 300);
}

window.selectEvent = function(el, id) {
    document.querySelectorAll('.dash-event-card').forEach(c => c.classList.remove('selected'));
    el.classList.add('selected');

    // Highlight map pin
    document.querySelectorAll('.map-pin-bubble').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.map-pin-tail').forEach(t => t.classList.remove('active'));
    const pin = document.getElementById(`pin-${id}`);
    if (pin) pin.classList.add('active');
    const tail = document.getElementById(`pin-tail-${id}`);
    if (tail) tail.classList.add('active');
}

window.selectPin = function(id) {
    const cards = document.querySelectorAll('.dash-event-card');
    cards.forEach(c => c.classList.remove('selected'));
    
    const card = document.getElementById(`card-${id}`);
    if (card) {
        card.classList.add('selected');
        card.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    document.querySelectorAll('.map-pin-bubble').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.map-pin-tail').forEach(t => t.classList.remove('active'));
    const pin = document.getElementById(`pin-${id}`);
    if(pin) pin.classList.add('active');
    const tail = document.getElementById(`pin-tail-${id}`);
    if(tail) tail.classList.add('active');
}

window.locateMe = function() {
    if (!navigator.geolocation) return;
    if(map) {
        navigator.geolocation.getCurrentPosition(pos => {
            const lat = pos.coords.latitude;
            const lng = pos.coords.longitude;
            map.flyTo([lat, lng], 14);
            L.circleMarker([lat, lng], { color: '#0066ff', radius: 8 }).addTo(map);
        });
    }
}