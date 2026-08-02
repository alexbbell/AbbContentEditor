using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbbContentEditor.Migrations
{
    /// <inheritdoc />
    public partial class InitialStaticModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdDate",
                value: new DateTime(2026, 8, 2, 9, 17, 29, 785, DateTimeKind.Utc).AddTicks(4943));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdDate",
                value: new DateTime(2026, 8, 2, 9, 13, 32, 819, DateTimeKind.Utc).AddTicks(8957));
        }
    }
}
