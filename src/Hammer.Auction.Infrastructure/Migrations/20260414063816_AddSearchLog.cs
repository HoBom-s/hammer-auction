using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hammer.Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    keyword = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    searched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_search_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_search_logs_keyword",
                table: "search_logs",
                column: "keyword");

            migrationBuilder.CreateIndex(
                name: "ix_search_logs_searched_at",
                table: "search_logs",
                column: "searched_at");

            migrationBuilder.CreateIndex(
                name: "ix_search_logs_user_id_searched_at",
                table: "search_logs",
                columns: new[] { "user_id", "searched_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_logs");
        }
    }
}
