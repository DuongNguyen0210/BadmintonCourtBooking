# Spec: Join Requests, Wallet Escrow, Cancellations, and Notifications

## Objective

Build the next business slice for BadmintonCourtBooking: a player can request to join a `PlaySessionPost`, the host can approve or reject the request, the player can pay using an in-system wallet balance, the system holds money in escrow, notifications are persisted, and the player can cancel participation with clear refund rules.

This spec intentionally keeps the current entity name `PlaySessionPost` for this phase. Renaming it to `PlaySession` is technical debt for a later refactor after the join/payment slice is stable.

This phase must keep controllers thin. Controllers should only:

- Receive request DTOs.
- Resolve the current authenticated user id.
- Call application services.
- Convert service results to HTTP responses.

Business rules must live in domain entities, policies, and application services.

## Confirmed Decisions

- Keep `PlaySessionPost` as the current session aggregate name.
- Add `PricePerPlayerVnd` as `long`.
- `PricePerPlayerVnd` is the only official source for payment, refund, cancellation fee, escrow, and new UI price display.
- Existing `PricePerPlayer decimal` is deprecated compatibility data only.
- New business logic must never use `PricePerPlayer`.
- Add a new xUnit test project named `BadmintonCourtBooking.Tests`.
- Organize tests under `Unit/` and `Integration/`.
- Do not commit or push automatically.
- Do not integrate a real bank or payment provider in this phase.
- Add a development-only fake top-up flow for the current authenticated user only.
- Development top-up must not accept a target `UserId` from frontend.
- Notifications are persisted in PostgreSQL. SignalR is out of scope.
- Payment approval uses a configurable 15 minute window through `PaymentOptions`.
- Standard cancellation refund uses integer math: refund is floored and the rounding remainder belongs to the host cancellation fee.
- `WalletTransaction` is the audit ledger, while wallet balances are summary fields updated in the same transaction.
- Standard cancellation transfers the host fee into the host available balance after the cancellation transaction completes successfully.
- Active participants may cancel from a `Filled` session if the session has not started, the session is not cancelled, the participant is active, and there is no completed cancellation.

## Current Codebase Summary

Backend:

- ASP.NET Core / .NET 10 Web API project at `BadmintonCourtBooking/`.
- Authentication uses ASP.NET Core Identity cookie auth.
- PostgreSQL is configured through EF Core and Npgsql.
- `ApplicationUser` currently contains `FullName`.
- `ApplicationDbContext` inherits from `IdentityDbContext<ApplicationUser>`.
- Current play session entity is `PlaySessionPost`.
- Current feed is implemented in `PlaySessionsController.GetFeed`.
- `PlaySessionsController` currently contains query, mapping, create, update, and cancel logic directly.
- There is no service layer yet.
- There are no wallet, notification, join request, participant, or cancellation entities yet.

Frontend:

- React + TypeScript + Vite.
- Axios client uses `withCredentials: true`.
- Existing routes include `/feed`, `/play-sessions/create`, `/play-sessions/:id`, and `/play-sessions/:id/edit`.
- Existing auth state is in `AuthProvider` / `useAuth`.
- There are no wallet, join request, host approval, cancellation, or notification pages yet.

Known issue:

- Some existing Vietnamese text appears encoded incorrectly in source files. New UI work should preserve valid UTF-8 text or use clear ASCII constants where appropriate.

## Tech Stack

Backend:

- ASP.NET Core Web API
- .NET 10 target
- ASP.NET Core Identity cookie auth
- Entity Framework Core
- Npgsql.EntityFrameworkCore.PostgreSQL
- PostgreSQL

Frontend:

- React
- TypeScript
- Vite
- Tailwind CSS
- React Router
- Axios with credentials

Testing:

- Add `BadmintonCourtBooking.Tests`.
- Prefer built-in .NET test stack plus minimal necessary packages.
- Unit tests for domain transitions, policies, and wallet calculations.
- Integration tests for API/service behavior where database transactions matter.

## Commands

Backend build:

