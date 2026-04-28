import { apiGet } from '../core/api.js';
import { sportEmoji, formatEventDate } from '../core/ui.js';

// ═════════════════════════════════════════════════════════
//  EXPLORE PAGE
// ═════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', () => {
    initExplorePage();
});

async function initExplorePage() {
    const params = new URLSearchParams(window.location.search);
    const filters = {
        sport: params.get('sport') || '',
        search: params.get('search') || '',
        skillLevel: params.get('skill') || '',
        dateFilter: params.get('date') || '',
        sortBy: params.get('sort') || 'date'
    };

    // Pre-check sport param if provided
    if (filters.sport) {
        const cb = document.querySelector(`input[value="${filters.sport.toLowerCase()}"]`);
        if (cb) cb.checked = true;
        const mainSearch = document.getElementById('main-search');
        if (mainSearch) mainSearch.value = filters.sport.charAt(0).toUpperCase() + filters.sport.slice(1);
    }

    await loadExploreEvents(filters);
}

async function loadExploreEvents(filters = {}) {
    const queryParts = [];
    if (filters.sport) queryParts.push(`sport=${filters.sport}`);
    if (filters.search) queryParts.push(`search=${filters.search}`);
    if (filters.skillLevel) queryParts.push(`skillLevel=${filters.skillLevel}`);
    if (filters.dateFilter) queryParts.push(`dateFilter=${filters.dateFilter}`);
    if (filters.sortBy) queryParts.push(`sortBy=${filters.sortBy}`);
    
    // Add dynamically checked filters
    const checkboxes = document.querySelectorAll('.explore-filters input[type="checkbox"]:checked');
    if (checkboxes.length > 0) {
        // Find selected sports
        const sports = Array.from(checkboxes).filter(cb => ['football', 'basketball', 'tennis', 'running', 'volleyball', 'cycling', 'swimming'].includes(cb.value)).map(c => c.value);
        if (sports.length > 0 && !filters.sport) queryParts.push(`sport=${sports.join(',')}`);

        // Find skill
        const skills = Array.from(checkboxes).filter(cb => ['all', 'beginner', 'intermediate', 'advanced'].includes(cb.value)).map(c => c.value);
        if (skills.length > 0 && !filters.skillLevel) queryParts.push(`skillLevel=${skills.join(',')}`);
    }

    const query = queryParts.length ? '?' + queryParts.join('&') : '';
    const events = await apiGet(`/event${query}`);

    const gridContainer = document.getElementById('events-grid');
    const listContainer = document.getElementById('events-list');
    const countEl = document.querySelector('.results-count');

    if (events && events.length > 0) {
        // We do sorting dynamically here if backend doesn't support sortBy fully yet
        if (gridContainer) gridContainer.innerHTML = events.map(ev => buildExploreCard(ev)).join('');
        if (listContainer) listContainer.innerHTML = events.map(ev => buildExploreListItem(ev)).join('');
        if (countEl) countEl.innerHTML = `${events.length} <span>events</span>`;
    } else {
        const emptyMsg = '<div style="text-align:center;padding:60px 20px;opacity:.5;grid-column:1/-1;">No events found. Try different filters or create one!</div>';
        if (gridContainer) gridContainer.innerHTML = emptyMsg;
        if (listContainer) listContainer.innerHTML = emptyMsg;
        if (countEl) countEl.innerHTML = `0 <span>events</span>`;
    }
}

