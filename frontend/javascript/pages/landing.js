import { apiGet } from '../core/api.js';

// Load real stats from API
export async function init(params) {
    try {
        const events = await apiGet('/event');
        if (events) {
            document.getElementById('stat-events').textContent = events.length;
        }
    } catch(e) { /* API not available, keep defaults */ }
}