```powershell
dotnet build .\BadmintonCourtBooking.sln
```

Backend build if Debug output is locked by a running server:

```powershell
dotnet build .\BadmintonCourtBooking.sln -c Release
```

Backend tests:

```powershell
dotnet test .\BadmintonCourtBooking.sln
```

Run backend:

```powershell
dotnet run --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
```

Database:

```powershell
docker compose up -d
docker compose ps
```

EF migration:

```powershell
dotnet tool run dotnet-ef migrations add AddJoinWalletNotifications --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj --startup-project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
dotnet tool run dotnet-ef database update --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj --startup-project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
```

Frontend:

```powershell
cd .\frontend
npm.cmd run build
npm.cmd run lint
npm.cmd run dev
```

## Project Structure

Target backend structure for this phase:

```text
BadmintonCourtBooking/
  Controllers/
    PlaySessionsController.cs
    JoinRequestsController.cs
    HostJoinRequestsController.cs
    ParticipationsController.cs
    WalletController.cs
    NotificationsController.cs
    DevelopmentWalletController.cs
  Data/
    ApplicationDbContext.cs
  Dtos/
    JoinRequests/
    Participations/
    Wallet/
    Notifications/
    Host/
    Development/
  Models/
    PlaySessionPost.cs
    PlaySessionJoinRequest.cs
    JoinRequestStatus.cs
    PlaySessionParticipant.cs
    ParticipantStatus.cs
    Wallet.cs
    WalletTransaction.cs
    WalletTransactionType.cs
    WalletTransactionStatus.cs
    ParticipationCancellation.cs
    CancellationRefundChoice.cs
    CancellationStatus.cs
    Notification.cs
    NotificationType.cs
  Options/
    CancellationPolicyOptions.cs
    PaymentOptions.cs
  Services/
    IClock.cs
    SystemClock.cs
    IJoinRequestService.cs
    JoinRequestService.cs
    IWalletService.cs
    WalletService.cs
    IPaymentService.cs
    PaymentService.cs
    ICancellationService.cs
    CancellationService.cs
    ICancellationPolicy.cs
    CancellationPolicy.cs
    INotificationService.cs
    NotificationService.cs
    IPaymentGateway.cs
    DevelopmentPaymentGateway.cs
    ServiceResult.cs
```

Target test structure:

```text
BadmintonCourtBooking.Tests/
  Unit/
    PlaySessionJoinRequestTests.cs
    CancellationPolicyTests.cs
    JoinRequestServiceTests.cs
    WalletServiceTests.cs
    PaymentServiceTests.cs
    CancellationServiceTests.cs
  Integration/
    JoinRequestFlowTests.cs
    WalletFlowTests.cs
```

Target frontend structure:

```text
frontend/src/
  api/
    joinRequests.ts
    wallet.ts
    notifications.ts
    hostJoinRequests.ts
  types/
    joinRequest.ts
    wallet.ts
    notification.ts
  pages/
    MyJoinRequestsPage.tsx
    HostJoinRequestsPage.tsx
    WalletPage.tsx
    NotificationsPage.tsx
  components/
    join-requests/
    wallet/
    notifications/
```

## Domain Model

### PlaySessionPost

Existing entity. This phase extends it.

New or changed fields:

```csharp
public long PricePerPlayerVnd { get; private set; }
```

Compatibility:

- Existing `PricePerPlayer decimal` must be treated as deprecated.
- New payment and refund code must only use `PricePerPlayerVnd`.
- If the old database column remains for compatibility, application code must keep it synchronized from `PricePerPlayerVnd`.
- Frontend should move to `pricePerPlayerVnd`.
- A later migration can remove or convert the old `PricePerPlayer` column after all consumers are updated.

Feed availability must not depend on a manually stored `IsFull`.

Occupied slots:

```text
OccupiedSlots =
  CurrentPlayers
  + Active online participants
  + AwaitingPayment join requests whose PaymentDueAtUtc has not passed
```

