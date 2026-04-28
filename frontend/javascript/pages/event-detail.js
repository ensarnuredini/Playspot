import { apiGet, getUser, isLoggedIn, apiPost, apiDelete, getToken } from '../core/api.js';
import { sportEmoji, formatEventDate, showToast } from '../core/ui.js';

// ═════════════════════════════════════════════════════════
//  EVENT DETAIL PAGE
// ═════════════════════════════════════════════════════════

let eventMap = null;

document.addEventListener("DOMContentLoaded", () => {
  initEventDetailPage();
});

async function initEventDetailPage() {
  const params = new URLSearchParams(window.location.search);
  const eventId = params.get("id");
  if (!eventId) return;

  window._eventId = eventId;
  window._isJoined = false;

  const ev = await apiGet(`/event/${eventId}`);
  if (!ev) return;

  // Determine joined state and host state
  window._isHost = false;
  const user = typeof getUser === "function" ? getUser() : null;
  if (user) {
    if (ev.participants && ev.participants.some((p) => p.userId === user.id)) {
      window._isJoined = true;
    }
    if (ev.organizerId === user.id) {
      window._isHost = true;
    }
  }

  populateHero(ev);
  populateActionCard(ev);
  populateAttendees(ev);
  initEventMap(ev);
  populateComments(eventId);
  populateSimilarEvents(ev.sport, ev.id);

  if (window._isHost) {
    populateJoinRequests(eventId);
  }
}

function populateHero(ev) {
  const title = document.querySelector(".detail-title");
  if (title) title.innerHTML = ev.title;

  const sportBadge = document.querySelector(".detail-sport-badge");
  const safeSportEmoji =
    typeof sportEmoji === "function" ? sportEmoji(ev.sport) : "🏅";
  if (sportBadge) sportBadge.textContent = `${safeSportEmoji} ${ev.sport}`;

  const desc = document.querySelector(".detail-desc");
  if (desc) desc.textContent = ev.description || "No description provided.";

  const metaItems = document.querySelectorAll(".detail-meta-value");
  if (metaItems.length >= 5) {
    metaItems[0].textContent = formatEventDate(ev.dateTime || ev.date);
    metaItems[1].textContent = ev.durationMinutes
      ? `${ev.durationMinutes} min`
      : "TBD";
    metaItems[2].textContent = ev.location;
    metaItems[3].textContent = ev.skillLevel || "All levels";
    metaItems[4].textContent = `${ev.approvedParticipantCount || 0} / ${ev.maxParticipants || ev.totalSpots || "?"}`;
  }
}

function populateActionCard(ev) {
  const total = ev.maxParticipants || ev.totalSpots || 10;
  const joined = ev.approvedParticipantCount || 0;
  const spotsLeft = total - joined;

  const spotsCount = document.getElementById("spots-count");
  if (spotsCount) spotsCount.textContent = spotsLeft;

  const capacityFill = document.querySelector(".capacity-fill");
  if (capacityFill) capacityFill.style.width = `${(joined / total) * 100}%`;

  const el = (id) => document.getElementById(id);
  if (el("action-date"))
    el("action-date").textContent = formatEventDate(ev.dateTime || ev.date);
  if (el("action-duration"))
    el("action-duration").textContent = ev.durationMinutes
      ? `${ev.durationMinutes} min`
      : "TBD";
  if (el("action-location")) el("action-location").textContent = ev.location;
  if (el("action-level"))
    el("action-level").textContent = ev.skillLevel || "All levels";
  if (el("action-join-type"))
    el("action-join-type").textContent = ev.requiresApproval
      ? "Request"
      : "Instant";

  const hostName = document.querySelector(".action-host-name");
  if (hostName) hostName.textContent = ev.organizerName || "Unknown";
  const hostAvatar = document.querySelector(".action-host-avatar");
  if (hostAvatar) {
    if (ev.organizerImageUrl) {
        hostAvatar.innerHTML = `<img src="${ev.organizerImageUrl}" alt="${ev.organizerName}" style="width:100%;height:100%;border-radius:50%;object-fit:cover;">`;
        hostAvatar.style.overflow = "hidden";
        hostAvatar.style.padding = "0";
    } else {
        hostAvatar.textContent = (ev.organizerName || "?")[0].toUpperCase();
    }
  }

  const joinBtn = document.getElementById("join-btn");
  if (joinBtn) {
    if (window._isJoined) {
      joinBtn.textContent = "✓ Joined — leave event";
      joinBtn.classList.add("joined");
    } else if (spotsLeft <= 0) {
      joinBtn.textContent = "Event Full";
      joinBtn.disabled = true;
    } else {
      joinBtn.textContent = "Join event →";
      joinBtn.classList.remove("joined");
    }
  }
}

