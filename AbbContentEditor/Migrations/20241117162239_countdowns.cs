using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbbContentEditor.Migrations
{
    /// <inheritdoc />
    public partial class countdowns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countdowns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countdowns", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdDate",
                value: new DateTime(2024, 11, 17, 16, 22, 35, 691, DateTimeKind.Utc).AddTicks(9131));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Countdowns");

            migrationBuilder.UpdateData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdDate",
                value: new DateTime(2024, 11, 9, 21, 8, 29, 239, DateTimeKind.Utc).AddTicks(3594));
        }
    }
}
