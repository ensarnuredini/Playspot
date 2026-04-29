import { getUser, setUser, apiGet, isLoggedIn, getNotifications, getToken, markNotificationRead } from './api.js';

// ── Navbar Auth State ────────────────────────────────────

async function updateNavbar() {
  let user = typeof getUser === "function" ? getUser() : null;
  const actions = document.querySelector(".navbar-actions");
  if (!actions) return;

  // Render initial state from localStorage quickly
  renderNavbarAuth(user, actions);

  // Silently refresh user profile to catch any profile image changes
  if (user && user.id) {
    try {
      const freshProfile = await apiGet(`/users/${user.id}`);
      if (freshProfile) {
          user.profileImageUrl = freshProfile.profileImageUrl;
          user.username = freshProfile.username || user.username;
          if (typeof setUser === 'function') setUser(user);
          // Re-render auth block with fresh image
          renderNavbarAuth(user, actions);
      }
    } catch (e) {
      // Ignore errors on background refresh
    }
  }
}

function renderNavbarAuth(user, actions) {
  if (user) {
    const initial = (user.username || "U")[0].toUpperCase();
    actions.innerHTML = `
            <div class="navbar-notifications">
                <a href="#" class="navbar-bell" id="bell-icon" onclick="toggleNotifications(event)">
                    🔔
                    <span class="navbar-bell-dot" id="bell-dot" style="display: none;"></span>
                </a>
                <div class="notifications-dropdown" id="notifications-dropdown">
                    <div class="notifications-header">Notifications</div>
                    <div class="notifications-list" id="notifications-list">
                        <div class="notification-item" style="justify-content:center;color:#888;">No notifications yet.</div>
                    </div>
                </div>
            </div>
            <a href="create-event.html" class="btn btn-acid btn-sm">+ New event</a>
            <div class="navbar-avatar" onclick="window.location.hash='profile'" title="My Profile (${user.username})">
                ${user.profileImageUrl 
                    ? `<img src="${user.profileImageUrl}" alt="${user.username}" style="width:100%;height:100%;border-radius:50%;object-fit:cover;">` 
                    : initial}
            </div>
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
    football: "⚽",
    basketball: "🏀",
    tennis: "🎾",
    volleyball: "🏐",
    running: "🏃",
    swimming: "🏊",
    cycling: "🚴",
    tabletennis: "🏓",
    badminton: "🏸",
    hiking: "🥾",
    yoga: "🧘",
    boxing: "🥊",
  };
  return map[(sport || "").toLowerCase()] || "🏅";
}

// ── Date Formatting Helper ──────────────────────────────

function formatEventDate(dateStr) {
  if (!dateStr) return "—";
  const d = new Date(dateStr);
  const now = new Date();
  const tomorrow = new Date(now);
  tomorrow.setDate(tomorrow.getDate() + 1);

  const timeStr = d.toLocaleTimeString("en-US", {
    hour: "numeric",
    minute: "2-digit",
    hour12: true,
  });

  if (d.toDateString() === now.toDateString()) return `Today · ${timeStr}`;
  if (d.toDateString() === tomorrow.toDateString())
    return `Tomorrow · ${timeStr}`;

  const dayName = d.toLocaleDateString("en-US", { weekday: "long" });
  return `${dayName} · ${timeStr}`;
}

// Ensure navbar is updated on load for any page
function initUI() {
  updateNavbar();
  if (isLoggedIn()) {
      setupNotifications();
  }
}
document.addEventListener("DOMContentLoaded", initUI);
document.addEventListener("routeChanged", initUI);

// ── Notifications UI & SignalR ───────────────────────────

let unreadCount = 0;

async function setupNotifications() {
    const raw = await getNotifications();
    if (raw) {
        renderNotifications(raw);
    }
    
    // Connect to SignalR
    const token = getToken();
    if (!token || typeof signalR === 'undefined') return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5258/hubs/notifications", {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", (notification) => {
        showToast(notification.message);
        addNotificationToList(notification);
        updateBadge(1);
    });

    connection.start().catch(err => console.error("SignalR Connection Error: ", err));
}

function updateBadge(increment) {
    unreadCount += increment;
    const dot = document.getElementById("bell-dot");
    if (dot) {
        dot.style.display = unreadCount > 0 ? "block" : "none";
        dot.innerText = unreadCount > 0 ? unreadCount : "";
    }
}

function renderNotifications(list) {
    unreadCount = 0;
    const ul = document.getElementById("notifications-list");
    if (!ul) return;
    
    if (list.length === 0) {
        ul.innerHTML = '<div class="notification-item" style="justify-content:center;color:#888;padding:15px;">No notifications yet.</div>';
        updateBadge(0);
        return;
    }

    ul.innerHTML = "";
    list.forEach(n => {
        if (!n.isRead) unreadCount++;
        ul.appendChild(createNotificationElement(n));
    });
    updateBadge(0);
}

function addNotificationToList(n) {
    const ul = document.getElementById("notifications-list");
    if (!ul) return;
    
    const isEmpty = ul.innerHTML.includes("No notifications yet.");
    if (isEmpty) ul.innerHTML = "";
    
    ul.insertBefore(createNotificationElement(n), ul.firstChild);
}

function createNotificationElement(n) {
    const div = document.createElement("div");
    div.className = "notification-item" + (!n.isRead ? " unread" : "");
    div.innerHTML = `
        <div class="notification-text">${n.message}</div>
        <div class="notification-time">${n.createdDateFormatted}</div>
    `;
    div.onclick = async () => {
        if (!n.isRead) {
            n.isRead = true;
            div.classList.remove("unread");
            updateBadge(-1);
            await markNotificationRead(n.id);
        }
    };
    return div;
}

window.toggleNotifications = function(e) {
    e.preventDefault();
    const drop = document.getElementById("notifications-dropdown");
    if (drop) {
        drop.classList.toggle("active");
    }
};

window.toggleMenu = function () {
  const links = document.querySelector(".navbar-links");
  if (links) {
    links.style.display = links.style.display === "flex" ? "none" : "flex";
  }
};

// ── Toast Notification UI ────────────────────────────────

function showToast(message) {
    let container = document.getElementById("toast-container");
    if (!container) {
        container = document.createElement("div");
        container.id = "toast-container";
        document.body.appendChild(container);
    }

    const toast = document.createElement("div");
    toast.className = "toast";
    toast.innerHTML = `
        <div class="toast-icon">🔔</div>
        <div class="toast-msg">${message}</div>
    `;
    
    container.appendChild(toast);
    
    setTimeout(() => {
        toast.classList.add("fade-out");
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

export { updateNavbar, renderNavbarAuth, sportEmoji, formatEventDate, setupNotifications, updateBadge, renderNotifications, addNotificationToList, createNotificationElement, showToast };
