using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AbbContentEditor.Migrations
{
    /// <inheritdoc />
    public partial class _2blogcat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pubdate",
                table: "Blogs",
                newName: "PubDate");

            migrationBuilder.RenameColumn(
                name: "Updated",
                table: "Blogs",
                newName: "UpdDate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PubDate",
                table: "Blogs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Blogs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TheText",
                table: "Blogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PubDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "PubDate", "UpdDate" },
                values: new object[,]
                {
                    { 1, "Lifestyle", null, new DateTime(2024, 8, 25, 7, 6, 40, 765, DateTimeKind.Utc).AddTicks(4150) },
                    { 2, "Sport", null, new DateTime(2024, 8, 25, 7, 6, 40, 765, DateTimeKind.Utc).AddTicks(4155) },
                    { 3, "Software Development", null, new DateTime(2024, 8, 25, 7, 6, 40, 765, DateTimeKind.Utc).AddTicks(4156) }
                });

            migrationBuilder.InsertData(
                table: "Blogs",
                columns: new[] { "Id", "CategoryId", "ImageUrl", "IsDeleted", "Preview", "PubDate", "TheText", "Title", "UpdDate" },
                values: new object[] { 1, 1, "imageUrl", false, "Vitafit Digital Personal Scales for People, Weighing Professional since 2001, Body Scales with Clear LED Display and Step-On, 180 kg, Batteries Included, Silver Black…", null, "HIGH PRECISION GUARANTEE With more than 20 years experience in the scale industry, we have developed the scale with the best technology and expertise, guaranteeing high accuracy of 0.1lb/0.05kg throughout the life of the scale.\r\nEasy to use: the scale people uses up-to-date digital technology, along with many friendly functions, including: auto calibration, auto step up, auto power off, convenient large platform in 280 x 280 mm, 3 x AAA batteries included, 3 unit switch: lb/kg/st, and high precision in full weighing range.", "My first blog post from dbcontext migration", new DateTime(2024, 8, 25, 7, 6, 40, 765, DateTimeKind.Utc).AddTicks(4303) });

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CategoryId",
                table: "Blogs",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blogs_Categories_CategoryId",
                table: "Blogs",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blogs_Categories_CategoryId",
                table: "Blogs");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Blogs_CategoryId",
                table: "Blogs");

            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "TheText",
                table: "Blogs");

            migrationBuilder.RenameColumn(
                name: "PubDate",
                table: "Blogs",
                newName: "Pubdate");

            migrationBuilder.RenameColumn(
                name: "UpdDate",
                table: "Blogs",
                newName: "Updated");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Pubdate",
                table: "Blogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
