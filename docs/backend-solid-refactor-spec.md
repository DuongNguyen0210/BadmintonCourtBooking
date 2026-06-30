# Spec: Backend SOLID Refactor

## Assumptions

1. Backend continues to target `net10.0`.
2. The project stays as one ASP.NET Core Web API project for this phase.
3. ASP.NET Core Identity cookie authentication remains unchanged.
4. PostgreSQL and EF Core remain the persistence stack.
5. Existing frontend API contracts should remain compatible unless a change is explicitly approved.
6. This refactor must not add booking, court management, payment gateway, chat, review, admin, or role features.
7. Business behavior for authentication, play session posts, join requests, wallet, payment, cancellation, feed visibility, and notifications should remain the same unless a bug is found and approved for correction.

## Objective

Improve the backend implementation so it follows SOLID more clearly while preserving current behavior.

Success means:

- Controllers are thin: receive HTTP input, get current user, call application services, return HTTP responses.
- Business rules are not duplicated across controllers and services.
- EF Core configuration is separated from `ApplicationDbContext`.
- Wallet and ledger changes are centralized so balance updates and ledger entries cannot drift.
- Notification creation is consistently routed through a notification abstraction.
- Slot availability calculation has one source of truth.
- Error-to-HTTP response mapping is consistent.
- Critical flows are protected by unit/integration tests before larger refactors.

## Current Backend Summary

Important files currently involved:

- `Program.cs`: dependency injection, Identity, cookie auth, CORS, middleware, controller mapping.
- `Data/ApplicationDbContext.cs`: Identity DbContext plus all EF Core entity configuration.
- `Controllers/PlaySessionsController.cs`: feed, detail, create, update, cancel.
- `Controllers/*JoinRequestsController.cs`: player and host join request endpoints.
- `Controllers/ParticipationsController.cs`: participation cancellation endpoint.
- `Controllers/WalletController.cs`, `DevelopmentWalletController.cs`, `NotificationsController.cs`.
- `Services/JoinRequestService.cs`: request-to-join, host approval/rejection, request queries.
- `Services/PaymentService.cs`: confirm payment, escrow hold, participant creation, notifications.
- `Services/CancellationService.cs`: participant cancellation and host session cancellation.
- `Services/WalletService.cs`: wallet read, transaction history, development top-up.
- `Services/PlaySessionAvailabilityService.cs`: feed visibility and occupied slot calculation.
- `Models/*`: domain entities and enums.
- `BadmintonCourtBooking.Tests/Unit/*`: current unit tests.

## SOLID Findings

### Strengths

- Domain entities already contain some behavior, for example join request state transitions and wallet balance operations.
- Important business operations use database transactions.
- `IClock`, options classes, and service interfaces already exist.
- Controllers for wallet, notification, and join request flows are mostly thin.
- Money is represented as `long` VND in the newer wallet/payment flow.

### Main Problems

1. `ApplicationDbContext.OnModelCreating` violates SRP because it contains all entity mappings in one class.
2. `PlaySessionsController` contains query logic, creation/update orchestration, DTO mapping, time access, and cancellation response mapping.
3. `JoinRequestService`, `PaymentService`, and `CancellationService` duplicate slot calculation and validation concepts.
4. `PaymentService`, `CancellationService`, and `WalletService` directly manipulate wallet balances and ledger entries in separate places.
5. Notification creation is sometimes done through `NotificationService`, sometimes directly through `dbContext.Notifications.Add`.
6. Error code to HTTP status mapping is repeated in multiple controllers.
7. Current-user extraction is repeated in multiple controllers.
8. Transaction orchestration is repeated and not clearly named by use case.
9. Integration tests are not yet covering the high-risk flows: payment, cancellation, feed reappearance, duplicate submit, authorization, and concurrency.

## Confirmed Decisions

1. Integration tests will use `Testcontainers.PostgreSql`.
   - Tests must use an isolated PostgreSQL test environment.
   - Tests must not use the development Docker Compose database.
   - EF Core InMemory must not be the primary tool for testing constraints, transactions, concurrency, foreign keys, or PostgreSQL-specific behavior.
   - A shared fixture per test collection is allowed to avoid starting a container per test.
   - Test data must be reset, rolled back, or isolated per test/test class.
   - Test connection strings must exist only at test runtime.
   - Tests should run with `dotnet test` when Docker is running.