function populateAttendees(ev) {
  const total = ev.maxParticipants || ev.totalSpots || 10;
  const joined = ev.approvedParticipantCount || 0;

  // Update header
  const titleEls = document.querySelectorAll(".detail-section-title");
  titleEls.forEach((el) => {
    if (el.textContent.includes("Players")) {
      el.textContent = `Players (${joined} / ${total})`;
    }
  });

  const grid = document.querySelector(".attendees-grid");
  const spotsContainer = document.querySelector(".spots-grid");

  if (grid && ev.participants) {
    const colors = ["blue", "pink", "green", "amber", "purple"];
    let html = "";
    ev.participants.forEach((p, index) => {
      const color = colors[index % colors.length];
      const isHostStr = p.isHost ? '<div class="attendee-role">Host</div>' : "";
      const hostClass = p.isHost ? "host" : "";
      const avatarContent = p.profileImageUrl 
          ? `<img src="${p.profileImageUrl}" alt="${p.username}" style="width:100%;height:100%;border-radius:50%;object-fit:cover;">` 
          : (p.username || "?")[0].toUpperCase();
      html += `
                <div class="attendee-card ${hostClass}">
                    <div class="attendee-avatar avatar-${color}" style="overflow:hidden; padding:0;">${avatarContent}</div>
                    <div>
                        <div class="attendee-name">${p.username}</div>
                        ${isHostStr}
                    </div>
                </div>`;
    });
    grid.innerHTML = html;
  }

  if (spotsContainer) {
    const spotsLeft = total - joined;
    let spotsHtml = "";
    for (let i = 0; i < spotsLeft && i < 10; i++) {
      spotsHtml += `<div class="spot-empty" title="Open spot — join to fill it!">+</div>`;
    }
    if (spotsLeft > 10) spotsHtml += `<div>+ ${spotsLeft - 10} more</div>`;
    spotsContainer.innerHTML = spotsHtml;
  }
}

function initEventMap(ev) {
  if (!window.L) return;
  const mapContainer = document.getElementById("event-map");
  if (!mapContainer) return;

  // Only render if we have coordinates, else fallback
  const lat = ev.latitude !== 0 ? ev.latitude : 41.9981;
  const lng = ev.longitude !== 0 ? ev.longitude : 21.4254;

  mapContainer.innerHTML = ""; // clear static placeholders
  eventMap = L.map("event-map", {
    zoomControl: false,
    scrollWheelZoom: false,
  }).setView([lat, lng], 15);

  L.tileLayer(
    "https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png",
    {
      attribution: "&copy; OpenStreetMap &copy; CARTO",
    },
  ).addTo(eventMap);

  const safeSportEmoji =
    typeof sportEmoji === "function" ? sportEmoji(ev.sport) : "📍";
  const icon = L.divIcon({
    className: "custom-map-icon",
    html: `<div class="map-pin-bubble active">${safeSportEmoji}</div><div class="map-pin-tail active"></div>`,
    iconSize: [40, 48],
    iconAnchor: [20, 48],
  });
  L.marker([lat, lng], { icon }).addTo(eventMap);
}

async function populateComments(eventId) {
  const comments = await apiGet(`/comment/event/${eventId}`);
  const commentsList = document.querySelector(".comments-list");
  if (!commentsList) return;

  const sections = document.querySelectorAll(".detail-section-title");
  sections.forEach((s) => {
    if (s.textContent.includes("Comments")) {
      s.textContent = `Comments (${comments ? comments.length : 0})`;
    }
  });

  if (comments && comments.length > 0) {
    commentsList.innerHTML = comments
      .map(
        (c) => `
            <div class="comment">
                <div class="comment-avatar avatar-blue">${(c.username || "U")[0].toUpperCase()}</div>
                <div class="comment-body">
                    <div class="comment-header">
                        <span class="comment-name">${c.username}</span>
                        <span class="comment-time">${new Date(c.createdAt).toLocaleString()}</span>
                    </div>
                    <div class="comment-text">${c.text}</div>
                </div>
            </div>
        `,
      )
      .join("");
  } else {
    commentsList.innerHTML =
      '<div style="text-align:center;padding:20px;opacity:.4;">No comments yet. Be the first!</div>';
  }
}

async function populateSimilarEvents(sport, currentId) {
  const similar = await apiGet(`/event?sport=${sport}`);
  const similarContainer = document.querySelector(".similar-events");
  if (!similarContainer || !similar) return;

  const otherEvents = similar.filter((s) => s.id !== currentId).slice(0, 3);
  const safeSportEmoji =
    typeof sportEmoji === "function" ? sportEmoji(sport) : "🏅";

  if (otherEvents.length > 0) {
    similarContainer.innerHTML = otherEvents
      .map(
        (s) => `
            <a href="event-detail.html?id=${s.id}" class="similar-event-row">
                <span class="similar-icon">${safeSportEmoji}</span>
                <div class="similar-info">
                    <div class="similar-title">${s.title}</div>
                    <div class="similar-meta">${formatEventDate(s.dateTime || s.date)} · ${s.location}</div>
                </div>
                <span class="badge badge-success">Open</span>
            </a>
        `,
      )
      .join("");
  } else {
    similarContainer.innerHTML =
      '<div style="text-align:center;padding:16px;opacity:.4;">No similar events found.</div>';
  }
}

