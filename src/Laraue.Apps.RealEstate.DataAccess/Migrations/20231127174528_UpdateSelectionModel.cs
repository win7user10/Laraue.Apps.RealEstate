using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSelectionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crawling_session_advertisements");

            migrationBuilder.DropTable(
                name: "crawling_sessions");

            migrationBuilder.AddColumn<int>(
                name: "limit",
                table: "selections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "notification_interval",
                table: "selections",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "sent_at",
                table: "selections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "crawled_at",
                table: "advertisements",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "ix_selections_sent_at",
                table: "selections",
                column: "sent_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_selections_sent_at",
                table: "selections");

            migrationBuilder.DropColumn(
                name: "limit",
                table: "selections");

            migrationBuilder.DropColumn(
                name: "notification_interval",
                table: "selections");

            migrationBuilder.DropColumn(
                name: "sent_at",
                table: "selections");

            migrationBuilder.DropColumn(
                name: "crawled_at",
                table: "advertisements");

            migrationBuilder.CreateTable(
                name: "crawling_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_crawling_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crawling_session_advertisements",
                columns: table => new
                {
                    advertisement_id = table.Column<long>(type: "bigint", nullable: false),
                    crawling_session_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_crawling_session_advertisements", x => new { x.advertisement_id, x.crawling_session_id });
                    table.ForeignKey(
                        name: "fk_crawling_session_advertisements_advertisements_advertisemen",
                        column: x => x.advertisement_id,
                        principalTable: "advertisements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_crawling_session_advertisements_crawling_sessions_crawling_",
                        column: x => x.crawling_session_id,
                        principalTable: "crawling_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_crawling_session_advertisements_crawling_session_id",
                table: "crawling_session_advertisements",
                column: "crawling_session_id");
        }
    }
}