2. Backend code can move gradually into feature folders.
   - No big-bang move.
   - API routes and response contracts must stay compatible unless a bug/spec explicitly requires change.
   - The project must build after each task.
   - Namespaces and dependencies are updated in small steps.
   - Do not move files and rewrite all business logic in the same large task.
   - Do not create circular dependencies between features.
3. Each implementation task should be committed separately after build/tests pass and the diff is scoped.
4. Clear bugs may be fixed inside the same task only when proven by a regression test and when they do not change public API, database schema, business rules, auth, transaction, concurrency, money flow, or task scope.

## Interface Rules

Create interfaces for:

- Application services used by controllers or other features.
- External dependencies that may be replaced.
- Components that need mocks/fakes in unit tests.
- Business policies that may have multiple implementations.
- Infrastructure boundaries.

Do not create interfaces only to mirror every helper class. Avoid generic repositories that simply wrap EF Core CRUD methods without adding meaningful business abstraction.

## Target Structure

Keep one backend project, but organize by responsibility:

```text
BadmintonCourtBooking/
  Controllers/
  Data/
    ApplicationDbContext.cs
    Configurations/
      ApplicationUserConfiguration.cs
      PlaySessionPostConfiguration.cs
      PlaySessionJoinRequestConfiguration.cs
      PlaySessionParticipantConfiguration.cs
      WalletConfiguration.cs
      WalletTransactionConfiguration.cs
      ParticipationCancellationConfiguration.cs
      NotificationConfiguration.cs
  Dtos/
  Models/
  Options/
  Services/
    Common/
      IClock.cs
      SystemClock.cs
      ServiceResult.cs
      ServiceError.cs
      ICurrentUserAccessor.cs
      ControllerResultExtensions.cs
    PlaySessions/
      IPlaySessionPostService.cs
      PlaySessionPostService.cs
      PlaySessionPostMapper.cs
      IPlaySessionAvailabilityService.cs
      PlaySessionAvailabilityService.cs
    JoinRequests/
      IJoinRequestService.cs
      JoinRequestService.cs
      JoinRequestMapper.cs
    Payments/
      IPaymentService.cs
      PaymentService.cs
    Cancellations/
      IParticipationCancellationService.cs
      IHostPlaySessionCancellationService.cs
      CancellationPolicy.cs
    Wallets/
      IWalletService.cs
      IWalletAccountingService.cs
      WalletService.cs
      WalletAccountingService.cs
    Notifications/
      INotificationService.cs
      NotificationService.cs
  Features/
    Auth/
    PlaySessions/
    JoinRequests/
    Wallets/
    Notifications/
```

This is not Clean Architecture split into multiple projects. It is a gradual folder-level cleanup inside the current Web API project. During the transition, some files can remain in the existing `Controllers`, `Dtos`, and `Services` folders until their feature is refactored.

## Code Style

Controllers should look like this style:

```csharp
[HttpPost("{joinRequestId:guid}/confirm-payment")]
public async Task<IActionResult> ConfirmPayment(Guid joinRequestId, CancellationToken cancellationToken)
{
    var result = await paymentService.ConfirmPaymentAsync(
        joinRequestId,
        currentUserAccessor.GetRequiredUserId(User),
        cancellationToken);

    return this.ToActionResult(result);
}
```

Application services should own use-case orchestration:

```csharp
public async Task<ServiceResult<ConfirmPaymentResponse>> ConfirmPaymentAsync(
    Guid joinRequestId,
    string userId,
    CancellationToken cancellationToken)
{
    await using var transaction = await transactionRunner.BeginSerializableAsync(cancellationToken);

    // Load aggregate data, validate business rules, call wallet accounting,
    // create participant, create notifications, save, commit.
}
```

Domain entities/policies should own state rules and calculations:

```csharp
public void MarkAsPaid(DateTimeOffset now)
{
    if (Status != JoinRequestStatus.AwaitingPayment)
        throw new DomainException("JOIN_REQUEST_NOT_AWAITING_PAYMENT", "Only approved join requests can be paid.");

    Status = JoinRequestStatus.Joined;
    PaidAtUtc = now;
    UpdatedAtUtc = now;
}
```

## Commands

Backend build:

```powershell
dotnet build .\BadmintonCourtBooking.sln
```

Backend tests:

```powershell
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
```

Frontend build, only when API contracts or generated frontend usage are touched:

```powershell
cd frontend
npm.cmd run build
```

Docker PostgreSQL check:

```powershell
docker compose ps
```

Migration list, only when persistence mapping/schema is touched:

```powershell
dotnet tool run dotnet-ef migrations list --project .\BadmintonCourtBooking\BadmintonCourtBooking.csproj
```

