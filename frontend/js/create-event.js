import { createEvent } from './api.js';
import { getToken, requireAuth, getUser } from './auth.js';

requireAuth();

let currentStep = 1;
let formData = {
    sport: '',
    title: '',
    location: '',
    latitude: 0,
    longitude: 0,
    dateTime: '',
    totalSpots: 0
};

// Select sport
window.selectSport = function(element, sport, icon, bandClass) {
    document.querySelectorAll('.sport-option').forEach(el => el.classList.remove('active'));
    element.classList.add('active');
    formData.sport = sport;
    updateBand(icon, bandClass);
};

// Update band (visual feedback)
function updateBand(icon, bandClass) {
    const band = document.querySelector('.band');
    if (band) {
        band.textContent = icon;
        band.className = `band ${bandClass}`;
    }
}

// Next step
window.nextStep = function() {
    if (validateStep(currentStep)) {
        currentStep++;
        updateStepUI();
    } else {
        alert('Please complete all required fields');
    }
};

// Previous step
window.prevStep = function() {
    if (currentStep > 1) {
        currentStep--;
        updateStepUI();
    }
};

// Validate step
function validateStep(step) {
    switch (step) {
        case 1:
            return formData.sport !== '';
        case 2:
            return formData.title !== '' && formData.location !== '';
        case 3:
            return formData.dateTime !== '' && formData.totalSpots > 0;
        default:
            return true;
    }
}

// Update UI for step
function updateStepUI() {
    document.querySelectorAll('.form-panel').forEach(panel => {
        panel.classList.remove('active');
    });
    document.getElementById(`step-${currentStep}`).classList.add('active');

    document.querySelectorAll('.step-item').forEach(item => {
        item.classList.remove('active');
        if (parseInt(item.dataset.step) === currentStep) {
            item.classList.add('active');
        }
    });
}

// Get location
window.getLocation = function() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(position => {
            formData.latitude = position.coords.latitude;
            formData.longitude = position.coords.longitude;
            alert('Location captured!');
        });
    } else {
        alert('Geolocation not supported');
    }
};

// Collect form data
function collectFormData() {
    const titleInput = document.getElementById('event-title');
    const locationInput = document.getElementById('event-location');
    const dateInput = document.getElementById('event-date');
    const timeInput = document.getElementById('event-time');
    const spotsInput = document.getElementById('event-spots');

    formData.title = titleInput?.value || '';
    formData.location = locationInput?.value || '';
    formData.dateTime = dateInput?.value && timeInput?.value ? `${dateInput.value}T${timeInput.value}` : '';
    formData.totalSpots = parseInt(spotsInput?.value || 0);
}

// Publish event
window.publishEvent = async function() {
    collectFormData();

    if (!formData.sport || !formData.title || !formData.location || !formData.dateTime || formData.totalSpots === 0) {
        alert('Please fill all required fields');
        return;
    }

    try {
        const token = getToken();
        const response = await createEvent(formData, token);

        if (response.id) {
            alert('Event created successfully!');
            window.location.href = 'dashboard.html';
        } else {
            alert('Error creating event: ' + (response.message || 'Unknown error'));
        }
    } catch (error) {
        alert('Error: ' + error.message);
    }
};

// Toggle menu
window.toggleMenu = function() {
    const navLinks = document.querySelector('.navbar-links');
    if (navLinks) {
        navLinks.classList.toggle('active');
    }
};

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    const user = getUser();
    if (user) {
        const avatar = document.querySelector('.navbar-avatar');
        if (avatar) {
            avatar.textContent = user.username.charAt(0).toUpperCase();
        }
    }
});
