const API_BASE = 'http://localhost:5258/api';

// ── Token & User Storage (localStorage) ──────────────────

function getToken() {
    return localStorage.getItem('playspot_token');
}

function setToken(token) {
    localStorage.setItem('playspot_token', token);
}

function getUser() {
    const raw = localStorage.getItem('playspot_user');
    return raw ? JSON.parse(raw) : null;
}

function setUser(user) {
    localStorage.setItem('playspot_user', JSON.stringify(user));
}

function isLoggedIn() {
    return !!getToken();
}

function logout() {
    localStorage.removeItem('playspot_token');
    localStorage.removeItem('playspot_user');
    window.location.hash = 'auth';
}

// ── Fetch Helpers ────────────────────────────────────────

async function apiGet(endpoint) {
    const headers = { 'Content-Type': 'application/json' };
    const token = getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    try {
        const res = await fetch(`${API_BASE}${endpoint}`, { headers });
        if (res.status === 401) { logout(); return null; }
        if (!res.ok) return null;
        const text = await res.text();
        return text ? JSON.parse(text) : null;
    } catch (e) {
        console.warn('API GET failed:', endpoint, e.message);
        return null;
    }
}

async function apiPost(endpoint, body = {}) {
    const headers = { 'Content-Type': 'application/json' };
    const token = getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: 'POST', headers, body: JSON.stringify(body)
        });
        if (res.status === 401) { logout(); return null; }
        const text = await res.text();
        return { ok: res.ok, status: res.status, data: text ? JSON.parse(text) : null };
    } catch (e) {
        console.warn('API POST failed:', endpoint, e.message);
        return null;
    }
}

async function apiPut(endpoint, body = {}) {
    const headers = { 'Content-Type': 'application/json' };
    const token = getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: 'PUT', headers, body: JSON.stringify(body)
        });
        if (res.status === 401) { logout(); return null; }
        const text = await res.text();
        return { ok: res.ok, status: res.status, data: text ? JSON.parse(text) : null };
    } catch (e) {
        console.warn('API PUT failed:', endpoint, e.message);
        return null;
    }
}

async function apiPatch(endpoint, body = {}) {
    const headers = { 'Content-Type': 'application/json' };
    const token = getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: 'PATCH', headers, body: JSON.stringify(body)
        });
        if (res.status === 401) { logout(); return null; }
        const text = await res.text();
        return { ok: res.ok, status: res.status, data: text ? JSON.parse(text) : null };
    } catch (e) {
        console.warn('API PATCH failed:', endpoint, e.message);
        return null;
    }
}

async function apiDelete(endpoint) {
    const headers = { 'Content-Type': 'application/json' };
    const token = getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    try {
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: 'DELETE', headers
        });
        if (res.status === 401) { logout(); return null; }
        return { ok: res.ok, status: res.status };
    } catch (e) {
        console.warn('API DELETE failed:', endpoint, e.message);
        return null;
    }
}

// ── Notifications ───────────────────────────────────────

async function getNotifications() {
    return await apiGet('/notifications');
}

async function markNotificationRead(id) {
    return await apiPatch(`/notifications/${id}/read`);
}

export { getToken, setToken, getUser, setUser, isLoggedIn, logout, apiGet, apiPost, apiPut, apiPatch, apiDelete, getNotifications, markNotificationRead };