import { getAllEvents } from './api.js';
import { requireAuth, getUser, logout } from './auth.js';

requireAuth();

let allEvents = [];
let filteredEvents = [];
let currentSportFilter = 'all';
let currentDateFilter = 'all';
let mapMarkers = [];

// Initialize map using Leaflet
let map;

function initMap() {
    // Create map container if it doesn't exist
    const mapContainer = document.getElementById('map');
    if (!mapContainer) {
        console.error('Map container not found');
        loadEvents();
        return;
    }

    // Initialize Leaflet map
    map = L.map('map').setView([40.7128, -74.0060], 12); // Default to NYC

    // Add OpenStreetMap tiles
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors',
        maxZoom: 19
    }).addTo(map);

    // Get user location
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(position => {
            const userLocation = [position.coords.latitude, position.coords.longitude];
            map.setView(userLocation, 13);
            loadEvents();
        }, () => {
            loadEvents(); // Load events even if geolocation fails
        });
    } else {
        loadEvents();
    }
}

// Load events from API
async function loadEvents() {
    try {
        allEvents = await getAllEvents();
        renderEvents();
    } catch (error) {
        console.error('Error loading events:', error);
    }
}

// Render events on page
function renderEvents() {
    filteredEvents = allEvents.filter(event => {
        const sportMatch = currentSportFilter === 'all' || event.sport.toLowerCase() === currentSportFilter.toLowerCase();
        const dateMatch = currentDateFilter === 'all' || isDateInRange(new Date(event.dateTime), currentDateFilter);
        return sportMatch && dateMatch;
    });

    renderSidebar();
    renderMapMarkers();
}

// Render sidebar events list
function renderSidebar() {
    const sidebar = document.querySelector('.dashboard-sidebar');
    const eventCount = document.getElementById('event-count');
    const eventList = document.querySelector('.sidebar-events');

    if (!eventList) return;

    eventList.innerHTML = '';
    eventCount.textContent = `${filteredEvents.length} events found`;

    filteredEvents.forEach(event => {
        const eventCard = document.createElement('div');
        eventCard.className = 'event-card';
        eventCard.innerHTML = `
            <div class="event-card-header">
                <div class="event-sport-badge">${event.sport}</div>
                <div class="event-spots">${event.spotsLeft}/${event.totalSpots}</div>
            </div>
            <h3 class="event-card-title">${event.title}</h3>
            <p class="event-card-location">📍 ${event.location}</p>
            <p class="event-card-date">🕐 ${formatDate(event.dateTime)}</p>
            <p class="event-card-organiser">Organised by ${event.organiserUsername}</p>
            <div class="event-card-actions">
                <button class="btn btn-sm" onclick="viewEvent(${event.id})">View details</button>
            </div>
        `;
        eventList.appendChild(eventCard);
    });
}

// Render map markers
function renderMapMarkers() {
    if (!map) return;

    // Clear existing markers
    mapMarkers.forEach(marker => map.removeLayer(marker));
    mapMarkers = [];

    filteredEvents.forEach((event, index) => {
        // Create custom icon with sport emoji
        const sportEmoji = getSportEmoji(event.sport);
        const customIcon = L.divIcon({
            html: `<div style="font-size: 24px; text-align: center;">${sportEmoji}</div>`,
            iconSize: [32, 32],
            className: 'custom-marker'
        });

        // Create marker
        const marker = L.marker([event.latitude, event.longitude], {
            icon: customIcon,
            title: event.title
        }).addTo(map);

        // Bind popup
        const popupContent = `
            <div style="padding: 8px;">
                <h4 style="margin: 0 0 4px 0;">${event.title}</h4>
                <p style="margin: 2px 0;"><strong>${event.sport}</strong></p>
                <p style="margin: 2px 0;">📍 ${event.location}</p>
                <p style="margin: 2px 0;">🕐 ${formatDate(event.dateTime)}</p>
                <p style="margin: 2px 0;">Spots: ${event.spotsLeft}/${event.totalSpots}</p>
                <button onclick="viewEvent(${event.id})" style="margin-top: 8px; padding: 4px 12px; cursor: pointer;">View</button>
            </div>
        `;
        marker.bindPopup(popupContent);

        marker.on('click', () => {
            marker.openPopup();
        });

        mapMarkers.push(marker);
    });
}

// Get sport emoji
function getSportEmoji(sport) {
    const emojis = {
        'Football': '⚽',
        'Basketball': '🏀',
        'Tennis': '🎾',
        'Volleyball': '🏐',
        'Running': '🏃',
        'Cycling': '🚴',
        'Swimming': '🏊'
    };
    return emojis[sport] || '🏟️';
}

// Format date
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

// Check if date is in range
function isDateInRange(date, range) {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    switch (range) {
        case 'today':
            return date.toDateString() === today.toDateString();
        case 'tomorrow':
            const tomorrow = new Date(today);
            tomorrow.setDate(tomorrow.getDate() + 1);
            return date.toDateString() === tomorrow.toDateString();
        case 'week':
            const weekEnd = new Date(today);
            weekEnd.setDate(weekEnd.getDate() + 7);
            return date >= today && date <= weekEnd;
        default:
            return true;
    }
}

// Set sport filter
window.setFilter = function(element, sport) {
    document.querySelectorAll('.filter-chip').forEach(el => el.classList.remove('active'));
    element.classList.add('active');
    currentSportFilter = sport;
    renderEvents();
};

// Set date filter
window.setDateFilter = function(date) {
    currentDateFilter = date;
    renderEvents();
};

// View event details
window.viewEvent = function(eventId) {
    window.location.href = `event-detail.html?id=${eventId}`;
};

// Toggle menu
window.toggleMenu = function() {
    const navLinks = document.querySelector('.navbar-links');
    if (navLinks) {
        navLinks.classList.toggle('active');
    }
};

// Logout
window.logout = function() {
    logout();
    window.location.href = 'landing.html';
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    const user = getUser();
    if (user) {
        const avatar = document.querySelector('.navbar-avatar');
        if (avatar) {
            avatar.textContent = user.username.charAt(0).toUpperCase();
        }
    }

    // Initialize Google Map if element exists
    if (document.getElementById('map')) {
        initMap();
    } else {
        loadEvents();
    }
});
