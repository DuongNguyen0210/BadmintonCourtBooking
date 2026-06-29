# Spec: BadmintonCourtBooking Authentication Phase 1

## Objective

Build the first authentication module for BadmintonCourtBooking.

This phase only covers:

- User registration with email and password.
- User login and logout.
- Safe password hashing through ASP.NET Core Identity.
- PostgreSQL persistence.
- Current-user API (`GET /api/auth/me`).
- Basic React authentication state.
- Basic protected frontend route.

This phase does not cover:

- Court booking.
- Court/host management.
- Payment.
- Reviews.
- Chat.
- Notifications.
- Admin/host/user role authorization.

## Confirmed Decisions

- Authentication: ASP.NET Core Identity + cookie authentication.
- Database: PostgreSQL local through Docker Compose.
- Backend structure: one simple Web API project first, not Clean Architecture yet.
- Frontend: React + TypeScript + Vite + Tailwind CSS + React Router + Axios.

## Local Environment Note

The existing backend project currently targets `net10.0`, but this machine currently has only .NET SDK 8.0 installed.

Implementation should keep the intended .NET 10 target unless the project owner approves a temporary downgrade to `net8.0`. Backend build verification will require installing .NET 10 SDK or approving the target framework change.

## Tech Stack

Backend:

- ASP.NET Core Web API
- .NET 10 target
- Entity Framework Core
- ASP.NET Core Identity
- Npgsql.EntityFrameworkCore.PostgreSQL
- PostgreSQL

Frontend:

- React
- TypeScript
- Vite
- Tailwind CSS
- React Router
- Axios

Database:

- PostgreSQL in Docker Compose for local development.

## Commands

Backend restore/build/run:

```powershell
dotnet restore .\BadmintonCourtBooking.sln
dotnet build .\BadmintonCourtBooking.sln
dotnet run --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
```

Database:

```powershell
docker compose up -d
docker compose ps
```

EF Core migration/update:

```powershell
dotnet ef migrations add InitialIdentitySchema --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
dotnet ef database update --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
```

Frontend:

```powershell
cd .\frontend
npm install
npm run dev
npm run build
```

## Project Structure

Target structure for this phase:

```text
BadmintonCourtBooking/
  BadmintonCourtBooking.sln
  BadmintonCourtBooking/
    Controllers/
    Data/
    Dtos/
    Models/
    Migrations/
    Program.cs
    appsettings.json
    appsettings.Development.json
    BadmintonCourtBooking.csproj
  frontend/
    src/
      api/
      components/
      contexts/
      pages/
      routes/
      types/
      App.tsx
      main.tsx
  docs/
    auth-spec.md
  docker-compose.yml
```

The existing project is currently at `BadmintonCourtBooking/`, so this phase will keep the backend there instead of moving it into a new `backend/` folder. Moving folders can be a later cleanup after the first working auth slice.

## Code Style

Backend DTO style:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BadmintonCourtBooking.Dtos;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
}
```

Frontend style:

```tsx
type UserResponse = {
  id: string;
  email: string;
  fullName: string;
};

