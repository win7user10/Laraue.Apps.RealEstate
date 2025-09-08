using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Laraue.Apps.RealEstate.Db.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "advertisements",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sourceid = table.Column<string>(name: "source_id", type: "text", nullable: false),
                    sourcetype = table.Column<int>(name: "source_type", type: "integer", nullable: false),
                    square = table.Column<double>(type: "double precision", nullable: false),
                    roomscount = table.Column<int>(name: "rooms_count", type: "integer", nullable: false),
                    floornumber = table.Column<int>(name: "floor_number", type: "integer", nullable: false),
                    totalfloorsnumber = table.Column<int>(name: "total_floors_number", type: "integer", nullable: false),
                    shortdescription = table.Column<string>(name: "short_description", type: "text", nullable: true),
                    totalprice = table.Column<double>(name: "total_price", type: "double precision", nullable: false),
                    updatedat = table.Column<DateTime>(name: "updated_at", type: "timestamp with time zone", nullable: false),
                    renovationrating = table.Column<double>(name: "renovation_rating", type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_advertisements", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "transport_stops",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "text", nullable: false),
                    priority = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_stops", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "advertisement_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ispicturerelevant = table.Column<bool>(name: "is_picture_relevant", type: "boolean", nullable: true),
                    isrenovationexists = table.Column<bool>(name: "is_renovation_exists", type: "boolean", nullable: true),
                    renovationrating = table.Column<double>(name: "renovation_rating", type: "double precision", nullable: true),
                    url = table.Column<string>(type: "text", nullable: false),
                    advertisementid = table.Column<long>(name: "advertisement_id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_advertisement_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_advertisement_images_advertisements_advertisement_id",
                        column: x => x.advertisementid,
                        principalTable: "advertisements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "advertisement_transport_stops",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    advertisementid = table.Column<long>(name: "advertisement_id", type: "bigint", nullable: false),
                    transportstopid = table.Column<long>(name: "transport_stop_id", type: "bigint", nullable: false),
                    distanceinminutes = table.Column<int>(name: "distance_in_minutes", type: "integer", nullable: true),
                    distancetype = table.Column<int>(name: "distance_type", type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_advertisement_transport_stops", x => x.id);
                    table.ForeignKey(
                        name: "fk_advertisement_transport_stops_advertisements_advertisement_",
                        column: x => x.advertisementid,
                        principalTable: "advertisements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_advertisement_transport_stops_transport_stops_transport_sto",
                        column: x => x.transportstopid,
                        principalTable: "transport_stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transport_stop_external_ids",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source = table.Column<int>(type: "integer", nullable: false),
                    externalid = table.Column<string>(name: "external_id", type: "text", nullable: false),
                    transportstopid = table.Column<long>(name: "transport_stop_id", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_stop_external_ids", x => x.id);
                    table.ForeignKey(
                        name: "fk_transport_stop_external_ids_transport_stops_transport_stop_",
                        column: x => x.transportstopid,
                        principalTable: "transport_stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "transport_stops",
                columns: new[] { "id", "color", "name", "priority" },
                values: new object[,]
                {
                    { 1L, "#cf0000", "Девяткино", (byte)4 },
                    { 2L, "#cf0000", "Гражданский проспект", (byte)4 },
                    { 3L, "#cf0000", "Академическая", (byte)3 },
                    { 4L, "#cf0000", "Политехническая", (byte)3 },
                    { 5L, "#cf0000", "Площадь Мужества", (byte)3 },
                    { 6L, "#cf0000", "Лесная", (byte)2 },
                    { 7L, "#cf0000", "Выборгская", (byte)2 },
                    { 8L, "#cf0000", "Площадь Ленина", (byte)2 },
                    { 9L, "#cf0000", "Чернышевская", (byte)2 },
                    { 10L, "#cf0000", "Площадь Восстания", (byte)1 },
                    { 11L, "#cf0000", "Владимирская", (byte)1 },
                    { 12L, "#cf0000", "Пушкинская", (byte)1 },
                    { 13L, "#03238b", "Технологический институт", (byte)2 },
                    { 14L, "#cf0000", "Балтийская", (byte)3 },
                    { 15L, "#cf0000", "Нарвская", (byte)3 },
                    { 16L, "#cf0000", "Кировский завод", (byte)4 },
                    { 17L, "#cf0000", "Автово", (byte)4 },
                    { 18L, "#cf0000", "Ленинский проспект", (byte)4 },
                    { 19L, "#cf0000", "Проспект Ветеранов", (byte)4 },
                    { 20L, "#03238b", "Парнас", (byte)4 },
                    { 21L, "#03238b", "Проспект Просвещения", (byte)4 },
                    { 22L, "#03238b", "Озерки", (byte)4 },
                    { 23L, "#03238b", "Удельная", (byte)3 },
                    { 24L, "#03238b", "Пионерская", (byte)3 },
                    { 25L, "#03238b", "Черная речка", (byte)3 },
                    { 26L, "#03238b", "Петроградская", (byte)1 },
                    { 27L, "#03238b", "Горьковская", (byte)1 },
                    { 28L, "#03238b", "Невский проспект", (byte)1 },
                    { 29L, "#03238b", "Сенная площадь", (byte)1 },
                    { 30L, "#03238b", "Фрунзенская", (byte)3 },
                    { 31L, "#03238b", "Московские Ворота", (byte)3 },
                    { 32L, "#03238b", "Электросила", (byte)2 },
                    { 33L, "#03238b", "Парк Победы", (byte)2 },
                    { 34L, "#03238b", "Московская", (byte)2 },
                    { 35L, "#03238b", "Звездная", (byte)4 },
                    { 36L, "#03238b", "Купчино", (byte)4 },
                    { 37L, "#00701a", "Приморская", (byte)4 },
                    { 38L, "#00701a", "Василеостровская", (byte)2 },
                    { 39L, "#00701a", "Гостиный Двор", (byte)1 },
                    { 40L, "#00701a", "Маяковская", (byte)1 },
                    { 41L, "#ff7f00", "Площадь Александра Невского", (byte)3 },
                    { 42L, "#00701a", "Елизаровская", (byte)3 },
                    { 43L, "#00701a", "Ломоносовская", (byte)3 },
                    { 44L, "#00701a", "Пролетарская", (byte)4 },
                    { 45L, "#00701a", "Обухово", (byte)4 },
                    { 46L, "#00701a", "Рыбацкое", (byte)4 },
                    { 47L, "#94007c", "Комендантский проспект", (byte)4 },
                    { 48L, "#94007c", "Старая Деревня", (byte)4 },
                    { 49L, "#94007c", "Крестовский остров", (byte)3 },
                    { 50L, "#94007c", "Чкаловская", (byte)2 },
                    { 51L, "#94007c", "Спортивная", (byte)2 },
                    { 52L, "#94007c", "Садовая", (byte)1 },
                    { 53L, "#ff7f00", "Достоевская", (byte)1 },
                    { 54L, "#ff7f00", "Лиговский проспект", (byte)2 },
                    { 55L, "#ff7f00", "Новочеркасская", (byte)3 },
                    { 56L, "#ff7f00", "Ладожская", (byte)3 },
                    { 57L, "#ff7f00", "Проспект Большевиков", (byte)4 },
                    { 58L, "#ff7f00", "Улица Дыбенко", (byte)4 },
                    { 59L, "#94007c", "Волковская", (byte)3 },
                    { 60L, "#94007c", "Звенигородская", (byte)1 },
                    { 61L, "#ff7f00", "Спасская", (byte)1 },
                    { 62L, "#94007c", "Обводный канал", (byte)3 },
                    { 63L, "#94007c", "Адмиралтейская", (byte)2 },
                    { 64L, "#94007c", "Международная", (byte)4 },
                    { 65L, "#94007c", "Бухарестская", (byte)4 },
                    { 66L, "#00701a", "Беговая", (byte)4 },
                    { 67L, "#00701a", "Новокрестовская", (byte)3 },
                    { 68L, "#94007c", "Проспект Славы", (byte)4 },
                    { 69L, "#94007c", "Дунайская", (byte)4 },
                    { 70L, "#94007c", "Шушары", (byte)4 },
                    { 71L, "#ff7f00", "Горный институт", (byte)4 }
                });

            migrationBuilder.InsertData(
                table: "transport_stop_external_ids",
                columns: new[] { "id", "external_id", "source", "transport_stop_id" },
                values: new object[,]
                {
                    { 1L, "167", 0, 1L },
                    { 2L, "168", 0, 2L },
                    { 3L, "169", 0, 3L },
                    { 4L, "170", 0, 4L },
                    { 5L, "171", 0, 5L },
                    { 6L, "172", 0, 6L },
                    { 7L, "173", 0, 7L },
                    { 8L, "174", 0, 8L },
                    { 9L, "175", 0, 9L },
                    { 10L, "176", 0, 10L },
                    { 11L, "177", 0, 11L },
                    { 12L, "178", 0, 12L },
                    { 13L, "179", 0, 13L },
                    { 14L, "180", 0, 14L },
                    { 15L, "181", 0, 15L },
                    { 16L, "182", 0, 16L },
                    { 17L, "183", 0, 17L },
                    { 18L, "184", 0, 18L },
                    { 19L, "185", 0, 19L },
                    { 20L, "186", 0, 20L },
                    { 21L, "187", 0, 21L },
                    { 22L, "188", 0, 22L },
                    { 23L, "189", 0, 23L },
                    { 24L, "190", 0, 24L },
                    { 25L, "191", 0, 25L },
                    { 26L, "192", 0, 26L },
                    { 27L, "193", 0, 27L },
                    { 28L, "194", 0, 28L },
                    { 29L, "195", 0, 29L },
                    { 30L, "197", 0, 30L },
                    { 31L, "198", 0, 31L },
                    { 32L, "199", 0, 32L },
                    { 33L, "200", 0, 33L },
                    { 34L, "201", 0, 34L },
                    { 35L, "202", 0, 35L },
                    { 36L, "203", 0, 36L },
                    { 37L, "204", 0, 37L },
                    { 38L, "205", 0, 38L },
                    { 39L, "206", 0, 39L },
                    { 40L, "207", 0, 40L },
                    { 41L, "208", 0, 41L },
                    { 42L, "210", 0, 42L },
                    { 43L, "211", 0, 43L },
                    { 44L, "212", 0, 44L },
                    { 45L, "213", 0, 45L },
                    { 46L, "214", 0, 46L },
                    { 47L, "215", 0, 47L },
                    { 48L, "216", 0, 48L },
                    { 49L, "217", 0, 49L },
                    { 50L, "218", 0, 50L },
                    { 51L, "219", 0, 51L },
                    { 52L, "220", 0, 52L },
                    { 53L, "221", 0, 53L },
                    { 54L, "222", 0, 54L },
                    { 55L, "224", 0, 55L },
                    { 56L, "225", 0, 56L },
                    { 57L, "226", 0, 57L },
                    { 58L, "227", 0, 58L },
                    { 59L, "230", 0, 59L },
                    { 60L, "231", 0, 60L },
                    { 61L, "232", 0, 61L },
                    { 62L, "241", 0, 62L },
                    { 63L, "242", 0, 63L },
                    { 64L, "246", 0, 64L },
                    { 65L, "247", 0, 65L },
                    { 66L, "355", 0, 66L },
                    { 67L, "356", 0, 67L },
                    { 68L, "357", 0, 68L },
                    { 69L, "358", 0, 69L },
                    { 70L, "359", 0, 70L },
                    { 71L, "382", 0, 71L }
                });

            migrationBuilder.CreateIndex(
                name: "ix_advertisement_images_advertisement_id",
                table: "advertisement_images",
                column: "advertisement_id");

            migrationBuilder.CreateIndex(
                name: "ix_advertisement_images_url",
                table: "advertisement_images",
                column: "url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_advertisement_transport_stops_advertisement_id",
                table: "advertisement_transport_stops",
                column: "advertisement_id");

            migrationBuilder.CreateIndex(
                name: "ix_advertisement_transport_stops_transport_stop_id_advertiseme",
                table: "advertisement_transport_stops",
                columns: new[] { "transport_stop_id", "advertisement_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transport_stop_external_ids_transport_stop_id",
                table: "transport_stop_external_ids",
                column: "transport_stop_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advertisement_images");

            migrationBuilder.DropTable(
                name: "advertisement_transport_stops");

            migrationBuilder.DropTable(
                name: "crawler_state");

            migrationBuilder.DropTable(
                name: "transport_stop_external_ids");

            migrationBuilder.DropTable(
                name: "advertisements");

            migrationBuilder.DropTable(
                name: "transport_stops");
        }
    }
}
