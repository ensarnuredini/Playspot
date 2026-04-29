const routes = {
  landing: { template: "html/landing.html", script: "../pages/landing.js" },
  auth: { template: "html/auth.html", script: "../auth.js" },
  dashboard: {
    template: "html/dashboard.html",
    script: "../pages/dashboard.js",
  },
  explore: { template: "html/explore.html", script: "../pages/explore.js" },
  "create-event": {
    template: "html/create-event.html",
    script: "../create-event.js",
  },
  "event-detail": {
    template: "html/event-detail.html",
    script: "../pages/event-detail.js",
  },
  "my-events": {
    template: "html/my-events.html",
    script: "../pages/my-events.js",
  },
  profile: { template: "html/profile.html", script: "../pages/profile.js" },
};

export async function route() {
  let hash = window.location.hash.substring(1);
  if (!hash || hash === "/") hash = "landing";

  let path = hash;
  let queryString = "";
  if (hash.includes("?")) {
    [path, queryString] = hash.split("?");
  }

  // Default route mapping for index.html etc
  if (path === "index.html" || path === "index") {
    path = "landing";
  }

  const matchedRoute = routes[path];
  const appRoot = document.getElementById("app-root");

  if (!matchedRoute) {
    appRoot.innerHTML = "<h1>404 - Page Not Found</h1>";
    return;
  }

  try {
    const response = await fetch(matchedRoute.template);
    if (!response.ok) throw new Error("Template not found");
    let html = await response.text();

    // Extract body content if present to strip boilerplate
    const bodyMatch = html.match(/<body[^>]*>([\s\S]*?)<\/body>/i);
    if (bodyMatch) {
      html = bodyMatch[1];
    }

    // Remove script tags to prevent execution issues
    html = html.replace(
      /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,
      "",
    );

    appRoot.innerHTML = html;

    // Ensure global components are updated
    if (typeof window.updateNavbar === "function") window.updateNavbar(); // Wait, updateNavbar is imported in ui.js, might not be global
    // We'll rely on the page scripts to call UI updates or trigger an event

    // Load specific JS script and run its init
    if (matchedRoute.script) {
      const module = await import(matchedRoute.script);
      if (module && typeof module.init === "function") {
        const params = new URLSearchParams(queryString);
        await module.init(params);
      }
    }

    // Dispatch route changed event
    document.dispatchEvent(
      new CustomEvent("routeChanged", {
        detail: { path, params: queryString },
      }),
    );
  } catch (e) {
    console.error("Routing error:", e);
    appRoot.innerHTML = "<h1>Error loading page</h1>";
  }
}

// Intercept regular <a> clicks
document.addEventListener("click", (e) => {
  const a = e.target.closest("a");
  if (a && a.href) {
    // If external or mailto etc, ignore
    const url = new URL(a.href, window.location.origin);
    if (url.origin !== window.location.origin) return;

    // If it points to an HTML file, convert to hash route
    if (url.pathname.endsWith(".html")) {
      e.preventDefault();
      let pageName = url.pathname.split("/").pop().replace(".html", "");
      if (pageName === "index") pageName = "landing";
      window.location.hash = pageName + url.search;
    } else if (url.pathname === "/" && a.getAttribute("href") !== "#") {
      e.preventDefault();
      window.location.hash = "landing" + url.search;
    }
  }
});

// For forms submitting to .html files, we might need to intercept submits too
// But forms here usually preventDefault() via inline `onsubmit` or JS handlers

window.addEventListener("hashchange", route);
window.addEventListener("DOMContentLoaded", route);
