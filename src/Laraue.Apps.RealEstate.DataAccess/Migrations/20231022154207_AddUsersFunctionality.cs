using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transport_stop_external_ids");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    normalized_user_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    normalized_email = table.Column<string>(type: "text", nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false),
                    telegram_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "selections",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    min_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    max_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    min_renovation_rating = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    max_renovation_rating = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    min_per_square_meter_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    max_per_square_meter_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    min_square = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    exclude_first_floor = table.Column<bool>(type: "boolean", nullable: false),
                    exclude_last_floor = table.Column<bool>(type: "boolean", nullable: false),
                    min_metro_station_priority = table.Column<byte>(type: "smallint", nullable: true),
                    sort_by = table.Column<int>(type: "integer", nullable: false),
                    sort_order_by = table.Column<int>(type: "integer", nullable: false),
                    metro_ids = table.Column<IList<long>>(type: "jsonb", nullable: true),
                    rooms_count = table.Column<IList<int>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_selections", x => x.id);
                    table.ForeignKey(
                        name: "fk_selections_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_selections_user_id",
                table: "selections",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "selections");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.CreateTable(
                name: "transport_stop_external_ids",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transport_stop_id = table.Column<long>(type: "bigint", nullable: false),
                    external_id = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_stop_external_ids", x => x.id);
                    table.ForeignKey(
                        name: "fk_transport_stop_external_ids_transport_stops_transport_stop_",
                        column: x => x.transport_stop_id,
                        principalTable: "transport_stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "transport_stop_external_ids",
                columns: new[] { "id", "external_id", "source", "transport_stop_id" },
                values: new object[,]
                {
                    { 1L, "167", 0, 1L },
                    { 2L, "168", 0, 2L },
                    { 3L, "169", 0, 3L },
                    { 4L, "170", 0, 4L },
                    { 5L, "171", 0, 5L },
                    { 6L, "172", 0, 6L },
                    { 7L, "173", 0, 7L },
                    { 8L, "174", 0, 8L },
                    { 9L, "175", 0, 9L },
                    { 10L, "176", 0, 10L },
                    { 11L, "177", 0, 11L },
                    { 12L, "178", 0, 12L },
                    { 13L, "179", 0, 13L },
                    { 14L, "180", 0, 14L },
                    { 15L, "181", 0, 15L },
                    { 16L, "182", 0, 16L },
                    { 17L, "183", 0, 17L },
                    { 18L, "184", 0, 18L },
                    { 19L, "185", 0, 19L },
                    { 20L, "186", 0, 20L },
                    { 21L, "187", 0, 21L },
                    { 22L, "188", 0, 22L },
                    { 23L, "189", 0, 23L },
                    { 24L, "190", 0, 24L },
                    { 25L, "191", 0, 25L },
                    { 26L, "192", 0, 26L },
                    { 27L, "193", 0, 27L },
                    { 28L, "194", 0, 28L },
                    { 29L, "195", 0, 29L },
                    { 30L, "197", 0, 30L },
                    { 31L, "198", 0, 31L },
                    { 32L, "199", 0, 32L },
                    { 33L, "200", 0, 33L },
                    { 34L, "201", 0, 34L },
                    { 35L, "202", 0, 35L },
                    { 36L, "203", 0, 36L },
                    { 37L, "204", 0, 37L },
                    { 38L, "205", 0, 38L },
                    { 39L, "206", 0, 39L },
                    { 40L, "207", 0, 40L },
                    { 41L, "208", 0, 41L },
                    { 42L, "210", 0, 42L },
                    { 43L, "211", 0, 43L },
                    { 44L, "212", 0, 44L },
                    { 45L, "213", 0, 45L },
                    { 46L, "214", 0, 46L },
                    { 47L, "215", 0, 47L },
                    { 48L, "216", 0, 48L },
                    { 49L, "217", 0, 49L },
                    { 50L, "218", 0, 50L },
                    { 51L, "219", 0, 51L },
                    { 52L, "220", 0, 52L },
                    { 53L, "221", 0, 53L },
                    { 54L, "222", 0, 54L },
                    { 55L, "224", 0, 55L },
                    { 56L, "225", 0, 56L },
                    { 57L, "226", 0, 57L },
                    { 58L, "227", 0, 58L },
                    { 59L, "230", 0, 59L },
                    { 60L, "231", 0, 60L },
                    { 61L, "232", 0, 61L },
                    { 62L, "241", 0, 62L },
                    { 63L, "242", 0, 63L },
                    { 64L, "246", 0, 64L },
                    { 65L, "247", 0, 65L },
                    { 66L, "355", 0, 66L },
                    { 67L, "356", 0, 67L },
                    { 68L, "357", 0, 68L },
                    { 69L, "358", 0, 69L },
                    { 70L, "359", 0, 70L },
                    { 71L, "382", 0, 71L }
                });

            migrationBuilder.CreateIndex(
                name: "ix_transport_stop_external_ids_transport_stop_id",
                table: "transport_stop_external_ids",
                column: "transport_stop_id");
        }
    }
}
