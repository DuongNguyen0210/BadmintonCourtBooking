using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonCourtBooking.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaySessionPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaySessionPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatorUserId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CourtName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CourtAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PricePerPlayer = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    CurrentPlayers = table.Column<int>(type: "integer", nullable: false),
                    MalePlayers = table.Column<int>(type: "integer", nullable: false),
                    FemalePlayers = table.Column<int>(type: "integer", nullable: false),
                    ShowMalePlayers = table.Column<bool>(type: "boolean", nullable: false),
                    ShowFemalePlayers = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaySessionPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaySessionPosts_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionPosts_CreatorUserId",
                table: "PlaySessionPosts",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionPosts_EndTime",
                table: "PlaySessionPosts",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionPosts_StartTime",
                table: "PlaySessionPosts",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_PlaySessionPosts_Status",
                table: "PlaySessionPosts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaySessionPosts");
        }
    }
}
