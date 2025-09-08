using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddIndiciesForAdvertisements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_advertisements_source_id",
                table: "advertisements",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_advertisements_updated_at",
                table: "advertisements",
                column: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_advertisements_source_id",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "ix_advertisements_updated_at",
                table: "advertisements");
        }
    }
}
