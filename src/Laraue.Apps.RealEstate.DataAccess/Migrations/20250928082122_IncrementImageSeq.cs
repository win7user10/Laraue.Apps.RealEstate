using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class IncrementImageSeq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SELECT setval('images_id_seq', (SELECT MAX(id) FROM images))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
