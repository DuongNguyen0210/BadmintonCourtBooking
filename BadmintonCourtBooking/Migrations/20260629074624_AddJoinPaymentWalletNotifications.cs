using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonCourtBooking.Migrations
{
    /// <inheritdoc />
    public partial class AddJoinPaymentWalletNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PricePerPlayerVnd",
                table: "PlaySessionPosts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql(
                """
                UPDATE "PlaySessionPosts"
                SET "PricePerPlayerVnd" = FLOOR("PricePerPlayer")::bigint
                """);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RelatedEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaySessionJoinRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaySessionPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestUserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByHostId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    PaymentDueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaidAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaySessionJoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaySessionJoinRequests_AspNetUsers_GuestUserId",
                        column: x => x.GuestUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaySessionJoinRequests_PlaySessionPosts_PlaySessionPostId",
                        column: x => x.PlaySessionPostId,
                        principalTable: "PlaySessionPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    AvailableBalanceVnd = table.Column<long>(type: "bigint", nullable: false),
                    HeldBalanceVnd = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaySessionParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaySessionPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    JoinRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaySessionParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaySessionParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaySessionParticipants_PlaySessionJoinRequests_JoinRequest~",
                        column: x => x.JoinRequestId,
                        principalTable: "PlaySessionJoinRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlaySessionParticipants_PlaySessionPosts_PlaySessionPostId",
                        column: x => x.PlaySessionPostId,
                        principalTable: "PlaySessionPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParticipationCancellations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "text", nullable: false),
                    RefundChoice = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OriginalAmountVnd = table.Column<long>(type: "bigint", nullable: false),
                    RefundAmountVnd = table.Column<long>(type: "bigint", nullable: false),
                    CancellationFeeVnd = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipationCancellations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipationCancellations_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipationCancellations_PlaySessionParticipants_Particip~",
                        column: x => x.ParticipantId,
                        principalTable: "PlaySessionParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RelatedUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    PlaySessionPostId = table.Column<Guid>(type: "uuid", nullable: true),
                    JoinRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AmountVnd = table.Column<long>(type: "bigint", nullable: false),
                    BalanceBeforeVnd = table.Column<long>(type: "bigint", nullable: false),
                    BalanceAfterVnd = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_AspNetUsers_RelatedUserId",
                        column: x => x.RelatedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_ParticipationCancellations_CancellationId",
                        column: x => x.CancellationId,
                        principalTable: "ParticipationCancellations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_PlaySessionJoinRequests_JoinRequestId",
                        column: x => x.JoinRequestId,
                        principalTable: "PlaySessionJoinRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_PlaySessionPosts_PlaySessionPostId",
                        column: x => x.PlaySessionPostId,
                        principalTable: "PlaySessionPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAtUtc",
                table: "Notifications",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipationCancellations_ParticipantId",
                table: "ParticipationCancellations",
                column: "ParticipantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipationCancellations_RequestedByUserId",
                table: "ParticipationCancellations",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionJoinRequests_GuestUserId_Status",
                table: "PlaySessionJoinRequests",
                columns: new[] { "GuestUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionJoinRequests_PaymentDueAtUtc",
                table: "PlaySessionJoinRequests",
                column: "PaymentDueAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionJoinRequests_PlaySessionPostId_GuestUserId",
                table: "PlaySessionJoinRequests",
                columns: new[] { "PlaySessionPostId", "GuestUserId" },
                unique: true,
                filter: "\"Status\" IN ('PendingHostApproval', 'AwaitingPayment', 'Joined')");

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionJoinRequests_PlaySessionPostId_Status",
                table: "PlaySessionJoinRequests",
                columns: new[] { "PlaySessionPostId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionParticipants_JoinRequestId",
                table: "PlaySessionParticipants",
                column: "JoinRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionParticipants_PlaySessionPostId_Status",
                table: "PlaySessionParticipants",
                columns: new[] { "PlaySessionPostId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionParticipants_PlaySessionPostId_UserId",
                table: "PlaySessionParticipants",
                columns: new[] { "PlaySessionPostId", "UserId" },
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionParticipants_UserId_Status",
                table: "PlaySessionParticipants",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CancellationId",
                table: "WalletTransactions",
                column: "CancellationId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_IdempotencyKey",
                table: "WalletTransactions",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_JoinRequestId",
                table: "WalletTransactions",
                column: "JoinRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_PlaySessionPostId",
                table: "WalletTransactions",
                column: "PlaySessionPostId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_RelatedUserId",
                table: "WalletTransactions",
                column: "RelatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_UserId_CreatedAtUtc",
                table: "WalletTransactions",
                columns: new[] { "UserId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "ParticipationCancellations");

            migrationBuilder.DropTable(
                name: "PlaySessionParticipants");

            migrationBuilder.DropTable(
                name: "PlaySessionJoinRequests");

            migrationBuilder.DropColumn(
                name: "PricePerPlayerVnd",
                table: "PlaySessionPosts");
        }
    }
}
