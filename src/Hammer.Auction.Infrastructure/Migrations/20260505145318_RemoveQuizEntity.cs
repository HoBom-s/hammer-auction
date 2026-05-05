using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuizEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Quiz table is managed by hammer-internal (Flyway) — no DROP TABLE here.
            // Only drop the FK from quiz_attempts since this service no longer tracks the relationship.
            migrationBuilder.DropForeignKey(
                name: "fk_quiz_attempts_quizzes_quiz_id",
                table: "quiz_attempts");

            migrationBuilder.DropIndex(
                name: "ix_quiz_attempts_quiz_id",
                table: "quiz_attempts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_quiz_attempts_quiz_id",
                table: "quiz_attempts",
                column: "quiz_id");

            migrationBuilder.AddForeignKey(
                name: "fk_quiz_attempts_quizzes_quiz_id",
                table: "quiz_attempts",
                column: "quiz_id",
                principalTable: "quizzes",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