## Testing Strategy

Use the existing `BadmintonCourtBooking.Tests` xUnit project.

Unit tests should cover:

- Domain state transitions.
- Cancellation policy money calculations.
- Wallet balance operations.
- Slot availability calculation edge cases.
- Error mapping helpers.

Integration tests should cover:

- API authorization boundaries.
- EF Core persistence behavior.
- Payment transaction success and failure.
- Duplicate payment submit.
- Participant cancellation refund behavior.
- Host cancellation full refund behavior.
- Feed visibility after slot release.

Integration tests will use `Testcontainers.PostgreSql`. This requires adding the minimal test dependency when Task 9 starts.

## Boundaries

Always:

- Keep `net10.0`.
- Keep authentication as ASP.NET Core Identity cookie auth.
- Preserve existing API contracts unless explicitly approved.
- Keep controllers thin.
- Keep money operations inside database transactions.
- Run relevant build/tests before each commit.
- Commit each approved task separately if implementation is requested.
- Use the appropriate skill before each task.

Ask first:

- Adding new NuGet packages.
- Changing database schema or creating a migration.
- Changing API response shape or route names.
- Renaming `PlaySessionPost` to `PlaySession`.
- Removing legacy `PricePerPlayer`.
- Splitting backend into multiple projects.
- Changing authentication or CORS behavior.
- Changing money, refund, authorization, or concurrency rules.

Never:

- Downgrade target framework from `net10.0`.
- Rewrite the whole backend.
- Push to remote without explicit request.
- Commit secrets or local connection strings.
- Use frontend-provided user id, host id, price, refund amount, or balance as source of truth.
- Remove tests to make the build pass.

## Implementation Plan

### Task 1: Document and commit backend SOLID refactor spec

Files:

- `docs/backend-solid-refactor-spec.md`

Acceptance:

- Refactor scope and boundaries are documented.
- User decisions are recorded in the spec.
- No production code is changed.

Verify:

```powershell
git diff -- docs/backend-solid-refactor-spec.md
```

Skill:

- `spec-driven-development`
- `documentation-and-adrs`
- `git-workflow-and-versioning`

### Task 2: Extract EF Core entity configurations

Files:

- `Data/ApplicationDbContext.cs`
- `Data/Configurations/*.cs`

Acceptance:

- `ApplicationDbContext.OnModelCreating` only calls `base.OnModelCreating(builder)` and applies configurations.
- No schema behavior changes.
- No migration is needed if generated model snapshot stays unchanged.

Verify:

```powershell
dotnet build .\BadmintonCourtBooking.sln
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
```

Skills:

- `incremental-implementation`
- `test-driven-development`
- `code-review-and-quality`

### Task 3: Centralize controller current-user and error mapping

Files:

- `Services/Common/ICurrentUserAccessor.cs`
- `Services/Common/CurrentUserAccessor.cs`
- `Services/Common/ControllerResultExtensions.cs`
- Controllers using repeated `GetCurrentUserId` and error switch logic.

Acceptance:

- Controllers no longer duplicate `User.FindFirstValue`.
- Common `ServiceResult<T>` mapping handles common error codes consistently.
- Existing endpoint behavior remains compatible.

Verify:

```powershell
dotnet build .\BadmintonCourtBooking.sln
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
```

Skills:

- `incremental-implementation`
- `security-and-hardening`
- `test-driven-development`
- `code-review-and-quality`

### Task 4: Move play session post use cases out of controller

Files:

- `Controllers/PlaySessionsController.cs`
- `Services/PlaySessions/IPlaySessionPostService.cs`
- `Services/PlaySessions/PlaySessionPostService.cs`
- `Services/PlaySessions/PlaySessionPostMapper.cs`

Acceptance:

- `PlaySessionsController` calls a service for feed, detail, create, update, and cancel.
- DTO mapping is no longer private static methods inside controller.
- Feed visibility still uses availability service.
- Create/update still keep `PricePerPlayer` and `PricePerPlayerVnd` synchronized through the entity method.

Verify:

```powershell
dotnet build .\BadmintonCourtBooking.sln
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
cd frontend
npm.cmd run build
```

Skills:

- `incremental-implementation`
- `test-driven-development`
- `security-and-hardening`
- `code-review-and-quality`

### Task 5: Make play session availability the only slot source

Files:

- `Services/PlaySessions/IPlaySessionAvailabilityService.cs`
- `Services/PlaySessions/PlaySessionAvailabilityService.cs`
- `Services/JoinRequests/JoinRequestService.cs`
- `Services/Payments/PaymentService.cs`
- Unit tests for availability.

