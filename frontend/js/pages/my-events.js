// ═════════════════════════════════════════════════════════
//  MY EVENTS PAGE
// ═════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', () => {
    initMyEventsPage();
});

async function initMyEventsPage() {
    if (!typeof isLoggedIn === 'function' || !isLoggedIn()) { 
        window.location.href = 'auth.html'; 
        return; 
    }

    const [hosting, joined, past] = await Promise.all([
        apiGet('/event/my/hosting'),
        apiGet('/event/my/joined'),
        apiGet('/event/my/past')
    ]);

    // Update tab counts
    const tabCounts = document.querySelectorAll('.my-tab-count');
    if (tabCounts.length >= 3) {
        tabCounts[0].textContent = hosting ? hosting.length : 0;
        tabCounts[1].textContent = joined ? joined.length : 0;
        tabCounts[2].textContent = past ? past.length : 0;
    }

    // Update stat numbers if they exist
    const stats = document.querySelectorAll('.my-stat-num');
    if (stats.length >= 3) {
        stats[0].textContent = hosting ? hosting.length : 0;
        stats[1].textContent = joined ? joined.length : 0;
        stats[2].textContent = past ? past.length : 0;
    }

    renderMyEventsList('panel-hosting', hosting, true);
    renderMyEventsList('panel-joined', joined, false);
    renderMyEventsList('panel-past', past, false, true);
}

function renderMyEventsList(panelId, events, isHosting, isPast = false) {
    const panel = document.getElementById(panelId);
    if (!panel) return;

    if (!events || events.length === 0) {
        panel.innerHTML = '<div style="text-align:center;padding:40px;opacity:.5;">No events here yet.</div>';
        return;
    }

    panel.innerHTML = events.map(ev => {
        const emoji = typeof sportEmoji === 'function' ? sportEmoji(ev.sport) : '🏅';
        const total = ev.maxParticipants || ev.totalSpots || 10;
        const count = ev.approvedParticipantCount || 0;
        const spotsLeft = total - count;
        const badge = isPast
            ? '<span class="badge badge-muted">Completed</span>'
            : spotsLeft <= 0
                ? '<span class="badge badge-danger">Full</span>'
                : `<span class="badge badge-success">${spotsLeft} spots open</span>`;

        const actions = isHosting
            ? `<div class="manage-menu" id="menu-${ev.id}">
                 <button class="btn btn-ghost btn-sm" onclick="toggleManageMenu('menu-${ev.id}', event)">Manage ▾</button>
                 <div class="manage-dropdown">
                   <button class="manage-item" onclick="goToEvent(${ev.id})">View event</button>
                   <button class="manage-item danger" onclick="cancelEvent(${ev.id})">Cancel event</button>
                 </div>
               </div>`
            : isPast
                ? `<button class="btn btn-ghost btn-sm" onclick="rateEvent(${ev.id})">Rate ⭐</button>`
                : `<button class="btn btn-ghost btn-sm" onclick="leaveEvent(${ev.id})">Leave</button>`;

        return `
            <div class="my-event-row" onclick="goToEvent(${ev.id})" ${isPast ? 'style="opacity:.75;"' : ''}>
                <div class="my-event-strip strip-${(ev.sport || 'default').toLowerCase()}"></div>
                <div class="my-event-body">
                    <span class="my-event-icon">${emoji}</span>
                    <div class="my-event-info">
                        <div class="my-event-title">${ev.title}</div>
                        <div class="my-event-meta">
                            <span class="my-event-meta-item">📅 ${formatEventDate(ev.dateTime || ev.date)}</span>
                            <span class="my-event-meta-item">📍 ${ev.location}</span>
                            <span class="my-event-meta-item">👥 ${count} / ${total}</span>
                            ${badge}
                        </div>
                    </div>
                </div>
                <div class="my-event-actions" onclick="event.stopPropagation()">
                    ${actions}
                </div>
            </div>`;
    }).join('');
}

// Global actions
window.goToEvent = function(id) {
    window.location.href = `event-detail.html?id=${id}`;
};

window.cancelEvent = async function(id) {
    if (confirm('Are you sure you want to cancel this event?')) {
        const result = await apiDelete(`/event/${id}`);
        if (result && result.ok) {
            alert('Event cancelled.');
            location.reload();
        }
    }
};

window.leaveEvent = async function(id) {
    if (confirm('Leave this event?')) {
        const result = await apiDelete(`/joinrequest/${id}`);
        if (result && result.ok) {
            alert('You have left the event.');
            location.reload();
        }
    }
};

window.rateEvent = async function(id) {
    const score = prompt('Rate this event (1-5):');
    if (!score) return;
    const result = await apiPost(`/event/${id}/rate`, { score: parseInt(score) });
    if (result && result.ok) alert('Thanks for your rating!');
    else alert('Already rated or invalid score.');
};

window.toggleManageMenu = function(id, e) {
    e.stopPropagation();
    document.querySelectorAll('.manage-dropdown').forEach(d => d.style.display = 'none');
    const menu = document.getElementById(id);
    if(menu) {
        const drop = menu.querySelector('.manage-dropdown');
        if(drop) drop.style.display = 'block';
    }
};

document.addEventListener('click', () => {
    document.querySelectorAll('.manage-dropdown').forEach(d => d.style.display = 'none');
});

window.switchTab = function(tab, btn) {
    document.querySelectorAll('.my-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.my-tab-panel').forEach(p => p.classList.remove('active'));
    btn.classList.add('active');
    document.getElementById(`panel-${tab}`).classList.add('active');
}
