const API_URL = 'http://localhost:5258/api';

// Authentication
export async function register(username, email, password) {
    const response = await fetch(`${API_URL}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, email, password })
    });
    return response.json();
}

export async function login(email, password) {
    const response = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
    });
    return response.json();
}

// Events
export async function getAllEvents(sport, location) {
    let url = `${API_URL}/event`;
    const params = new URLSearchParams();
    if (sport) params.append('sport', sport);
    if (location) params.append('location', location);
    if (params.toString()) url += `?${params.toString()}`;

    const response = await fetch(url);
    return response.json();
}

export async function getEventById(id) {
    const response = await fetch(`${API_URL}/event/${id}`);
    return response.json();
}

export async function createEvent(eventData, token) {
    const response = await fetch(`${API_URL}/event`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(eventData)
    });
    return response.json();
}

export async function deleteEvent(id, token) {
    const response = await fetch(`${API_URL}/event/${id}`, {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return response.json();
}

// Join Requests
export async function requestJoinEvent(eventId, token) {
    const response = await fetch(`${API_URL}/joinrequest/${eventId}`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return response.json();
}

export async function getJoinRequests(eventId, token) {
    const response = await fetch(`${API_URL}/joinrequest/event/${eventId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return response.json();
}

export async function updateJoinRequestStatus(requestId, status, token) {
    const response = await fetch(`${API_URL}/joinrequest/${requestId}/status`, {
        method: 'PATCH',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(status)
    });
    return response.json();
}
