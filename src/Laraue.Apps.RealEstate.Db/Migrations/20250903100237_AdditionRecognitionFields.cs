using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class AdditionRecognitionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_picture_relevant",
                table: "advertisement_images");

            migrationBuilder.DropColumn(
                name: "is_renovation_exists",
                table: "advertisement_images");

            migrationBuilder.AlterColumn<double>(
                name: "renovation_rating",
                table: "advertisement_images",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "decription",
                table: "advertisement_images",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "tags",
                table: "advertisement_images",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "decription",
                table: "advertisement_images");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "advertisement_images");

            migrationBuilder.AlterColumn<double>(
                name: "renovation_rating",
                table: "advertisement_images",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<bool>(
                name: "is_picture_relevant",
                table: "advertisement_images",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_renovation_exists",
                table: "advertisement_images",
                type: "boolean",
                nullable: true);
        }
    }
}
