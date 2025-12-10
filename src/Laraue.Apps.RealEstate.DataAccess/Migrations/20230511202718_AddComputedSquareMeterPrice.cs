using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddComputedSquareMeterPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "square_meter_price",
                table: "advertisements",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
            
            migrationBuilder.Sql(@"
                update advertisements
                set square_meter_price = 
                CASE WHEN square > 0
                THEN total_price / square
                ELSE 79228162514264337593543950335
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "square_meter_price",
                table: "advertisements");
        }
    }
}