The existing `CurrentPlayers` field remains the offline/manual player count for now.

Feed shows a post when:

- `Status == Active`
- `StartTime > now`
- `OccupiedSlots < MaxPlayers`

### PlaySessionJoinRequest

Fields:

```text
Id
PlaySessionPostId
GuestUserId
Status
RequestedAtUtc
ReviewedAtUtc
ReviewedByHostId
PaymentDueAtUtc
PaidAtUtc
CancelledAtUtc
CreatedAtUtc
UpdatedAtUtc
```

Status:

```csharp
public enum JoinRequestStatus
{
    PendingHostApproval,
    AwaitingPayment,
    Joined,
    Rejected,
    Cancelled,
    Expired
}
```

Required behavior methods:

```csharp
Approve(string hostUserId, DateTimeOffset now, DateTimeOffset paymentDueAtUtc)
Reject(string hostUserId, DateTimeOffset now)
MarkAsPaid(DateTimeOffset now)
Expire(DateTimeOffset now)
Cancel(DateTimeOffset now)
```

State transition rules:

```text
PendingHostApproval -> AwaitingPayment
PendingHostApproval -> Rejected
PendingHostApproval -> Cancelled
AwaitingPayment -> Joined
AwaitingPayment -> Expired
AwaitingPayment -> Cancelled
```

No controller may assign `Status` directly.

### PlaySessionParticipant

Fields:

```text
Id
PlaySessionPostId
UserId
JoinRequestId
Status
JoinedAtUtc
CancelledAtUtc
```

Status:

```csharp
public enum ParticipantStatus
{
    Active,
    Cancelled
}
```

Participant rows are never hard deleted.

### Wallet

Fields:

```text
Id
UserId
AvailableBalanceVnd
HeldBalanceVnd
CreatedAtUtc
UpdatedAtUtc
ConcurrencyToken
```

Rules:

- Frontend never sends a new balance.
- Wallet service controls balance changes.
- Use optimistic concurrency token and database transactions.
- Amounts are `long` VND.
- Available balance cannot go below 0.
- Held balance cannot go below 0.

### WalletTransaction

Immutable ledger entry. Never update or delete completed entries.

Fields:

```text
Id
UserId
RelatedUserId
PlaySessionPostId
JoinRequestId
CancellationId
Type
Status
AmountVnd
BalanceBeforeVnd
BalanceAfterVnd
Description
CreatedAtUtc
IdempotencyKey
```

Type:

```csharp
public enum WalletTransactionType
{
    TopUp,
    EscrowHold,
    EscrowReleaseToHost,
    Refund,
    CancellationFee,
    FullRefund,
    ManualCompensation,
    Reversal
}
```

Status:

```csharp
public enum WalletTransactionStatus
{
    Completed,
    Reversed,
    Failed
}
```

Idempotency:

- `IdempotencyKey` should be unique where present.
- Payment confirmation should use a deterministic idempotency key based on join request id.
- Manual compensation should prevent total compensation for a participant from exceeding original paid amount.

### ParticipationCancellation

Fields:

```text
Id
ParticipantId
RequestedByUserId
RefundChoice
OriginalAmountVnd
RefundAmountVnd
CancellationFeeVnd
Status
Reason
CreatedAtUtc
CompletedAtUtc
```

Refund choice:

```csharp
public enum CancellationRefundChoice
{
    StandardRefund,
    WaiveRefund
}
```

Status:

```csharp
public enum CancellationStatus
{
    Completed,
    Reversed
}
```

### Notification

Fields:

```text
Id
RecipientUserId
Type
Title
Message
RelatedEntityId
IsRead
CreatedAtUtc
ReadAtUtc
```

Type:

```csharp
public enum NotificationType
{
    JoinRequested,
    JoinApproved,
    JoinRejected,
    PaymentRequired,
    PaymentCompleted,
    ParticipantJoined,
    JoinRequestExpired,
    ParticipantCancelled,
    RefundCompleted,
    NoRefundCancellation,
    HostCancelledSession,
    ManualCompensationReceived
}
```

