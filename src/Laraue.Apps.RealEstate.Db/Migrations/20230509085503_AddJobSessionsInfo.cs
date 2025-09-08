using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSessionsInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "distance_type",
                table: "advertisement_transport_stops",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "distance_in_minutes",
                table: "advertisement_transport_stops",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "crawling_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_crawling_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crawling_session_advertisements",
                columns: table => new
                {
                    crawling_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    advertisement_id = table.Column<long>(type: "bigint", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crawling_session_advertisements");

            migrationBuilder.DropTable(
                name: "crawling_sessions");

            migrationBuilder.AlterColumn<int>(
                name: "distance_type",
                table: "advertisement_transport_stops",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "distance_in_minutes",
                table: "advertisement_transport_stops",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
