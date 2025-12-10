using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMetroPriorities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 33L,
                column: "priority",
                value: (byte)3);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 34L,
                column: "priority",
                value: (byte)3);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 41L,
                column: "priority",
                value: (byte)2);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 49L,
                column: "priority",
                value: (byte)2);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 50L,
                column: "priority",
                value: (byte)1);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 54L,
                column: "priority",
                value: (byte)1);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 62L,
                column: "priority",
                value: (byte)2);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 63L,
                column: "priority",
                value: (byte)1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 33L,
                column: "priority",
                value: (byte)2);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 34L,
                column: "priority",
                value: (byte)2);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 41L,
                column: "priority",
                value: (byte)3);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 49L,
                column: "priority",
                value: (byte)3);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 50L,
                column: "priority",
                value: (byte)2);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 54L,
                column: "priority",
                value: (byte)2);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 62L,
                column: "priority",
                value: (byte)3);

            migrationBuilder.UpdateData(
                table: "transport_stops",
                keyColumn: "id",
                keyValue: 63L,
                column: "priority",
                value: (byte)2);
        }
    }
}