## Business Flows

### Request to Join

1. Authenticated guest opens a post.
2. Guest clicks "Request to join".
3. Backend loads the session from database.
4. Backend rejects if guest is the host.
5. Backend rejects if session is cancelled, full, or already started.
6. Backend rejects if the guest already has an active request or active participant.
7. Backend creates `PlaySessionJoinRequest` with `PendingHostApproval`.
8. Backend creates notification for host.
9. API returns `201 Created`.

Active join request statuses:

```text
PendingHostApproval
AwaitingPayment
Joined
```

### Host Approval

Approve:

1. Only host can approve.
2. Request must be `PendingHostApproval`.
3. Session must still be active and not started.
4. Occupied slots must be lower than `MaxPlayers`.
5. Request changes to `AwaitingPayment`.
6. `PaymentDueAtUtc = now + PaymentOptions.PaymentWindow`.
7. Create notification for guest.

Reject:

1. Only host can reject.
2. Request must be `PendingHostApproval`.
3. Request changes to `Rejected`.
4. Create notification for guest.

### Payment

Payment confirmation:

1. Guest calls confirm payment.
2. Backend loads request, session, guest wallet, and relevant participants.
3. Request must belong to current user.
4. Request must be `AwaitingPayment`.
5. `PaymentDueAtUtc` must be in the future.
6. Session must be active and not started.
7. Occupied slots must be lower than `MaxPlayers`.
8. Amount is read from `PlaySessionPost.PricePerPlayerVnd`.
9. Wallet available balance must be enough.
10. In one database transaction:
    - deduct guest available balance
    - increase escrow/held balance or record escrow liability
    - create immutable wallet transaction
    - create active participant
    - mark join request as paid/joined
    - create notifications for guest and host
11. Commit transaction.

No case may exist where money is deducted but participant is not created, or participant is created but money is not deducted.

### Payment Expiry

When an awaiting payment request passes `PaymentDueAtUtc`:

- Request changes to `Expired`.
- Held slot is released.
- Guest receives notification.
- Feed availability automatically reflects the released slot.

Implementation approach for first phase:

- Expire stale requests opportunistically in service methods that read feed, join requests, or payment.
- A background job can be added later.

### Standard Player Cancellation

Allowed only before session start.

Refund policy:

- Refund 90% to player.
- Transfer 10% to host as cancellation fee.
- No host approval is required.

In one transaction:

1. Load participant, session, original payment transaction, wallets.
2. Validate participant belongs to current user.
3. Validate participant is active.
4. Validate session has not started.
5. Calculate refund and fee through `ICancellationPolicy`.
6. Move money from escrow/held into player refund and host fee.
7. Write ledger entries.
8. Create `ParticipationCancellation`.
9. Mark participant `Cancelled`.
10. Create notifications.
11. Commit.

### Waive Refund Cancellation

Allowed only before session start.

Frontend confirmation requirements:

- Show exact amount forfeited.
- Require checkbox.
- Require text confirmation: `KHONG HOAN TIEN`.
- Disable final button until both are valid.

Backend requirements:

- Require `refundChoice = WaiveRefund`.
- Require `waiveRefundConfirmation = "KHONG HOAN TIEN"`.
- Do not rely only on frontend validation.

In one transaction:

1. Move all escrow for that participant to host.
2. Write ledger entries.
3. Create `ParticipationCancellation` with refund 0 and fee original amount.
4. Mark participant cancelled.
5. Create notifications.
6. Commit.

### Manual Compensation

After a waive refund cancellation, host may voluntarily compensate the player.

Rules:

- Only the host of the session can perform it.
- Amount must be greater than 0.
- Amount must not exceed the player's original paid amount.
- Total manual compensation for that participant must not exceed the original paid amount.
- Transfer is from host available balance to player available balance.
- Write immutable ledger entries.
- Notify player.
- Must be idempotent or guarded against double submit.

### Host Cancels Session

When host cancels a session:

