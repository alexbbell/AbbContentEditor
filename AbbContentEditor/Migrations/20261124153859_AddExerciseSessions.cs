using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AbbContentEditor.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "WordHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExerciseSessionId",
                table: "WordHistories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedAnswer",
                table: "WordHistories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WordId",
                table: "WordHistories",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExerciseSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentityUserId = table.Column<string>(type: "text", nullable: false),
                    WordCollectionId = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseSessions_Abb_Users_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "Abb_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdDate",
                value: new DateTime(2026, 11, 24, 15, 38, 55, 10, DateTimeKind.Utc).AddTicks(9340));

            migrationBuilder.CreateIndex(
                name: "IX_WordHistories_ExerciseSessionId",
                table: "WordHistories",
                column: "ExerciseSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSessions_IdentityUserId_StartedAt",
                table: "ExerciseSessions",
                columns: new[] { "IdentityUserId", "StartedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_WordHistories_ExerciseSessions_ExerciseSessionId",
                table: "WordHistories",
                column: "ExerciseSessionId",
                principalTable: "ExerciseSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WordHistories_ExerciseSessions_ExerciseSessionId",
                table: "WordHistories");

            migrationBuilder.DropTable(
                name: "ExerciseSessions");

            migrationBuilder.DropIndex(
                name: "IX_WordHistories_ExerciseSessionId",
                table: "WordHistories");

            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "WordHistories");

            migrationBuilder.DropColumn(
                name: "ExerciseSessionId",
                table: "WordHistories");

            migrationBuilder.DropColumn(
                name: "SelectedAnswer",
                table: "WordHistories");

            migrationBuilder.DropColumn(
                name: "WordId",
                table: "WordHistories");

            migrationBuilder.UpdateData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdDate",
                value: new DateTime(2026, 11, 24, 14, 43, 40, 896, DateTimeKind.Utc).AddTicks(3647));
        }
    }
}