export function LoginPage() {
  return <main className="mx-auto max-w-md px-4 py-10">Login</main>;
}
```

Conventions:

- C# nullable reference types enabled.
- One DTO per file.
- Controllers return clear HTTP status codes.
- React components use function components.
- Frontend API types live in `frontend/src/types`.
- No booking/court/host/payment/role code in this phase.

## API Endpoints

| Method | Endpoint | Purpose | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Register a new user | Public |
| POST | `/api/auth/login` | Login and issue auth cookie | Public |
| POST | `/api/auth/logout` | Clear auth cookie | Required |
| GET | `/api/auth/me` | Return current user | Required |

## DTOs

Backend request/response DTOs:

- `RegisterRequest`: `email`, `password`, `confirmPassword`, `fullName`
- `LoginRequest`: `email`, `password`, `rememberMe`
- `AuthResponse`: `success`, `message`, `user`
- `UserResponse`: `id`, `email`, `fullName`

No access token is returned in phase 1 because cookie authentication is selected.

## Authentication Flow

Register:

1. React submits `RegisterRequest`.
2. API validates request.
3. ASP.NET Core Identity creates the user.
4. Identity hashes the password.
5. User is stored in PostgreSQL.

Login:

1. React submits `LoginRequest`.
2. API validates credentials through Identity.
3. API signs in the user with an HTTP-only auth cookie.
4. React calls `/api/auth/me` to load current user state.

Me:

1. React sends request with credentials.
2. API reads authenticated user from cookie.
3. API returns `UserResponse` or `401`.

Logout:

1. React calls `/api/auth/logout`.
2. API clears auth cookie.
3. React clears local auth state.

## Testing Strategy

Manual testing is required for this learning phase:

1. Register a new user.
2. Register with an existing email.
3. Login with correct credentials.
4. Login with wrong password.
5. Call `/api/auth/me` while logged in.
6. Call `/api/auth/me` while logged out.
7. Logout.
8. Visit protected route while logged out.

Automated tests can be added after the first working API slice. Priority should be API integration tests around register/login/me/logout.

## Boundaries

Always:

- Use ASP.NET Core Identity for password hashing.
- Validate input on backend.
- Keep CORS limited to the local frontend origin.
- Run build/test commands before commit when the local SDK supports the target framework.
- Keep commits small and scoped.

Ask first:

- Changing target framework from `net10.0` to `net8.0`.
- Adding external dependencies beyond the confirmed auth stack.
- Moving backend folders into a different layout.
- Adding roles or authorization policies.

Never:

- Store plain text passwords.
- Implement custom password hashing.
- Commit real secrets or production connection strings.
- Store authentication tokens in localStorage for this cookie-auth phase.
- Add booking, court, host, payment, chat, review, notification, or role features in phase 1.

## Task Plan

| STT | Task | Backend | Frontend | Database | Result | Verify | Difficulty |
|---:|---|---|---|---|---|---|---|
| 1 | Save auth spec | None | None | None | Scope documented | Review `docs/auth-spec.md` | Easy |
| 2 | Backend scaffold/API shape | Convert existing MVC app toward API shape if needed | None | None | Backend project structure is ready | `dotnet build` when SDK supports target | Easy |
| 3 | PostgreSQL Docker Compose | Add DB config placeholders | None | PostgreSQL container | DB runs locally | `docker compose ps` | Medium |
| 4 | EF Core Identity setup | Add DbContext, user model, Identity config | None | Identity migration | Identity schema exists | `dotnet ef database update` | Medium |
| 5 | Auth DTOs | Add request/response DTOs | None | None | API contract exists | Build | Easy |
| 6 | Register API | Add `/api/auth/register` | None | User row created | User can register | Manual API request | Medium |
| 7 | Login/logout/me API | Add `/login`, `/logout`, `/me` | None | None | Cookie auth works | Manual API request | Medium |
| 8 | CORS/cookie config | Configure local frontend origin | None | None | Browser can call API with cookie | Browser/API check | Medium |
| 9 | Frontend scaffold | None | Add Vite React app | None | Frontend runs | `npm run dev` | Easy |
| 10 | Auth API client/context | None | Axios + AuthProvider | None | Auth state loads from `/me` | Browser check | Medium |
| 11 | Login/Register pages | None | Forms and error display | None | User can auth from UI | Manual browser check | Medium |
| 12 | ProtectedRoute/Navbar | None | Route guard and logout button | None | Protected route works | Manual browser check | Medium |

## Git Commit Plan

1. `docs: add authentication module spec`
2. `chore: prepare backend api project`
3. `chore: add postgres docker compose setup`
4. `feat: configure ef core identity`
5. `feat: add authentication dto contracts`
6. `feat: add registration endpoint`
7. `feat: add login logout and current user endpoints`
8. `chore: configure cors for frontend auth`
9. `chore: scaffold frontend react app`
10. `feat: add frontend auth state`
11. `feat: add login and register pages`
12. `feat: add protected route and navbar auth state`

## Success Criteria

- Register stores a user in PostgreSQL.
- Passwords are hashed by Identity.
- Login creates a valid cookie session.
- Logout clears the session.
- `/api/auth/me` returns the current user when authenticated.
- `/api/auth/me` returns `401` when unauthenticated.
- React pages show validation/auth errors.
- React can protect routes when unauthenticated.

## Open Questions

- Install .NET 10 SDK locally, or approve temporary target framework downgrade to `net8.0` for local verification?