// Global Actions for HTML

window.postComment = async function () {
  const input = document.getElementById("comment-input");
  const text = input.value.trim();
  if (!text || !window._eventId) return;
  if (typeof isLoggedIn === "function" && !isLoggedIn()) {
    window.location.href = "auth.html";
    return;
  }

  const result = await apiPost(`/comment/event/${window._eventId}`, { text });
  if (result && result.ok) {
    const c = result.data;
    const list = document.querySelector(".comments-list");
    if (list && list.innerHTML.includes("No comments yet")) list.innerHTML = "";
    const div = document.createElement("div");
    div.className = "comment fade-up";
    div.innerHTML = `
            <div class="comment-avatar avatar-purple">${(c.username || "U")[0].toUpperCase()}</div>
            <div class="comment-body">
                <div class="comment-header">
                    <span class="comment-name">${c.username || "You"}</span>
                    <span class="comment-time">Just now</span>
                </div>
                <div class="comment-text">${c.text}</div>
            </div>`;
    list.appendChild(div);
    input.value = "";
  }
};

window.toggleJoin = async function () {
  if (typeof isLoggedIn === "function" && !isLoggedIn()) {
    window.location.href = "auth.html";
    return;
  }
  if (!window._eventId) return;

  const btn = document.getElementById("join-btn");
  const spotsEl = document.getElementById("spots-count");

  if (!window._isJoined) {
    const result = await apiPost(`/joinrequest/${window._eventId}`);
    if (result && result.ok) {
      window._isJoined = true;
      btn.textContent = "✓ Joined — leave event";
      btn.classList.add("joined");
      let spots = parseInt(spotsEl.textContent);
      spotsEl.textContent = Math.max(0, spots - 1);
      location.reload();
    }
  } else {
    const result = await apiDelete(`/joinrequest/${window._eventId}`);
    if (result && result.ok) {
      window._isJoined = false;
      btn.textContent = "Join event →";
      btn.classList.remove("joined");
      let spots = parseInt(spotsEl.textContent);
      spotsEl.textContent = spots + 1;
      location.reload();
    }
  }
};

window.shareEvent = function () {
  if (navigator.share) {
    navigator.share({ title: document.title, url: window.location.href });
  } else {
    const temp = document.createElement("input");
    document.body.appendChild(temp);
    temp.value = window.location.href;
    temp.select();
    document.execCommand("copy");
    document.body.removeChild(temp);
    showToast("URL Copied to clipboard.");
  }
};

async function populateJoinRequests(eventId) {
  const requestsSection = document.getElementById("host-requests-section");
  const requestsList = document.getElementById("host-requests-list");
  if (!requestsSection || !requestsList) return;

  const requests = await apiGet(`/joinrequest/event/${eventId}`);
  if (!requests || requests.length === 0) return;

  const pending = requests.filter((r) => r.status === "Pending");
  if (pending.length === 0) return;

  requestsSection.style.display = "block";
  requestsList.innerHTML = pending
    .map(
      (r) => `
        <div style="display:flex;justify-content:space-between;align-items:center;padding:12px;border:1px solid var(--border);border-radius:8px;">
            <div style="display:flex;align-items:center;gap:12px;">
                <div class="avatar-blue" style="width:32px;height:32px;border-radius:50%;display:flex;align-items:center;justify-content:center;color:#fff;font-weight:600;">${(r.username || "U")[0].toUpperCase()}</div>
                <div>
                    <div style="font-weight:500;">${r.username}</div>
                    <div style="font-size:12px;color:var(--muted);">Requested ${new Date(r.requestedAt).toLocaleDateString()}</div>
                </div>
            </div>
            <div style="display:flex;gap:8px;">
                <button class="btn btn-ghost btn-sm" onclick="handleJoinRequest(${r.id}, 'Rejected')">Decline</button>
                <button class="btn btn-acid btn-sm" onclick="handleJoinRequest(${r.id}, 'Approved')">Accept</button>
            </div>
        </div>
    `,
    )
    .join("");
}

window.handleJoinRequest = async function (requestId, status) {
  const token = typeof getToken === "function" ? getToken() : "";
  try {
    const res = await fetch(
      `http://localhost:5258/api/JoinRequest/${requestId}/status`,
      {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(status),
      },
    );
    if (res.ok) {
      showToast(`Request ${status.toLowerCase()}`);
      setTimeout(() => location.reload(), 1000);
    } else {
      showToast("Failed to update status.");
    }
  } catch (e) {
    console.error(e);
    showToast("Error updating status.");
  }
};