function buildExploreCard(ev) {
    const emoji = typeof sportEmoji === 'function' ? sportEmoji(ev.sport) : '🏅';
    const spotsLeft = (ev.maxParticipants || ev.totalSpots || 10) - (ev.approvedParticipantCount || 0);
    const skillClass = (ev.skillLevel || 'all').toLowerCase().replace(/\s+/g, '');
    const spotsHtml = spotsLeft <= 0
        ? '<span class="dash-spots urgent"><strong>Full</strong></span>'
        : spotsLeft <= 2
            ? `<span class="dash-spots urgent"><strong>${spotsLeft}</strong> spot${spotsLeft > 1 ? 's' : ''} left</span>`
            : `<span class="dash-spots"><strong>${spotsLeft}</strong> spots left</span>`;

    return `
        <a href="event-detail.html?id=${ev.id}" class="explore-event-card">
            <div class="explore-event-card-band band-${(ev.sport || 'default').toLowerCase()}"></div>
            <div class="explore-event-card-body">
                <div class="explore-event-card-top">
                    <span class="explore-event-card-sport">${emoji} ${ev.sport}</span>
                </div>
                <div class="explore-event-card-title">${ev.title}</div>
                <div class="explore-event-card-meta">
                    <div class="explore-event-card-meta-row">
                        <span class="explore-event-card-meta-icon">📅</span>
                        ${formatEventDate(ev.dateTime || ev.date)}
                    </div>
                    <div class="explore-event-card-meta-row">
                        <span class="explore-event-card-meta-icon">📍</span>
                        ${ev.location}
                    </div>
                </div>
                <div class="explore-event-card-footer">
                    <span class="skill-pill skill-${skillClass}">${ev.skillLevel || 'All levels'}</span>
                    ${spotsHtml}
                </div>
            </div>
        </a>`;
}

function buildExploreListItem(ev) {
    const emoji = typeof sportEmoji === 'function' ? sportEmoji(ev.sport) : '🏅';
    const spotsLeft = (ev.maxParticipants || ev.totalSpots || 10) - (ev.approvedParticipantCount || 0);
    const skillClass = (ev.skillLevel || 'all').toLowerCase().replace(/\s+/g, '');
    const spotsHtml = spotsLeft <= 2
        ? `<span class="dash-spots urgent"><strong>${spotsLeft}</strong> spot${spotsLeft > 1 ? 's' : ''} left</span>`
        : `<span class="dash-spots"><strong>${spotsLeft}</strong> spots left</span>`;

    return `
        <a href="event-detail.html?id=${ev.id}" class="explore-event-list-item">
            <span class="list-item-sport-icon">${emoji}</span>
            <div class="list-item-body">
                <div class="list-item-title">${ev.title}</div>
                <div class="list-item-meta">${formatEventDate(ev.dateTime || ev.date)} · ${ev.location}</div>
            </div>
            <div class="list-item-right">
                <span class="skill-pill skill-${skillClass}">${ev.skillLevel || 'All levels'}</span>
                ${spotsHtml}
            </div>
        </a>`;
}

// ── View toggle: grid / list
window.setView = function(mode, btn) {
    document.querySelectorAll('.view-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    const grid = document.getElementById('events-grid');
    const list = document.getElementById('events-list');
    if (mode === 'grid') {
        grid.classList.add('active');
        list.classList.remove('active');
    } else {
        list.classList.add('active');
        grid.classList.remove('active');
    }
}

let searchTimeout;
window.handleSearch = function(query) {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        applyFilters();
    }, 300);
}

window.runSearch = function() {
    applyFilters();
}

window.quickSearch = function(sport) {
    const searchInput = document.getElementById('main-search');
    if(searchInput) searchInput.value = sport;
    applyFilters();
}

window.applyFilters = function() {
    const search = document.getElementById('main-search')?.value || '';
    const sortBy = document.querySelector('.results-sort')?.value || 'date';
    loadExploreEvents({ search: search, sortBy: sortBy });
}

window.resetFilters = function() {
    document.querySelectorAll('.explore-filters input[type="checkbox"]').forEach(cb => cb.checked = false);
    const distanceSlider = document.getElementById('distance-slider');
    if (distanceSlider) distanceSlider.value = 10;
    const distanceLabel = document.getElementById('distance-label');
    if (distanceLabel) distanceLabel.textContent = '10 km';
    const mainSearch = document.getElementById('main-search');
    if (mainSearch) mainSearch.value = '';
    applyFilters();
}

window.updateDistance = function(val) {
    const distanceLabel = document.getElementById('distance-label');
    if(distanceLabel) distanceLabel.textContent = val + ' km';
    applyFilters();
}

window.sortResults = function(value) {
    applyFilters();
}
