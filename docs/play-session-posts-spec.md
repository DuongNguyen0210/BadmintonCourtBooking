# Spec: BadmintonCourtBooking Play Session Posts

## Objective

Build the first feed feature for badminton walk-in play sessions.

After login, users should navigate to `/feed` and see a Facebook-like list of play session posts. Each post represents a badminton session looking for players. Users can open a post to view details.

In this phase, authenticated users can:

- View the play session feed.
- View play session post details.
- Create their own play session posts.
- Edit their own play session posts.
- Cancel their own play session posts so they no longer appear as active feed items.

This phase does not cover:

- Joining a play session.
- Cancelling a player registration.
- Deposits.
- Refund requests.
- Payment.
- Chat.
- Reviews.
- Notifications.
- Admin or host role authorization.

## Confirmed Decisions

- Code naming uses English.
- Main post entity name: `PlaySessionPost`.
- Main authenticated feed route: `/feed`.
- After login, frontend should navigate to `/feed`.
- `/dashboard` is reserved for future reporting and management views.
- `/host/dashboard` and `/admin/dashboard` can be added later for host/admin management.
- The current backend remains one ASP.NET Core Web API project.
- Backend target framework must remain `net10.0`.
- Authentication remains ASP.NET Core Identity cookie authentication.
- PostgreSQL remains the local database through Docker Compose.
- Delete behavior is soft delete through status changes, not hard delete.
- Current player counts and gender counts are manually entered in this phase.
- The cancel action label in Vietnamese is `Hủy bài đăng`.
- The feed only shows posts that are not full.
- Posts whose `EndTime` is in the past must not appear on the feed.

## Routes

Frontend routes for this phase:

| Route | Purpose |
|---|---|
| `/` | Landing page for logged-out users or redirect to `/feed` for logged-in users |
| `/feed` | Main authenticated feed |
| `/login` | Login page |
| `/register` | Register page |
| `/profile` | Future user profile page |
| `/play-sessions/create` | Create play session post |
| `/play-sessions/:id` | View play session post details |
| `/play-sessions/:id/edit` | Edit own play session post |

## Data Model

Entity:

```text
PlaySessionPost
```

Fields:

| Field | Type idea | Purpose |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `CreatorUserId` | `string` | Identity user id of the post creator |
| `Title` | `string` | Post title |
| `Description` | `string` | Post description |
| `CourtName` | `string` | Badminton court name |
| `CourtAddress` | `string` | Court address |
| `StartTime` | `DateTimeOffset` | Session start time |
| `EndTime` | `DateTimeOffset` | Session end time |
| `PricePerPlayer` | `decimal` | Fee per player |
| `MaxPlayers` | `int` | Maximum players |
| `CurrentPlayers` | `int` | Current joined/player count, manually entered in this phase |
| `MalePlayers` | `int` | Male player count, manually entered in this phase |
| `FemalePlayers` | `int` | Female player count, manually entered in this phase |
| `ShowMalePlayers` | `bool` | Whether male count is visible to viewers |
| `ShowFemalePlayers` | `bool` | Whether female count is visible to viewers |
| `Status` | `PostStatus` | Current post status |
| `CreatedAt` | `DateTimeOffset` | Created timestamp |
| `UpdatedAt` | `DateTimeOffset?` | Last updated timestamp |

Required TODO in code near player count fields:

```text
TODO: CurrentPlayers, MalePlayers, and FemalePlayers are manually entered in this phase.
Later, calculate these values from play session registration records.
```

Status enum:

```csharp
public enum PostStatus
{
    Active,
    Filled,
    Completed,
    Cancelled,
    Expired
}
```

Status meanings:

| Status | Meaning |
|---|---|
| `Active` | The post is visible on the active feed and still looking for players |
| `Filled` | The session has enough players |
| `Completed` | The session already happened |
| `Cancelled` | The creator cancelled the post/session |
| `Expired` | The session time has passed |

## API Endpoints

All endpoints in this phase require authentication.

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `/api/play-sessions` | Get active feed posts |
| `GET` | `/api/play-sessions/{id}` | Get post details |
| `POST` | `/api/play-sessions` | Create a post |
| `PUT` | `/api/play-sessions/{id}` | Edit own post |
| `DELETE` | `/api/play-sessions/{id}` | Soft-delete/cancel own post |

