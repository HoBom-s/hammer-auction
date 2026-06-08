using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hammer.Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRealEstateTrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "real_estate_trades",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lawd_cd = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    property_type = table.Column<int>(type: "integer", nullable: false),
                    building_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    jibun = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    umd_nm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    deal_amount = table.Column<long>(type: "bigint", nullable: false),
                    deal_year = table.Column<int>(type: "integer", nullable: false),
                    deal_month = table.Column<int>(type: "integer", nullable: false),
                    deal_day = table.Column<int>(type: "integer", nullable: false),
                    area = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    floor = table.Column<int>(type: "integer", nullable: true),
                    build_year = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_real_estate_trades", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_real_estate_trades_deal_year_deal_month_deal_day",
                table: "real_estate_trades",
                columns: new[] { "deal_year", "deal_month", "deal_day" });

            migrationBuilder.CreateIndex(
                name: "ix_real_estate_trades_lawd_cd",
                table: "real_estate_trades",
                column: "lawd_cd");

            migrationBuilder.CreateIndex(
                name: "ix_real_estate_trades_lawd_cd_property_type_jibun_deal_year_de",
                table: "real_estate_trades",
                columns: new[] { "lawd_cd", "property_type", "jibun", "deal_year", "deal_month", "deal_day", "area" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_real_estate_trades_property_type",
                table: "real_estate_trades",
                column: "property_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "real_estate_trades");
        }
    }
}
