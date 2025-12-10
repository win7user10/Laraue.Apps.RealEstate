using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionTimestampToImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "process_state",
                table: "images");

            migrationBuilder.AddColumn<DateTime>(
                name: "predicted_at",
                table: "images",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "predicted_at",
                table: "images");

            migrationBuilder.AddColumn<int>(
                name: "process_state",
                table: "images",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