Delete endpoint behavior:

- It must not remove the database row.
- It should set `Status = Cancelled`.
- Cancelled posts should not appear in the normal active feed.
- The UI should avoid the label "Delete" because the actual behavior is cancellation.

Confirmed UI label:

```text
Hủy bài đăng
```

Feed endpoint behavior:

- Only show posts with `Status = Active`.
- Hide posts with `Status = Filled`.
- Hide posts with `Status = Completed`.
- Hide posts with `Status = Cancelled`.
- Hide posts with `Status = Expired`.
- Hide posts where `CurrentPlayers >= MaxPlayers`, even if the status has not been updated to `Filled` yet.
- Hide posts where `EndTime` is in the past.

## DTOs

Backend request DTOs:

- `CreatePlaySessionPostRequest`
- `UpdatePlaySessionPostRequest`

Backend response DTOs:

- `PlaySessionPostListItemResponse`
- `PlaySessionPostDetailResponse`

Frontend types:

- `CreatePlaySessionPostRequest`
- `UpdatePlaySessionPostRequest`
- `PlaySessionPostListItem`
- `PlaySessionPostDetail`
- `PostStatus`

## Validation Rules

Backend validation must enforce:

- `Title` is required.
- `Title` has a reasonable maximum length.
- `CourtName` is required.
- `CourtAddress` is required.
- `StartTime` must be earlier than `EndTime`.
- `PricePerPlayer >= 0`.
- `MaxPlayers > 0`.
- `CurrentPlayers >= 0`.
- `CurrentPlayers <= MaxPlayers`.
- `MalePlayers >= 0`.
- `FemalePlayers >= 0`.
- `MalePlayers + FemalePlayers <= CurrentPlayers`.

Authorization rules:

- Only authenticated users can view, create, edit, or cancel posts.
- Only the creator can edit a post.
- Only the creator can cancel a post.
- A non-creator editing or cancelling a post should receive `403 Forbidden`.

## Commands

Backend build:

```powershell
dotnet build .\BadmintonCourtBooking.sln
```

Backend run:

```powershell
dotnet run --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
```

Database:

```powershell
docker compose up -d
docker compose ps
```

EF Core migration:

```powershell
dotnet ef migrations add AddPlaySessionPosts --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
dotnet ef database update --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
```

Frontend build:

```powershell
cd .\frontend
npm.cmd run build
```

Frontend dev:

```powershell
cd .\frontend
npm.cmd run dev
```

## Project Structure

Expected files and folders:

```text
BadmintonCourtBooking/
  BadmintonCourtBooking/
    Controllers/
      PlaySessionsController.cs
    Data/
      ApplicationDbContext.cs
    Dtos/
      PlaySessions/
        CreatePlaySessionPostRequest.cs
        UpdatePlaySessionPostRequest.cs
        PlaySessionPostListItemResponse.cs
        PlaySessionPostDetailResponse.cs
    Models/
      PlaySessionPost.cs
      PostStatus.cs
    Migrations/
  frontend/
    src/
      api/
        playSessions.ts
      components/
        play-sessions/
          PlaySessionCard.tsx
      pages/
        FeedPage.tsx
        CreatePlaySessionPage.tsx
        EditPlaySessionPage.tsx
        PlaySessionDetailPage.tsx
      types/
        playSession.ts
  docs/
    auth-spec.md
    play-session-posts-spec.md
```

## Code Style

Backend DTO style:

```csharp
using System.ComponentModel.DataAnnotations;

namespace BadmintonCourtBooking.Dtos.PlaySessions;

public sealed class CreatePlaySessionPostRequest
{
    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CourtName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string CourtAddress { get; set; } = string.Empty;
}
```

Frontend type style:

```ts
export type PlaySessionPostListItem = {
  id: string
  title: string
  courtName: string
  courtAddress: string
  startTime: string
  endTime: string
  pricePerPlayer: number
  maxPlayers: number
  currentPlayers: number
  malePlayers?: number
  femalePlayers?: number
  status: PostStatus
}
```

Conventions:

- Use English names in code.
- Keep UI text user-facing and simple.
- Keep backend validation authoritative.
- Keep frontend validation helpful but not trusted.
- Do not introduce Clean Architecture in this phase.
- Do not add payment, booking, registration, refund, host, or admin logic in this phase.

