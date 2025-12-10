using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class MoreIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_advertisements_floor_number",
                table: "advertisements",
                column: "floor_number");

            migrationBuilder.CreateIndex(
                name: "ix_advertisements_rooms_count",
                table: "advertisements",
                column: "rooms_count");

            migrationBuilder.CreateIndex(
                name: "ix_advertisements_square",
                table: "advertisements",
                column: "square");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_advertisements_floor_number",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "ix_advertisements_rooms_count",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "ix_advertisements_square",
                table: "advertisements");
        }
    }
}
