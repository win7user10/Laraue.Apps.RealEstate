using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePredictionLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_images_predicted_at",
                table: "images");

            migrationBuilder.DropColumn(
                name: "predicted_at",
                table: "images");

            migrationBuilder.DropColumn(
                name: "renovation_rating",
                table: "images");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "images");
            
            migrationBuilder.Sql(@"
update advertisements
set renovation_rating = ROUND(renovation_rating * 10)
where renovation_rating is not null");

            migrationBuilder.AlterColumn<int>(
                name: "renovation_rating",
                table: "advertisements",
                type: "integer",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "advantages",
                table: "advertisements",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string[]>(
                name: "problems",
                table: "advertisements",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "ready_at",
                table: "advertisements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_advertisements_predicted_at",
                table: "advertisements",
                column: "predicted_at");
            
            migrationBuilder.Sql(@"
update advertisements
set ready_at = predicted_at
where predicted_at is not null");

            migrationBuilder.CreateIndex(
                name: "ix_advertisements_ready_at",
                table: "advertisements",
                column: "ready_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_advertisements_predicted_at",
                table: "advertisements");

            migrationBuilder.DropIndex(
                name: "ix_advertisements_ready_at",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "advantages",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "problems",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "ready_at",
                table: "advertisements");

            migrationBuilder.AddColumn<DateTime>(
                name: "predicted_at",
                table: "images",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "renovation_rating",
                table: "images",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string[]>(
                name: "tags",
                table: "images",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AlterColumn<double>(
                name: "renovation_rating",
                table: "advertisements",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_images_predicted_at",
                table: "images",
                column: "predicted_at");
        }
    }
}