## Testing Strategy

Manual API checks:

1. Create a post while logged in.
2. Try creating a post while logged out and expect `401`.
3. Get the active feed.
4. Confirm full posts do not appear in the active feed.
5. Confirm posts whose `EndTime` is in the past do not appear in the active feed.
6. Get post details.
7. Edit own post.
8. Try editing another user's post and expect `403`.
9. Cancel own post.
10. Confirm cancelled post no longer appears in the active feed.

Manual browser checks:

1. Login redirects to `/feed`.
2. `/feed` shows post cards.
3. Create page creates a post and returns to feed or detail.
4. Clicking a card opens details.
5. Creator can see edit/cancel actions.
6. Non-creator should not see edit/cancel actions.
7. Full posts disappear from normal feed.
8. Posts whose `EndTime` is in the past disappear from normal feed.
9. Cancelled post disappears from normal feed.

Build checks:

```powershell
dotnet build .\BadmintonCourtBooking.sln
cd .\frontend
npm.cmd run build
```

Automated tests can be added after the first working vertical slice. Priority should be backend integration tests for authorization and validation.

## Boundaries

Always:

- Keep the feature authentication-only plus play session post CRUD.
- Require login for play session post endpoints.
- Validate inputs on the backend.
- Keep ownership checks on edit/cancel.
- Keep `net10.0`.
- Use PostgreSQL through EF Core migrations.
- Run backend and frontend builds before commits when possible.
- Commit in small task-based commits.

Ask first:

- Adding a new dependency.
- Changing the route structure.
- Changing database delete behavior.
- Adding registration/joining behavior.
- Adding payment or deposit fields.
- Adding roles or host/admin authorization.
- Moving backend folders.

Never:

- Hard delete play session posts in this phase.
- Add booking/payment/refund implementation in this phase.
- Store secrets in git.
- Store auth tokens in localStorage or sessionStorage.
- Downgrade backend from `net10.0`.

## Task Plan

| STT | Task | Backend | Frontend | Database | Result | Verify | Difficulty |
|---:|---|---|---|---|---|---|---|
| 1 | Save and review spec | None | None | None | Scope documented | Review this file | Easy |
| 2 | Add data model and migration | Add `PlaySessionPost`, `PostStatus`, DbContext config | None | Add migration/table | Database can store posts | `dotnet build`, `dotnet ef database update` | Medium |
| 3 | Add create/list/detail APIs | Add controller endpoints and DTOs | None | Read/write posts | API can create and read posts | Manual API checks | Medium |
| 4 | Add edit/cancel APIs | Add owner checks and soft delete | None | Update post status | Creator can edit/cancel only own posts | Manual API checks | Medium |
| 5 | Add frontend API types/client | None | Add `playSession` types and API functions | None | React can call post APIs | `npm.cmd run build` | Easy |
| 6 | Add `/feed` page | None | Render active post cards | None | User sees feed after login | Browser check | Medium |
| 7 | Add create page | None | Add create form | None | User can create post from UI | Browser check | Medium |
| 8 | Add detail page | None | Add detail route/page | None | User can inspect a post | Browser check | Medium |
| 9 | Add edit/cancel UI | None | Add edit form and cancel action for creator | None | Creator can update/cancel post | Browser check | Medium |
| 10 | Cleanup and verification | Minimal cleanup only | Minimal cleanup only | None | Builds pass and scope is clean | Backend/frontend build | Easy |

## Git Commit Plan

```text
docs: add play session posts spec
feat: add play session post data model
feat: add play session post create and read APIs
feat: add play session post edit and cancel APIs
feat: add play session frontend API client
feat: add feed page for play session posts
feat: add create play session post page
feat: add play session detail page
feat: add edit and cancel play session UI
```

## Success Criteria

- Logged-in users land on `/feed` after login.
- Logged-in users can view active play session posts.
- Logged-in users can view play session post details.
- Logged-in users can create posts.
- Creators can edit their own posts.
- Creators can cancel their own posts.
- Non-creators cannot edit or cancel other users' posts.
- Cancelled posts do not appear in the active feed.
- Full posts do not appear in the active feed.
- Posts whose `EndTime` is in the past do not appear in the active feed.
- The database keeps cancelled posts for history.
- Backend and frontend builds pass.

## Open Questions

None for the first implementation slice.
