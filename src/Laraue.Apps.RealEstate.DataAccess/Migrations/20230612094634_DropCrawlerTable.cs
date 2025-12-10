using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class DropCrawlerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crawler_state");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crawler_state",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_crawler_state", x => x.key);
                });
        }
    }
}