1. Only host can cancel.
2. Transaction starts.
3. All paid active participants are refunded 100%.
4. Pending or awaiting-payment join requests become `Cancelled`.
5. Session status becomes `Cancelled`.
6. Notifications are created for affected users.
7. Commit transaction.

No cancellation fee is charged to players.

## Money and Escrow Model

The platform must not immediately move player money into host available balance.

Recommended first implementation:

- Player wallet has `AvailableBalanceVnd`.
- Host wallet has `AvailableBalanceVnd`.
- Wallet has `HeldBalanceVnd` as a summary/display balance for escrow.
- `WalletTransaction` is the primary audit/history source for every money movement.
- When guest pays, deduct from guest available and record `EscrowHold`.
- On successful completion or host compensation/cancellation flows, release escrow to host or refund player.
- Ledger entries and wallet balance updates must happen in the same database transaction.
- Wallet balances must not be updated without corresponding ledger entries.
- Add tests or reconciliation checks to verify wallet summaries match ledger history.

Important invariant:

```text
Money held for a session must remain available to refund until it is explicitly released.
```

## Cancellation Policy

Options:

```csharp
public sealed class CancellationPolicyOptions
{
    public decimal RefundRate { get; init; } = 0.90m;
}
```

Implementation must use integer VND math for the actual amount calculation:

```csharp
var refundAmountVnd = originalAmountVnd * 90 / 100;
var cancellationFeeVnd = originalAmountVnd - refundAmountVnd;
```

Policy interface:

```csharp
public interface ICancellationPolicy
{
    CancellationQuote Quote(long originalAmountVnd);
}
```

Rules:

- Do not use `float` or `double`.
- Use `long` VND for resulting amounts.
- Refund is rounded down by integer division.
- Any rounding remainder belongs to the host cancellation fee.
- `RefundAmountVnd + CancellationFeeVnd == OriginalAmountVnd`.
- For standard cancellation, the cancellation fee moves into the host `AvailableBalanceVnd` inside the same completed transaction.

Payment window options:

```csharp
public sealed class PaymentOptions
{
    public int PaymentWindowMinutes { get; init; } = 15;
}
```

Host approval uses:

```csharp
PaymentDueAtUtc = clock.UtcNow.AddMinutes(paymentOptions.PaymentWindowMinutes);
```

## API Contract

All endpoints require authentication except development-only endpoints as noted. Development endpoints still require authentication unless explicitly decided otherwise.

### Player APIs

```http
POST /api/play-sessions/{playSessionPostId}/join-requests
GET  /api/join-requests/mine
POST /api/join-requests/{joinRequestId}/confirm-payment
POST /api/participations/{participantId}/cancel
GET  /api/wallet
GET  /api/wallet/transactions
GET  /api/notifications
PATCH /api/notifications/{notificationId}/read
```

Cancel participation request:

```json
{
  "refundChoice": "StandardRefund",
  "reason": "Cannot join",
  "waiveRefundConfirmation": null
}
```

Waive refund request:

```json
{
  "refundChoice": "WaiveRefund",
  "reason": "Plan changed",
  "waiveRefundConfirmation": "KHONG HOAN TIEN"
}
```

### Host APIs

```http
GET  /api/host/join-requests?status=PendingHostApproval
POST /api/host/join-requests/{joinRequestId}/approve
POST /api/host/join-requests/{joinRequestId}/reject
POST /api/host/cancellations/{cancellationId}/manual-compensation
```

Manual compensation request:

```json
{
  "amountVnd": 50000,
  "note": "Voluntary compensation after discussion"
}
```

### Development APIs

Only mapped when `app.Environment.IsDevelopment()`.

```http
POST /api/development/wallet/top-up
```

Request:

```json
{
  "amountVnd": 200000
}
```

Rules:

- Must not be available in Production.
- Requires authentication.
- Uses the current authenticated user as the wallet owner.
- Must not accept `UserId` or any target user selector from frontend.
- Does not store card or bank data.
- Creates `TopUp` ledger transaction.

