using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbbContentEditor.Migrations
{
    /// <inheritdoc />
    public partial class test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JsonDataString",
                table: "WordCollections",
                newName: "WordsCollectionString");

            migrationBuilder.UpdateData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdDate",
                value: new DateTime(2025, 2, 9, 21, 53, 51, 917, DateTimeKind.Utc).AddTicks(267));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WordsCollectionString",
                table: "WordCollections",
                newName: "JsonDataString");

            migrationBuilder.UpdateData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdDate",
                value: new DateTime(2025, 2, 9, 8, 48, 7, 256, DateTimeKind.Utc).AddTicks(8085));
        }
    }
}
