// ── Navbar Auth State ────────────────────────────────────

function updateNavbar() {
    const user = typeof getUser === 'function' ? getUser() : null;
    const actions = document.querySelector('.navbar-actions');
    if (!actions) return;

    if (user) {
        const initial = (user.username || 'U')[0].toUpperCase();
        actions.innerHTML = `
            <a href="#" class="navbar-bell">🔔<span class="navbar-bell-dot"></span></a>
            <a href="create-event.html" class="btn btn-acid btn-sm">+ New event</a>
            <div class="navbar-avatar" onclick="logout()" title="Log out (${user.username})">${initial}</div>
        `;
    } else {
        actions.innerHTML = `
            <a href="auth.html" class="btn btn-ghost">Log in</a>
            <a href="auth.html?tab=register" class="btn btn-primary">Sign up free</a>
        `;
    }
}

// ── Sport Emoji Helper ──────────────────────────────────

function sportEmoji(sport) {
    const map = {
        football: '⚽', basketball: '🏀', tennis: '🎾',
        volleyball: '🏐', running: '🏃', swimming: '🏊',
        cycling: '🚴', tabletennis: '🏓', badminton: '🏸',
        hiking: '🥾', yoga: '🧘', boxing: '🥊'
    };
    return map[(sport || '').toLowerCase()] || '🏅';
}

// ── Date Formatting Helper ──────────────────────────────

function formatEventDate(dateStr) {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    const now = new Date();
    const tomorrow = new Date(now);
    tomorrow.setDate(tomorrow.getDate() + 1);

    const timeStr = d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit', hour12: true });
    
    if (d.toDateString() === now.toDateString()) return `Today · ${timeStr}`;
    if (d.toDateString() === tomorrow.toDateString()) return `Tomorrow · ${timeStr}`;
    
    const dayName = d.toLocaleDateString('en-US', { weekday: 'long' });
    return `${dayName} · ${timeStr}`;
}

// Ensure navbar is updated on load for any page
document.addEventListener('DOMContentLoaded', () => {
    updateNavbar();
});
