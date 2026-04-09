using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_advertisements_renovation_rating",
                table: "advertisements",
                column: "renovation_rating");

            migrationBuilder.CreateIndex(
                name: "ix_advertisements_square_meter_price",
                table: "advertisements",
                column: "square_meter_price");

            migrationBuilder.CreateIndex(
                name: "ix_advertisements_total_price",
                table: "advertisements",
                column: "total_price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_advertisements_renovation_rating",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "ix_advertisements_square_meter_price",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "ix_advertisements_total_price",
                table: "advertisements");
        }
    }
}