## Response and Error Contract

Use consistent error responses.

Preferred shape:

```json
{
  "code": "INSUFFICIENT_BALANCE",
  "message": "Wallet balance is not enough to pay for this session."
}
```

HTTP status:

| Status | Use |
|---|---|
| `200 OK` | Successful operation with response |
| `201 Created` | Join request created |
| `204 No Content` | Successful mutation with no response body |
| `400 Bad Request` | Invalid DTO |
| `401 Unauthorized` | Not logged in |
| `403 Forbidden` | Logged in but not allowed |
| `404 Not Found` | Entity missing |
| `409 Conflict` | Invalid state, duplicate request, full session, already paid, concurrency conflict |

Error codes:

```text
SESSION_NOT_FOUND
SESSION_NOT_ACTIVE
SESSION_ALREADY_STARTED
SESSION_FULL
HOST_CANNOT_JOIN_OWN_SESSION
DUPLICATE_ACTIVE_JOIN_REQUEST
ALREADY_PARTICIPANT
JOIN_REQUEST_NOT_FOUND
JOIN_REQUEST_NOT_PENDING
JOIN_REQUEST_NOT_AWAITING_PAYMENT
JOIN_REQUEST_PAYMENT_EXPIRED
INSUFFICIENT_BALANCE
PAYMENT_ALREADY_COMPLETED
PARTICIPANT_NOT_FOUND
PARTICIPANT_ALREADY_CANCELLED
CANNOT_CANCEL_AFTER_START
INVALID_REFUND_CHOICE
WAIVE_REFUND_CONFIRMATION_REQUIRED
UNAUTHORIZED_NOTIFICATION_ACCESS
UNAUTHORIZED_WALLET_ACCESS
MANUAL_COMPENSATION_EXCEEDS_LIMIT
DEVELOPMENT_ENDPOINT_DISABLED
CONCURRENCY_CONFLICT
```

## Authorization Rules

Required:

1. Only authenticated users can create join requests.
2. Host cannot join their own session.
3. A user cannot have more than one active join request for the same session.
4. A user cannot become active participant twice for the same session.
5. Only the session host can approve or reject join requests.
6. Only the guest can pay for their own join request.
7. Only the participant owner can cancel their participation.
8. Only the host can manually compensate a cancellation for their own session.
9. Users can only view their own wallet.
10. Users can only view and mark read their own notifications.
11. Do not trust user ids, host ids, prices, refund amounts, or balances from frontend when they can be derived from database and current user.

## Transaction and Concurrency

Payment transaction must:

1. Read join request.
2. Validate status and payment deadline.
3. Read session.
4. Validate session state and start time.
5. Validate remaining slot count.
6. Read guest wallet.
7. Validate balance.
8. Write escrow ledger entry.
9. Update wallet balances.
10. Create participant.
11. Mark request joined.
12. Create notifications.
13. Commit.

Cancellation transaction must:

1. Read participant and original payment.
2. Validate active participant and start time.
3. Calculate refund through policy.
4. Update wallet/escrow balances.
5. Write ledger entries.
6. Create cancellation record.
7. Mark participant cancelled.
8. Create notifications.
9. Commit.

Concurrency requirements:

- Wallet uses optimistic concurrency token.
- Payment service must handle `DbUpdateConcurrencyException`.
- Slot count must be checked inside the transaction.
- A deterministic idempotency key should prevent double payment.
- Unique indexes should prevent duplicate active participation where database support makes it practical.

Candidate indexes:

```text
Notifications: RecipientUserId, IsRead
Notifications: RecipientUserId, CreatedAtUtc
JoinRequests: PlaySessionPostId, Status
JoinRequests: GuestUserId, Status
Participants: PlaySessionPostId, Status
Participants: UserId, Status
WalletTransactions: UserId, CreatedAtUtc
WalletTransactions: IdempotencyKey unique where not null
Wallets: UserId unique
```

Application-level guards are still required even when indexes exist.

## Frontend Scope

### Feed and Detail

Add:

