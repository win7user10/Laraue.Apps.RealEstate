using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddFlatTypeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "flat_type",
                table: "advertisements",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "flat_type",
                table: "advertisements");
        }
    }
}
