using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class MakeImagesRelationManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    renovation_rating = table.Column<double>(type: "double precision", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string[]>(type: "text[]", nullable: false),
                    process_state = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_images", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_images_url",
                table: "images",
                column: "url",
                unique: true);

            migrationBuilder.Sql(@"
insert into images
select
    id,
    renovation_rating,
    url,
    description,
    tags,
    process_state
    from advertisement_images
");
            
            migrationBuilder.DropPrimaryKey(
                name: "pk_advertisement_images",
                table: "advertisement_images");

            migrationBuilder.DropIndex(
                name: "ix_advertisement_images_advertisement_id",
                table: "advertisement_images");

            migrationBuilder.DropIndex(
                name: "ix_advertisement_images_url",
                table: "advertisement_images");

            migrationBuilder.DropColumn(
                name: "description",
                table: "advertisement_images");

            migrationBuilder.DropColumn(
                name: "process_state",
                table: "advertisement_images");

            migrationBuilder.DropColumn(
                name: "renovation_rating",
                table: "advertisement_images");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "advertisement_images");

            migrationBuilder.DropColumn(
                name: "url",
                table: "advertisement_images");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "advertisement_images",
                newName: "image_id");

            migrationBuilder.AlterColumn<long>(
                name: "image_id",
                table: "advertisement_images",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "fk_advertisement_images_images_image_id",
                table: "advertisement_images",
                column: "image_id",
                principalTable: "images",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddPrimaryKey(
                name: "pk_advertisement_images",
                table: "advertisement_images",
                columns: new[] { "advertisement_id", "image_id" });

            migrationBuilder.CreateIndex(
                name: "ix_advertisement_images_image_id",
                table: "advertisement_images",
                column: "image_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_advertisement_images_images_image_id",
                table: "advertisement_images");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropPrimaryKey(
                name: "pk_advertisement_images",
                table: "advertisement_images");

            migrationBuilder.DropIndex(
                name: "ix_advertisement_images_image_id",
                table: "advertisement_images");

            migrationBuilder.RenameColumn(
                name: "image_id",
                table: "advertisement_images",
                newName: "id");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "advertisement_images",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "advertisement_images",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "process_state",
                table: "advertisement_images",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "renovation_rating",
                table: "advertisement_images",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string[]>(
                name: "tags",
                table: "advertisement_images",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "advertisement_images",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "pk_advertisement_images",
                table: "advertisement_images",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_advertisement_images_advertisement_id",
                table: "advertisement_images",
                column: "advertisement_id");

            migrationBuilder.CreateIndex(
                name: "ix_advertisement_images_url",
                table: "advertisement_images",
                column: "url",
                unique: true);
        }
    }
}
