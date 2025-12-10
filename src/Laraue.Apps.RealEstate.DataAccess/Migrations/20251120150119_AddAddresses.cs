using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "house_id",
                table: "advertisements",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "streets",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    city_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streets", x => x.id);
                    table.ForeignKey(
                        name: "fk_streets_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "houses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    street_id = table.Column<long>(type: "bigint", nullable: false),
                    number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    number_normalized = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_houses", x => x.id);
                    table.ForeignKey(
                        name: "fk_houses_streets_street_id",
                        column: x => x.street_id,
                        principalTable: "streets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "cities",
                columns: new[] { "id", "name" },
                values: new object[] { 1L, "Санкт-Петербург" });

            migrationBuilder.CreateIndex(
                name: "ix_advertisements_house_id",
                table: "advertisements",
                column: "house_id");

            migrationBuilder.CreateIndex(
                name: "ix_houses_street_id_number",
                table: "houses",
                columns: new[] { "street_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_streets_city_id_name",
                table: "streets",
                columns: new[] { "city_id", "name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_advertisements_houses_house_id",
                table: "advertisements",
                column: "house_id",
                principalTable: "houses",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_advertisements_houses_house_id",
                table: "advertisements");

            migrationBuilder.DropTable(
                name: "houses");

            migrationBuilder.DropTable(
                name: "streets");

            migrationBuilder.DropTable(
                name: "cities");

            migrationBuilder.DropIndex(
                name: "ix_advertisements_house_id",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "house_id",
                table: "advertisements");
        }
    }
}
