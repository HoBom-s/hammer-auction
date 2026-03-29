using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hammer.Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuizSeedAndAddQuizAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "quizzes",
                keyColumn: "id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "quizzes",
                keyColumn: "id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "quizzes",
                keyColumn: "id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "quizzes",
                keyColumn: "id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "quizzes",
                keyColumn: "id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "quizzes",
                keyColumn: "id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "quizzes",
                keyColumn: "id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "quizzes",
                keyColumn: "id",
                keyValue: 8L);

            migrationBuilder.CreateTable(
                name: "quiz_attempts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quiz_id = table.Column<long>(type: "bigint", nullable: true),
                    selected_index = table.Column<int>(type: "integer", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
                    attempted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_attempts_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_quiz_attempts_quiz_id",
                table: "quiz_attempts",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_attempts_user_id",
                table: "quiz_attempts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_attempts_user_id_quiz_id",
                table: "quiz_attempts",
                columns: new[] { "user_id", "quiz_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quiz_attempts");

            migrationBuilder.InsertData(
                table: "quizzes",
                columns: new[] { "id", "choice1", "choice2", "choice3", "choice4", "correct_index", "created_at", "explanation", "question" },
                values: new object[,]
                {
                    { 1L, "공매는 공공기관이, 경매는 법원이 진행한다", "공매는 법원이, 경매는 공공기관이 진행한다", "공매와 경매는 동일한 절차이다", "공매는 부동산만, 경매는 동산만 취급한다", 0, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "공매는 한국자산관리공사(KAMCO) 등 공공기관이 체납세금 등의 사유로 압류한 재산을 매각하는 절차이고, 경매는 법원이 채권자의 신청에 따라 진행하는 강제집행 절차입니다.", "공매와 경매의 가장 큰 차이점은 무엇인가요?" },
                    { 2L, "감정평가서 확인", "회원가입 및 공동인증서 등록", "입찰보증금 납부", "현장 답사", 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "온비드에서 입찰에 참여하려면 먼저 회원가입을 하고 공동인증서(구 공인인증서)를 등록해야 합니다. 이후 물건 검색, 입찰보증금 납부, 입찰서 제출 순으로 진행됩니다.", "온비드에서 입찰에 참여하기 위한 첫 번째 단계는?" },
                    { 3L, "낙찰자가 대금을 납부한 상태", "입찰자가 없거나 최저가 미달로 매각이 이루어지지 않은 것", "입찰이 성공적으로 완료된 상태", "물건이 취소된 상태", 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "유찰이란 입찰에 참여한 사람이 없거나, 입찰가가 최저입찰가에 미달하여 매각이 성립되지 않은 것을 말합니다. 유찰되면 차회 공매 시 최저입찰가가 인하될 수 있습니다.", "공매에서 '유찰'이란 무엇을 의미하나요?" },
                    { 4L, "감정가와 최저입찰가는 항상 동일하다", "최저입찰가는 감정가보다 항상 높다", "최저입찰가는 감정가 이하로 설정되며, 유찰 시 더 낮아질 수 있다", "감정가는 입찰과 무관하다", 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "감정가는 감정평가사가 산정한 물건의 가치이고, 최저입찰가는 이를 기준으로 설정됩니다. 첫 공매 시 감정가의 일정 비율로 시작하며, 유찰될 때마다 단계적으로 인하됩니다.", "감정가와 최저입찰가의 관계로 올바른 것은?" },
                    { 5L, "7일 이내", "30일 이내", "60일 이내", "90일 이내", 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "공매 낙찰 후 잔금은 일반적으로 납부기한 내(통상 60일)에 납부해야 합니다. 기한 내 미납 시 매각결정이 취소되고 입찰보증금이 국고에 귀속됩니다.", "공매 낙찰 후 잔금 납부 기한은 일반적으로 얼마인가요?" },
                    { 6L, "5%", "10%", "20%", "30%", 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "공매 입찰보증금은 최저입찰가의 10%입니다. 입찰보증금은 입찰 전에 납부해야 하며, 유찰되거나 낙찰되지 않은 경우 환불됩니다.", "공매 입찰 시 입찰보증금은 최저입찰가의 몇 %인가요?" },
                    { 7L, "물건의 색상을 확인하기 위해", "낙찰 후에도 소멸되지 않는 권리(임차권, 유치권 등)가 있을 수 있기 때문", "입찰가를 자동으로 계산해주기 때문", "세금을 면제받기 위해", 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "권리분석은 해당 물건에 설정된 저당권, 가압류, 임차권, 유치권 등을 파악하는 과정입니다. 낙찰 후에도 소멸되지 않는 권리가 있으면 추가 비용이 발생할 수 있어 반드시 사전에 확인해야 합니다.", "공매 물건의 '권리분석'이 중요한 이유는?" },
                    { 8L, "아파트", "토지", "자동차", "상가건물", 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "자동차는 '동산'으로 분류됩니다. 부동산은 토지, 주거용 건물(아파트, 주택 등), 상가 및 업무용 건물 등을 포함합니다.", "다음 중 공매에서 '부동산'에 해당하지 않는 것은?" }
                });
        }
    }
}
