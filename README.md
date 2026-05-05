# PlaySpot — Project Documentation

---

## 1. Project Name

**PlaySpot**

---

## 2. One-Sentence Description

A full-stack web platform that enables users to create, discover, and join local sports events in real time using an interactive map interface.

---

## 3. Detailed Description / Purpose

PlaySpot is a community-driven sports platform designed to solve the problem of finding casual sports partners and pickup games. Users register, browse a live map populated with nearby events, and request to join activities organized by other users. Event hosts can approve or decline join requests, manage participants, and track their hosted and attended events through a personal dashboard.

The platform follows a clean **Single Page Application (SPA)** architecture on the frontend — using hash-based client-side routing — paired with a **.NET 10 Clean Architecture** backend that exposes a RESTful JSON API. Real-time notifications are delivered via **SignalR** WebSockets, so hosts and participants receive instant feedback when join requests are submitted, accepted, or declined.

The project is fully containerized with Docker and deployed on **Railway** (backend API + frontend served via Nginx).

---

## 4. Key Features

- **Interactive Map** — Explore events plotted on a Leaflet.js map with custom sport-category markers, hover effects, and click-to-view detail
- **Event CRUD** — Authenticated users can create, edit, and delete events with rich fields (sport, skill level, gender, age range, duration, max participants, approval toggle)
- **Map Picker** — Pin-drop location selector when creating events
- **Join Request Workflow** — Users request to join events; hosts accept/decline requests with real-time status updates
- **Real-Time Notifications** — SignalR-powered push notifications for join request activity, persisted to the database
- **User Profiles** — Editable profiles (name, city, bio, avatar) with tabbed views for hosted and joined events
- **Dashboard** — Personalized view showing upcoming hosted events, joined events, and past events with filter controls
- **Comments** — Users can post and view comments on event detail pages
- **Event Ratings** — Post-event rating system (one rating per user per event)
- **Saved Events** — Bookmark events for later viewing
- **Event Reporting** — Flag inappropriate events
- **Similar Events** — API endpoint returns sport-matched suggestions
- **JWT Authentication** — Secure register/login flow with token-based access
- **Swagger UI** — Auto-generated API documentation available at `/swagger`
- **SPA Routing** — Hash-based client-side router with no page reloads
- **Toast Notifications** — Non-intrusive inline UI feedback (no browser alerts)

---

## 5. Tech Stack

### Languages

| Layer    | Language                             |
| -------- | ------------------------------------ |
| Backend  | C# 13 (.NET 10)                      |
| Frontend | JavaScript (ES Modules), HTML5, CSS3 |

### Backend Frameworks & Libraries

| Package                              | Purpose                           |
| ------------------------------------ | --------------------------------- |
| ASP.NET Core 10                      | Web API framework                 |
| Entity Framework Core 10             | ORM / database access             |
| MediatR 12.4                         | CQRS / mediator pattern           |
| ASP.NET Core SignalR                 | Real-time WebSocket notifications |
| BCrypt.Net-Next 4.1                  | Password hashing                  |
| System.IdentityModel.Tokens.Jwt 8.17 | JWT generation & validation       |
| Swashbuckle.AspNetCore 6.9           | Swagger / OpenAPI docs            |
| SQLite (via EF Core Sqlite)          | Embedded database                 |

### Frontend Libraries

| Library                         | Purpose                       |
| ------------------------------- | ----------------------------- |
| Leaflet.js                      | Interactive map rendering     |
| Microsoft SignalR JS Client 7.0 | Real-time notification client |

### DevOps / Infrastructure

| Tool                | Purpose                               |
| ------------------- | ------------------------------------- |
| Docker              | Containerization (multi-stage builds) |
| Railway             | Cloud hosting (backend + frontend)    |
| Nginx (Alpine)      | Static file server for frontend       |
| VS Code Live Server | Local frontend development            |

---

