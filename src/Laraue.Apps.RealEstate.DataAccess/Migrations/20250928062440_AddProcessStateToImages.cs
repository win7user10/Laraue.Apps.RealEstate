using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "decription",
                table: "advertisement_images",
                newName: "description");

            migrationBuilder.AddColumn<int>(
                name: "process_state",
                table: "advertisement_images",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "process_state",
                table: "advertisement_images");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "advertisement_images",
                newName: "decription");
        }
    }
}