- "Request to join" button.
- Current user's join request status.
- Disable repeated request clicks.
- Hide request button for host.
- Disable request button when full.

### My Join Requests

Route:

```text
/join-requests
```

Show:

- Pending host approval.
- Awaiting payment.
- Payment due time.
- Joined.
- Rejected.
- Expired.
- Cancelled.

When awaiting payment, show:

- Court name.
- Time.
- Price.
- Current wallet balance.
- Balance after payment.
- Pay button.

### Host Join Requests

Route:

```text
/host/join-requests
```

Host can:

- View pending requests.
- See basic guest information.
- Approve.
- Reject.
- Avoid double submit while request is in progress.

### Wallet

Route:

```text
/wallet
```

Show:

- Available balance.
- Held/escrow balance.
- Transaction history.
- Development-only fake top-up button when frontend env indicates development.

### Notifications

Route:

```text
/notifications
```

Show:

- Unread count.
- Notification list.
- Mark as read.
- Links to related session, request, or wallet where possible.
- Refresh on page open. Polling can be added later.

### Participation Cancellation

Add modal for joined participants:

Option A:

- Standard refund.
- Show refund amount 90%.
- Show cancellation fee 10%.

Option B:

- Waive refund.
- Show exact amount forfeited.
- Require checkbox.
- Require text confirmation: `KHONG HOAN TIEN`.

After cancellation:

- Refresh join request/participant UI.
- Refresh wallet.
- Feed must reflect newly available slot after reload.

## Code Style

Domain entity style:

```csharp
public sealed class PlaySessionJoinRequest
{
    public JoinRequestStatus Status { get; private set; }

    public void Approve(string hostUserId, DateTimeOffset now, DateTimeOffset paymentDueAtUtc)
    {
        if (Status != JoinRequestStatus.PendingHostApproval)
            throw new DomainException("JOIN_REQUEST_NOT_PENDING", "Only pending requests can be approved.");

        Status = JoinRequestStatus.AwaitingPayment;
        ReviewedAtUtc = now;
        ReviewedByHostId = hostUserId;
        PaymentDueAtUtc = paymentDueAtUtc;
        UpdatedAtUtc = now;
    }
}
```

Service style:

```csharp
public sealed class JoinRequestService(ApplicationDbContext dbContext, IClock clock)
    : IJoinRequestService
{
    public async Task<ServiceResult<JoinRequestResponse>> RequestToJoinAsync(
        Guid playSessionPostId,
        string guestUserId,
        CancellationToken cancellationToken)
    {
        // Load data, enforce rules, create entity, save.
    }
}
```

Controller style:

```csharp
[Authorize]
[HttpPost("{playSessionPostId:guid}/join-requests")]
public async Task<IActionResult> RequestToJoin(Guid playSessionPostId, CancellationToken cancellationToken)
{
    var result = await joinRequestService.RequestToJoinAsync(
        playSessionPostId,
        GetCurrentUserId(),
        cancellationToken);

    return ToActionResult(result);
}
```

Frontend API style:

```ts
export async function requestToJoin(playSessionPostId: string) {
  const response = await apiClient.post<JoinRequestDetail>(
    `/api/play-sessions/${playSessionPostId}/join-requests`,
  )
  return response.data
}
```

## Testing Strategy

Test project:

```text
BadmintonCourtBooking.Tests
```

Framework and structure:

- Use xUnit.
- Put domain and policy tests under `Unit/`.
- Put API, EF Core, authorization, transaction, and concurrency tests under `Integration/`.
- Prefer PostgreSQL-backed integration tests because production uses PostgreSQL.
- Do not rely only on EF Core InMemory for transaction/concurrency behavior.

Unit tests:

- Domain state transitions.
- Cancellation policy.
- Wallet balance calculations.
- Idempotency key behavior where pure enough to test.

Integration tests:

- Service/API flows that require EF Core and transactions.
- PostgreSQL-backed tests are preferred for transaction/concurrency behavior.
- If local integration setup is too heavy for the first slice, start with service tests and add PostgreSQL integration tests before payment/cancellation is considered complete.

