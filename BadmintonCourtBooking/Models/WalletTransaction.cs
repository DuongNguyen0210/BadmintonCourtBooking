namespace BadmintonCourtBooking.Models;

public sealed class WalletTransaction
{
    private WalletTransaction()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();

    public string UserId { get; private set; } = string.Empty;

    public string? RelatedUserId { get; private set; }

    public Guid? PlaySessionPostId { get; private set; }

    public Guid? JoinRequestId { get; private set; }

    public Guid? CancellationId { get; private set; }

    public WalletTransactionType Type { get; private set; }

    public WalletTransactionStatus Status { get; private set; } = WalletTransactionStatus.Completed;

    public long AmountVnd { get; private set; }

    public long BalanceBeforeVnd { get; private set; }

    public long BalanceAfterVnd { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public string? IdempotencyKey { get; private set; }

    public static WalletTransaction CreateCompleted(
        string userId,
        WalletTransactionType type,
        long amountVnd,
        long balanceBeforeVnd,
        long balanceAfterVnd,
        string description,
        DateTimeOffset now,
        string? relatedUserId = null,
        Guid? playSessionPostId = null,
        Guid? joinRequestId = null,
        Guid? cancellationId = null,
        string? idempotencyKey = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        if (amountVnd <= 0)
            throw new ArgumentOutOfRangeException(nameof(amountVnd), "Amount must be greater than zero.");

        return new WalletTransaction
        {
            UserId = userId,
            RelatedUserId = relatedUserId,
            PlaySessionPostId = playSessionPostId,
            JoinRequestId = joinRequestId,
            CancellationId = cancellationId,
            Type = type,
            Status = WalletTransactionStatus.Completed,
            AmountVnd = amountVnd,
            BalanceBeforeVnd = balanceBeforeVnd,
            BalanceAfterVnd = balanceAfterVnd,
            Description = description.Trim(),
            CreatedAtUtc = now,
            IdempotencyKey = idempotencyKey
        };
    }
}
