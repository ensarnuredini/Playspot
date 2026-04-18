// Token management
export function setToken(token) {
    localStorage.setItem('authToken', token);
}

export function getToken() {
    return localStorage.getItem('authToken');
}

export function removeToken() {
    localStorage.removeItem('authToken');
}

export function isAuthenticated() {
    return !!getToken();
}

// User management
export function setUser(user) {
    localStorage.setItem('user', JSON.stringify(user));
}

export function getUser() {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
}

export function removeUser() {
    localStorage.removeItem('user');
}

// Logout
export function logout() {
    removeToken();
    removeUser();
}

// Token validation
export function isTokenExpired(token) {
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        return payload.exp * 1000 < Date.now();
    } catch {
        return true;
    }
}

// Redirect to login if not authenticated
export function requireAuth() {
    if (!isAuthenticated()) {
        window.location.href = '/auth.html';
    }
}
