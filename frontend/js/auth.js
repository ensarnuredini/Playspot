// ═══════════════════════════════════════════════════════════
//  PlaySpot — Auth Integration
//  Handles login & register form submission
// ═══════════════════════════════════════════════════════════

// Override the inline handleLogin function
window.handleLogin = async function(e) {
    e.preventDefault();

    const btn = document.getElementById('login-btn');
    const errorEl = document.getElementById('login-error');
    
    btn.classList.add('loading');
    btn.textContent = 'Logging in...';
    errorEl.classList.remove('visible');

    const email = document.getElementById('login-email').value.trim();
    const password = document.getElementById('login-password').value;

    try {
        const result = await apiPost('/auth/login', { email, password });

        if (result && result.ok && result.data) {
            // Store token and user info in localStorage
            setToken(result.data.token);
            setUser({
                id: result.data.userId,
                username: result.data.username
            });

            // Redirect to dashboard
            window.location.href = 'dashboard.html';
        } else {
            errorEl.textContent = 'Incorrect email or password. Please try again.';
            errorEl.classList.add('visible');
            btn.classList.remove('loading');
            btn.textContent = 'Log in →';
        }
    } catch (err) {
        console.error('Login error:', err);
        errorEl.textContent = 'Connection error. Is the server running?';
        errorEl.classList.add('visible');
        btn.classList.remove('loading');
        btn.textContent = 'Log in →';
    }
};

// Override the inline handleRegister function
window.handleRegister = async function(e) {
    e.preventDefault();

    const btn = document.getElementById('register-btn');
    const errorEl = document.getElementById('register-error');

    btn.classList.add('loading');
    btn.textContent = 'Creating account...';
    errorEl.classList.remove('visible');

    const firstName = document.getElementById('reg-firstname').value.trim();
    const lastName = document.getElementById('reg-lastname').value.trim();
    const email = document.getElementById('reg-email').value.trim();
    const password = document.getElementById('reg-password').value;
    const city = document.getElementById('reg-city').value.trim();

    // Generate username from first + last name
    const username = (firstName + lastName).toLowerCase().replace(/\s/g, '');

    try {
        const result = await apiPost('/auth/register', {
            firstName, lastName, username, email, password, city
        });

        if (result && result.ok && result.data) {
            // Store token and user info
            setToken(result.data.token);
            setUser({
                id: result.data.userId,
                username: result.data.username
            });

            // Redirect to dashboard
            window.location.href = 'dashboard.html';
        } else {
            errorEl.textContent = result?.data?.message || 'Email already taken or registration failed.';
            errorEl.classList.add('visible');
            btn.classList.remove('loading');
            btn.textContent = 'Create account →';
        }
    } catch (err) {
        console.error('Register error:', err);
        errorEl.textContent = 'Connection error. Is the server running?';
        errorEl.classList.add('visible');
        btn.classList.remove('loading');
        btn.textContent = 'Create account →';
    }
};

// If user is already logged in and visits auth page, redirect to dashboard
document.addEventListener('DOMContentLoaded', () => {
    if (isLoggedIn() && window.location.pathname.includes('auth.html')) {
        window.location.href = 'dashboard.html';
    }
});

console.log('✅ PlaySpot Auth module loaded');