Required test cases:

1. Guest sends join request successfully.
2. Duplicate active join request is rejected.
3. Different host cannot approve request.
4. Host approve changes status to `AwaitingPayment`.
5. Expired request releases held slot.
6. Insufficient balance does not deduct money and does not create participant.
7. Successful payment creates participant and escrow ledger.
8. Double payment does not deduct twice.
9. Two users paying for final slot results in one success.
10. Standard cancellation refunds 90% and transfers 10%.
11. Waive refund requires valid confirmation.
12. Cannot cancel twice.
13. Host cancellation refunds 100%.
14. After participant cancellation, previously full feed item appears again.
15. Manual compensation cannot exceed original paid amount.
16. User cannot view or mutate another user's notification, wallet, or request.
17. Development top-up is not available in Production.

## Boundaries

Always:

- Keep `net10.0`.
- Use ASP.NET Core Identity cookie auth.
- Use `PricePerPlayerVnd` for all new money logic.
- Keep money as `long` VND in wallet and transaction entities.
- Use transactions for payment and cancellation.
- Persist notifications.
- Keep controllers thin.
- Validate frontend input again on backend.
- Check ownership/authorization on every protected operation.
- Run backend build, backend tests, frontend build, and frontend lint before reporting implementation done.

Ask first:

- Adding new runtime dependencies.
- Renaming `PlaySessionPost`.
- Removing the old `PricePerPlayer` database column.
- Introducing real payment integration.
- Adding SignalR.
- Changing auth strategy.
- Changing route conventions significantly.

Never:

- Trust frontend price, refund amount, host id, guest id, balance, or ownership.
- Store bank card data.
- Enable development top-up in Production.
- Hard delete participants, wallet transactions, join requests, or notifications.
- Put payment/cancellation business logic directly in controllers.
- Use `float` or `double` for money.
- Commit secrets.

## Implementation Plan

1. Create and approve this spec.
2. Add test project and baseline test infrastructure.
3. Add domain entities/enums and service result/error primitives.
4. Add `PricePerPlayerVnd` migration and compatibility plan.
5. Add wallet and notification services.
6. Add join request service and APIs.
7. Update feed slot calculation.
8. Add payment service and confirm payment API.
9. Add cancellation service and participant cancel API.
10. Add host join request APIs.
11. Add host session cancellation refund flow.
12. Add development top-up API.
13. Add notification APIs.
14. Add frontend API clients/types.
15. Add feed/detail join UI.
16. Add player join requests page.
17. Add host approval page.
18. Add wallet page and dev top-up UI.
19. Add notifications page.
20. Add cancellation modal.
21. Run full verification and document remaining phase work.

## Success Criteria

- Guest can request to join.
- Host can approve or reject.
- Approved request reserves a slot until payment deadline.
- Expired payment request releases the slot.
- Player can pay from wallet using server-side price.
- Payment is transaction-safe and creates participant plus escrow ledger.
- Double payment cannot charge twice.
- Overbooking is prevented.
- Player can cancel before start with standard refund.
- Player can waive refund only with strong confirmation.
- Host can voluntarily compensate after waive refund.
- Host cancellation refunds all paid participants.
- Feed reappears automatically when slots are released.
- Wallet and transaction history are visible.
- Notifications are persisted and readable.
- Development top-up works only in Development.
- Required tests are implemented and passing.

## Technical Debt

- Rename `PlaySessionPost` to `PlaySession` after this feature stabilizes.
- Remove or convert old `PricePerPlayer decimal` after all API/frontend consumers use `PricePerPlayerVnd`.
- Replace opportunistic payment expiry with a scheduled background job.
- Add SignalR for real-time notifications.
- Add richer host/user profiles and contact flow.
- Fix existing frontend text encoding issues.

## Open Questions

None for the approved implementation slice.

If a new business decision affects database shape, money movement, authorization, or concurrency, stop and ask before implementation.