Acceptance:

- Join request, payment, and feed use one service for occupied slot logic.
- Payment can exclude the paying request from held slot calculation.
- No duplicated slot-count queries remain in join/payment services.

Verify:

```powershell
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
dotnet build .\BadmintonCourtBooking.sln
```

Skills:

- `test-driven-development`
- `incremental-implementation`
- `code-review-and-quality`

### Task 6: Centralize wallet accounting and ledger creation

Files:

- `Services/Wallets/IWalletAccountingService.cs`
- `Services/Wallets/WalletAccountingService.cs`
- `Services/Wallets/WalletService.cs`
- `Services/Payments/PaymentService.cs`
- `Services/Cancellations/CancellationService.cs`
- Wallet/accounting tests.

Acceptance:

- Balance mutation and corresponding `WalletTransaction` creation are handled by one accounting service.
- `PaymentService` and cancellation flows no longer manually create ledger entries.
- No balance update can happen without a ledger entry in the service layer.

Verify:

```powershell
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
dotnet build .\BadmintonCourtBooking.sln
```

Skills:

- `test-driven-development`
- `security-and-hardening`
- `incremental-implementation`
- `code-review-and-quality`

### Task 7: Use notification service consistently

Files:

- `Services/Notifications/INotificationService.cs`
- `Services/Notifications/NotificationService.cs`
- `Services/JoinRequests/JoinRequestService.cs`
- `Services/Payments/PaymentService.cs`
- `Services/Cancellations/CancellationService.cs`

Acceptance:

- Application services no longer call `dbContext.Notifications.Add` directly.
- Notification creation can participate in an existing transaction.
- Notification query/read behavior remains unchanged.

Verify:

```powershell
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
dotnet build .\BadmintonCourtBooking.sln
```

Skills:

- `incremental-implementation`
- `test-driven-development`
- `code-review-and-quality`

### Task 8: Split cancellation use cases

Files:

- `Services/Cancellations/IParticipationCancellationService.cs`
- `Services/Cancellations/IHostPlaySessionCancellationService.cs`
- `Services/Cancellations/ParticipationCancellationService.cs`
- `Services/Cancellations/HostPlaySessionCancellationService.cs`
- `Controllers/ParticipationsController.cs`
- `Controllers/PlaySessionsController.cs`

Acceptance:

- Player cancellation and host session cancellation are separated.
- Shared money/refund behavior is delegated to wallet accounting and cancellation policy.
- Controller methods depend only on the service they need.

Verify:

```powershell
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
dotnet build .\BadmintonCourtBooking.sln
```

Skills:

- `incremental-implementation`
- `test-driven-development`
- `security-and-hardening`
- `code-review-and-quality`

### Task 9: Add integration test foundation

Files:

- `BadmintonCourtBooking.Tests/Integration/*`
- `BadmintonCourtBooking.Tests/BadmintonCourtBooking.Tests.csproj`

Acceptance:

- Integration tests can run against PostgreSQL.
- Test data is isolated per test run.
- Production database is never touched by tests.

Verify:

```powershell
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
```

Skills:

- `test-driven-development`
- `security-and-hardening`
- `source-driven-development`

### Task 10: Add high-risk flow regression tests

Files:

- `BadmintonCourtBooking.Tests/Unit/*`
- `BadmintonCourtBooking.Tests/Integration/*`

Acceptance:

- Tests cover duplicate join request, wrong host approval, approval to awaiting payment, insufficient balance, successful payment, duplicate payment, cancellation refund, no-refund confirmation, host cancellation, feed reappearance, wallet/notification authorization.

Verify:

```powershell
dotnet test .\BadmintonCourtBooking.Tests\BadmintonCourtBooking.Tests.csproj
```

Skills:

- `test-driven-development`
- `security-and-hardening`
- `code-review-and-quality`

## Risks

- Refactoring payment/cancellation without enough tests could introduce money bugs.
- Moving EF configuration may accidentally change model metadata if not checked carefully.
- Centralizing error mapping can accidentally change HTTP status codes.
- Integration test setup may require a new dependency or a stricter Docker workflow.
- Moving files into feature folders can cause namespace churn; keep this mechanical and separate from behavior changes.

## Open Questions

No open business decisions for Task 1.

Before implementation tasks that add dependencies, change schema, change public API, or touch money/authorization/concurrency behavior, confirm the specific change first.
