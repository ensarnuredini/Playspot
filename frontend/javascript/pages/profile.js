import { getUser, setUser, apiGet, apiPut } from '../core/api.js';
import { sportEmoji, updateNavbar, showToast } from '../core/ui.js';

document.addEventListener('DOMContentLoaded', () => {
    const user = getUser();
    if (!user) {
        window.location.href = 'auth.html';
        return;
    }

    // Get ID from URL parameter (e.g. ?id=1) or default to logged-in user
    const urlParams = new URLSearchParams(window.location.search);
    const profileId = urlParams.get('id') || user.id;

    const isOwnProfile = (parseInt(profileId) === user.id);

    if (isOwnProfile) {
        document.getElementById('profile-actions').style.display = 'block';
    }

    loadProfile(profileId);
    loadEvents(profileId, 'hosting');
});

async function loadProfile(id) {
    const profile = await apiGet(`/users/${id}`);
    if (!profile) {
        document.getElementById('profile-name').textContent = "User not found";
        return;
    }

    document.getElementById('profile-name').textContent = `${profile.firstName} ${profile.lastName}`.trim() || profile.username;
    
    // Set location
    document.getElementById('profile-location').textContent = profile.city ? `📍 ${profile.city}` : "📍 Unknown Location";

    // Set Image
    const imgEl = document.getElementById('profile-image');
    if (profile.profileImageUrl) {
        imgEl.src = profile.profileImageUrl;
        imgEl.classList.remove('placeholder');
    } else {
        const initial = (profile.username || "U")[0].toUpperCase();
        // Since we don't have an avatar generation service, we'll try to just show a placeholder
        imgEl.style.display = 'none';
        const wrapper = document.querySelector('.profile-image-wrapper');
        const div = document.createElement('div');
        div.className = 'profile-img placeholder';
        div.style.fontSize = '48px';
        div.textContent = initial;
        div.id = 'profile-initial';
        wrapper.appendChild(div);
    }

    // Set Bio
    document.getElementById('profile-bio').textContent = profile.bio || "This user hasn't added a bio yet.";

    // Set Stats
    document.getElementById('stat-created').textContent = profile.eventsCreatedCount || 0;
    document.getElementById('stat-joined').textContent = profile.eventsJoinedCount || 0;

    // Prefill edit form
    document.getElementById('edit-fname').value = profile.firstName || '';
    document.getElementById('edit-lname').value = profile.lastName || '';
    document.getElementById('edit-city').value = profile.city || '';
    document.getElementById('edit-bio').value = profile.bio || '';
    document.getElementById('edit-image').value = profile.profileImageUrl || '';
}

async function loadEvents(id, type) {
    const loader = document.getElementById('events-loader');
    const containerId = type === 'hosting' ? 'hosting-list' : 'joined-list';
    const container = document.getElementById(containerId);
    
    loader.style.display = 'block';
    container.innerHTML = '';

    const endpoint = type === 'hosting' ? `/users/${id}/events` : `/users/${id}/joined-events`;
    const events = await apiGet(endpoint);

    loader.style.display = 'none';

    if (!events || events.length === 0) {
        container.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px; color: var(--color-text-muted);">No events found in this category.</div>`;
        return;
    }

    events.forEach(ev => {
        const d = new Date(ev.date).toLocaleDateString();
        const card = document.createElement('div');
        card.className = 'event-card';
        card.style.cursor = 'pointer';
        card.onclick = () => window.location.href = `event-detail.html?id=${ev.id}`;
        card.innerHTML = `
            <div class="event-card-sport">${sportEmoji(ev.sport)} ${ev.sport}</div>
            <div class="event-card-title">${ev.title}</div>
            <div class="event-card-meta">📅 ${d}</div>
            <div class="event-card-meta">📍 ${ev.location}</div>
            <div style="margin-top: 15px; font-size: 13px; color: var(--color-text-muted);">
                Participants: ${ev.approvedParticipantCount} / ${ev.maxParticipants}
            </div>
        `;
        container.appendChild(card);
    });
}

window.switchTab = function(type) {
    const tabs = document.querySelectorAll('.tab-btn');
    tabs.forEach(t => t.classList.remove('active'));
    event.target.classList.add('active');

    document.getElementById('hosting-list').style.display = type === 'hosting' ? 'grid' : 'none';
    document.getElementById('joined-list').style.display = type === 'joined' ? 'grid' : 'none';

    const urlParams = new URLSearchParams(window.location.search);
    const profileId = urlParams.get('id') || getUser().id;
    
    loadEvents(profileId, type);
}

// Modal Logic
window.openEditModal = function() {
    document.getElementById('edit-modal').classList.add('show');
}

window.closeEditModal = function() {
    document.getElementById('edit-modal').classList.remove('show');
}

window.handleEditProfile = async function(e) {
    e.preventDefault();
    const user = getUser();
    const btn = e.target.querySelector('button[type="submit"]');
    btn.disabled = true;
    btn.innerText = 'Saving...';

    const dto = {
        firstName: document.getElementById('edit-fname').value,
        lastName: document.getElementById('edit-lname').value,
        city: document.getElementById('edit-city').value,
        bio: document.getElementById('edit-bio').value,
        profileImageUrl: document.getElementById('edit-image').value
    };

    const res = await apiPut(`/users/${user.id}`, dto);

    btn.disabled = false;
    btn.innerText = 'Save Changes';

    if (res && res.ok) {
        user.profileImageUrl = dto.profileImageUrl;
        user.username = dto.firstName && dto.lastName ? `${dto.firstName}${dto.lastName}`.toLowerCase() : user.username;
        setUser(user);
        
        closeEditModal();
        loadProfile(user.id);
        if (typeof updateNavbar === 'function') updateNavbar();
        if (typeof showToast === 'function') {
            showToast("Profile updated successfully!");
        }
    } else {
        showToast("Failed to update profile");
    }
}

import { logout } from '../core/api.js';
window.logout = logout;