## 6. Installation Steps

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A modern browser (Chrome, Firefox, Edge)
- (Optional) [Docker](https://www.docker.com/) for containerized deployment
- (Optional) VS Code with the [Live Server](https://marketplace.visualstudio.com/items?itemName=ritwickdey.LiveServer) extension

### Backend Setup

```bash
# 1. Clone the repository
git clone https://github.com/<owner>/Playspot.git
cd Playspot

# 2. Restore NuGet packages
dotnet restore backend/Playspot.API/Playspot.API.csproj

# 3. Run the backend API (defaults to port 8080)
dotnet run --project backend/Playspot.API
```

The API will be available at `http://localhost:8080`. Swagger UI is at `http://localhost:8080/swagger`.

> [!NOTE]
> The SQLite database (`playspot.db`) is auto-created on first startup via `EnsureCreated()`. No manual migrations are required.

### Frontend Setup

```bash
# Option A — VS Code Live Server
# Open the `frontend/` folder in VS Code and launch Live Server (defaults to port 5500).

# Option B — Any static file server
cd frontend
npx -y serve .
```

> [!IMPORTANT]
> If the frontend is served on a port other than `5500`, `3000`, or `8080`, update the CORS origins in `backend/Playspot.API/Program.cs`.

### Docker Deployment

```bash
# Backend
cd backend
docker build -t playspot-api .
docker run -p 8080:8080 playspot-api

# Frontend
cd frontend
docker build -t playspot-frontend .
docker run -p 80:80 playspot-frontend
```

---

## 7. Usage Instructions

### Register & Login

1. Navigate to the app → you land on the **Landing page** (`#landing`)
2. Click "Get Started" → redirected to **Auth page** (`#auth`)
3. Fill in username, email, password → click Register
4. After registration, use the same credentials to login
5. A JWT token is stored in `localStorage` and attached to all subsequent API calls

### Create an Event

1. Click "Create Event" in the navigation → (`#create-event`)
2. Fill in title, sport, description, date/time, max participants
3. Drop a pin on the map to set the location
4. Toggle "Requires Approval" if you want to manually approve participants
5. Submit → event appears on the map immediately

### Browse & Join Events

1. Navigate to **Explore** (`#explore`) to see all events on the map
2. Click a marker or card to view event details (`#event-detail?id=<eventId>`)
3. Click "Request to Join" → the host receives a real-time notification
4. The button updates to "Requested" (pending) or "Joined" (auto-approved)

### Manage Events (Host)

1. Go to **My Events** (`#my-events`) to see your hosted events
2. View pending join requests on the event detail page
3. Accept or decline each request — participants are notified in real time

### Profile

1. Navigate to **Profile** (`#profile?id=<userId>`) to view any user's profile
2. Your own profile shows "Edit Profile" to update name, city, bio, and avatar URL
3. Tabs display "Hosted Events" and "Joined Events"

---

## 8. Configuration / Environment Variables

### `appsettings.json` (Backend)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=playspot.db"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharsLong!",
    "Issuer": "PlayspotAPI",
    "Audience": "PlayspotClient",
    "ExpiresInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

| Variable                              | Description                            | Default                   |
| ------------------------------------- | -------------------------------------- | ------------------------- |
| `ConnectionStrings:DefaultConnection` | SQLite connection string               | `Data Source=playspot.db` |
| `Jwt:Key`                             | HMAC-SHA256 signing key (≥32 chars)    | Hardcoded dev key         |
| `Jwt:Issuer`                          | JWT issuer claim                       | `PlayspotAPI`             |
| `Jwt:Audience`                        | JWT audience claim                     | `PlayspotClient`          |
| `Jwt:ExpiresInMinutes`                | Token lifetime                         | `60`                      |
| `PORT` (env var)                      | Server port override (used by Railway) | `8080`                    |

### Frontend

| Constant   | Location                                 | Description                                                                                     |
| ---------- | ---------------------------------------- | ----------------------------------------------------------------------------------------------- |
| `API_BASE` | `frontend/javascript/core/api.js` line 1 | Base URL for the backend API. Change this to `http://localhost:8080/api` for local development. |

> [!WARNING]
> The JWT secret key in `appsettings.json` is a development placeholder. In production, use a secure secret injected via environment variables or a secret manager.

---

## 9. Project Structure

```
Playspot/
├── Playspot.sln                           # Visual Studio solution file
├── README.md                              # Project readme
├── .gitignore                             # Git ignore rules
│
├── backend/                               # .NET backend (Clean Architecture)
│   ├── Dockerfile                         # Multi-stage Docker build
│   ├── Playspot.slnx                      # Lightweight solution reference
│   │
│   ├── Playspot.API/                      # 🌐 Presentation layer (entry point)
│   │   ├── Program.cs                     # App startup, DI, middleware pipeline
│   │   ├── appsettings.json               # Configuration (DB, JWT, logging)
│   │   ├── Controllers/                   # API controller classes
│   │   │   ├── AuthController.cs          #   POST register, login
│   │   │   ├── EventController.cs         #   CRUD events, my hosting/joined/past
│   │   │   ├── EventActionsController.cs  #   Save, report, rate events
│   │   │   ├── JoinRequestController.cs   #   Join, approve, reject, withdraw
│   │   │   ├── CommentController.cs       #   Get/add comments
│   │   │   ├── UsersController.cs         #   Profiles, user events
│   │   │   └── NotificationsController.cs #   Get/mark-read notifications
│   │   ├── Hubs/
│   │   │   └── NotificationHub.cs         # SignalR hub (real-time push)
│   │   ├── Services/
│   │   │   └── NotificationHubService.cs  # INotificationService → SignalR bridge
│   │   └── playspot.db                    # SQLite database file (auto-created)
│   │
│   ├── Playspot.Application/             # 📦 Application / Use-case layer
│   │   ├── DTOs/                          # Data transfer objects
│   │   │   ├── Auth/                      #   RegisterDto, LoginDto, AuthResultDto
│   │   │   ├── Events/                    #   CreateEventDto, EventFilterDto, etc.
│   │   │   ├── Comments/                  #   CreateCommentDto, CommentDto
│   │   │   ├── JoinRequests/              #   JoinRequest DTOs
│   │   │   ├── Notifications/             #   NotificationDto
│   │   │   ├── Ratings/                   #   CreateRatingDto
│   │   │   ├── Reports/                   #   CreateReportDto
│   │   │   └── Users/                     #   UserProfileDto, UpdateProfileDto
│   │   ├── Features/                      # MediatR command/query handlers
│   │   │   ├── Auth/                      #   RegisterCommand, LoginCommand
│   │   │   ├── Events/                    #   CRUD + filtered/my queries
│   │   │   ├── EventActions/              #   Save, report, rate commands
│   │   │   ├── JoinRequests/              #   Join, status update, withdraw
│   │   │   ├── Comments/                  #   Add/get comments
│   │   │   ├── Notifications/             #   Get/mark-read
│   │   │   └── Users/                     #   Profile queries & commands
│   │   └── Interfaces/                    # Abstractions
│   │       ├── IAppDbContext.cs            #   Database context contract
│   │       ├── IJwtTokenGenerator.cs       #   Token generation contract
│   │       └── INotificationService.cs     #   Push notification contract
│   │
│   ├── Playspot.Domain/                   # 🏛️ Domain / Entity layer
│   │   └── Entities/
│   │       ├── User.cs                    #   User entity (profile fields)
│   │       ├── Event.cs                   #   Event entity (location, sport, etc.)
│   │       ├── JoinRequest.cs             #   Join request (Pending/Approved/Rejected)
│   │       ├── Comment.cs                 #   Event comment
│   │       ├── Notification.cs            #   User notification
│   │       ├── SavedEvent.cs              #   Bookmarked event
│   │       ├── EventRating.cs             #   Event rating
│   │       └── EventReport.cs             #   Event report/flag
│   │
│   └── Playspot.Infrastructure/           # 🔧 Infrastructure layer
│       ├── Data/
│       │   └── AppDbContext.cs            #   EF Core DbContext + Fluent API config
│       ├── Migrations/                    #   EF Core migration history (4 migrations)
│       └── Services/
│           └── JwtTokenGenerator.cs       #   JWT token generation implementation
│
└── frontend/                              # 🎨 Frontend SPA
    ├── index.html                         # App shell (single entry point)
    ├── DockerFile                         # Nginx container for static files
    ├── css/
    │   ├── base.css                       #   Design tokens, reset, typography
    │   ├── components.css                 #   Reusable component styles
    │   └── pages/                         #   Page-specific stylesheets (8 files)
    ├── html/                              # Page templates (loaded by router)
    │   ├── landing.html                   #   Marketing landing page
    │   ├── auth.html                      #   Login / register form
    │   ├── dashboard.html                 #   User dashboard
    │   ├── explore.html                   #   Map-based event explorer
    │   ├── create-event.html              #   Event creation form + map picker
    │   ├── event-detail.html              #   Single event view + join/comments
    │   ├── my-events.html                 #   Host's event management
    │   └── profile.html                   #   User profile + event tabs
    └── javascript/
        ├── auth.js                        #   Auth form logic
        ├── create-event.js                #   Event creation logic + map picker
        ├── core/
        │   ├── api.js                     #   Fetch wrapper, token management
        │   ├── router.js                  #   Hash-based SPA router
        │   └── ui.js                      #   Navbar, toast notifications, shared UI
        └── pages/
            ├── landing.js                 #   Landing page init
            ├── dashboard.js               #   Dashboard data loading + filters
            ├── explore.js                 #   Map rendering + event cards
            ├── event-detail.js            #   Event detail, join flow, comments
            ├── my-events.js               #   Host event management
            └── profile.js                 #   Profile view/edit, event tabs
```

---

## 10. API Endpoints

Base URL: `/api`

### Authentication (`/api/auth`)

| Method | Endpoint         | Auth | Description             |
| ------ | ---------------- | ---- | ----------------------- |
| `POST` | `/auth/register` | ❌   | Register a new user     |
| `POST` | `/auth/login`    | ❌   | Login and receive a JWT |

### Events (`/api/event`)

| Method   | Endpoint              | Auth | Description                                   |
| -------- | --------------------- | ---- | --------------------------------------------- |
| `GET`    | `/event`              | ❌   | Get all events (supports filter query params) |
| `GET`    | `/event/{id}`         | ❌   | Get event by ID                               |
| `POST`   | `/event`              | ✅   | Create a new event                            |
| `PUT`    | `/event/{id}`         | ✅   | Update an event (owner only)                  |
| `DELETE` | `/event/{id}`         | ✅   | Delete an event (owner only)                  |
| `GET`    | `/event/my/hosting`   | ✅   | Get events the user is hosting                |
| `GET`    | `/event/my/joined`    | ✅   | Get events the user has joined                |
| `GET`    | `/event/my/past`      | ✅   | Get user's past events                        |
| `GET`    | `/event/{id}/similar` | ❌   | Get similar events by sport                   |

### Event Actions (`/api/event`)

| Method   | Endpoint                   | Auth | Description              |
| -------- | -------------------------- | ---- | ------------------------ |
| `POST`   | `/event/{eventId}/save`    | ✅   | Bookmark an event        |
| `DELETE` | `/event/{eventId}/save`    | ✅   | Remove bookmark          |
| `GET`    | `/event/saved`             | ✅   | Get all saved events     |
| `GET`    | `/event/{eventId}/saved`   | ✅   | Check if event is saved  |
| `POST`   | `/event/{eventId}/report`  | ✅   | Report an event          |
| `POST`   | `/event/{eventId}/rate`    | ✅   | Rate an event            |
| `GET`    | `/event/{eventId}/ratings` | ❌   | Get ratings for an event |

### Join Requests (`/api/joinrequest`)

| Method   | Endpoint                          | Auth | Description                                |
| -------- | --------------------------------- | ---- | ------------------------------------------ |
| `POST`   | `/joinrequest/{eventId}`          | ✅   | Request to join an event                   |
| `GET`    | `/joinrequest/event/{eventId}`    | ✅   | Get join requests for an event (host view) |
| `PATCH`  | `/joinrequest/{requestId}/status` | ✅   | Accept or decline a request (host)         |
| `DELETE` | `/joinrequest/{eventId}`          | ✅   | Withdraw a join request                    |

### Comments (`/api/comment`)

| Method | Endpoint                   | Auth | Description               |
| ------ | -------------------------- | ---- | ------------------------- |
| `GET`  | `/comment/event/{eventId}` | ❌   | Get comments for an event |
| `POST` | `/comment/event/{eventId}` | ✅   | Add a comment to an event |

### Users (`/api/users`)

| Method | Endpoint                    | Auth | Description                     |
| ------ | --------------------------- | ---- | ------------------------------- |
| `GET`  | `/users/{id}`               | ❌   | Get user profile                |
| `PUT`  | `/users/{id}`               | ✅   | Update user profile (self only) |
| `GET`  | `/users/{id}/events`        | ❌   | Get events hosted by user       |
| `GET`  | `/users/{id}/joined-events` | ❌   | Get events joined by user       |

### Notifications (`/api/notifications`)

| Method  | Endpoint                   | Auth | Description                 |
| ------- | -------------------------- | ---- | --------------------------- |
| `GET`   | `/notifications`           | ✅   | Get user's notifications    |
| `PATCH` | `/notifications/{id}/read` | ✅   | Mark a notification as read |

### Real-Time (SignalR)

| Hub             | Endpoint              | Auth                 | Events                |
| --------------- | --------------------- | -------------------- | --------------------- |
| NotificationHub | `/hubs/notifications` | ✅ (via query token) | `ReceiveNotification` |

---

## 11. Known Issues / Limitations

| #   | Issue                                                          | Details                                                                                                                                                                     |
| --- | -------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **Hardcoded API base URL**                                     | `API_BASE` in `api.js` points to the Railway production URL. Developers must manually change this for local development. No `.env` or build-time variable injection exists. |
| 2   | **SQLite in production**                                       | SQLite is a single-file embedded database. It does not support concurrent writes well and the database is lost on Railway's ephemeral filesystem on each deploy.            |
| 3   | **No image upload**                                            | Profile images and event images use URL strings only — there is no file upload endpoint or cloud storage integration.                                                       |
| 4   | **JWT secret in source**                                       | The JWT signing key is committed in `appsettings.json`. It should be injected via environment variables in production.                                                      |
| 5   | **No email verification**                                      | Registration does not verify email addresses.                                                                                                                               |
| 6   | **No pagination**                                              | The `GET /event` endpoint returns all events. No server-side pagination or infinite scroll is implemented.                                                                  |
| 7   | **No admin panel**                                             | There is no admin/moderator interface for managing users, reports, or content moderation.                                                                                   |
| 8   | **SavedEvent & Report entities exist but frontend is removed** | The Save and Report features were removed from the frontend UI but the backend endpoints and database tables still exist.                                                   |
| 9   | **No unit/integration tests**                                  | The project has no test projects or test coverage.                                                                                                                          |
| 10  | **CORS in dev mode**                                           | Development mode uses `AllowAll` CORS policy, which is insecure.                                                                                                            |

---

## 12. Future Improvements / Roadmap

| Priority  | Improvement                  | Description                                                                                                  |
| --------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------ |
| 🔴 High   | **PostgreSQL migration**     | Replace SQLite with PostgreSQL for production reliability, concurrent writes, and Railway persistent storage |
| 🔴 High   | **Environment-based config** | Externalize `API_BASE`, JWT secrets, and DB connection strings via environment variables                     |
| 🔴 High   | **Pagination**               | Add server-side pagination to event listing and user event queries                                           |
| 🟡 Medium | **Image uploads**            | Integrate cloud storage (e.g., Cloudinary, AWS S3) for profile and event images                              |
| 🟡 Medium | **Email verification**       | Send verification emails on registration                                                                     |
| 🟡 Medium | **Admin dashboard**          | Build a moderator panel for managing reports, users, and flagged content                                     |
| 🟡 Medium | **Unit & integration tests** | Add xUnit test projects for Application and API layers                                                       |
| 🟡 Medium | **Geolocation-based search** | Filter events by proximity (radius search using lat/lng)                                                     |
| 🟢 Low    | **PWA support**              | Add service worker and manifest for offline access and install prompt                                        |
| 🟢 Low    | **Event chat**               | Add real-time group chat within events using SignalR                                                         |
| 🟢 Low    | **Marker clustering**        | Cluster map markers at higher zoom levels for performance                                                    |
| 🟢 Low    | **Dark mode toggle**         | Add user-controlled dark/light theme switching                                                               |
| 🟢 Low    | **Social auth**              | Google / Facebook OAuth login                                                                                |

---

## 13. License

No license file is present in the repository. The project is currently **unlicensed** (all rights reserved by default).

> [!TIP]
> Consider adding a `LICENSE` file (e.g., MIT, Apache 2.0) to clarify usage and contribution terms.

---

_Documentation generated on 2026-05-01 from source analysis._
