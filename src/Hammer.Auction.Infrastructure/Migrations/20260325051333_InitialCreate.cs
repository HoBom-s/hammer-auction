using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hammer.Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kamco_auction_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plnm_no = table.Column<long>(type: "bigint", nullable: false),
                    pbct_no = table.Column<long>(type: "bigint", nullable: false),
                    cltr_no = table.Column<long>(type: "bigint", nullable: false),
                    cltr_nm = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ctgr_full_nm = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ldnm_adrs = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    nmrd_adrs = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    min_bid_prc = table.Column<long>(type: "bigint", nullable: false),
                    apsl_ases_avg_amt = table.Column<long>(type: "bigint", nullable: false),
                    bid_mtd_nm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pbct_cltr_stat_nm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pbct_begn_dtm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    pbct_cls_dtm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    uscbd_cnt = table.Column<int>(type: "integer", nullable: false),
                    iqry_cnt = table.Column<int>(type: "integer", nullable: false),
                    cltr_img_files = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kamco_auction_items", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_kamco_auction_items_ctgr_full_nm",
                table: "kamco_auction_items",
                column: "ctgr_full_nm");

            migrationBuilder.CreateIndex(
                name: "ix_kamco_auction_items_min_bid_prc",
                table: "kamco_auction_items",
                column: "min_bid_prc");

            migrationBuilder.CreateIndex(
                name: "ix_kamco_auction_items_pbct_cls_dtm",
                table: "kamco_auction_items",
                column: "pbct_cls_dtm");

            migrationBuilder.CreateIndex(
                name: "ix_kamco_auction_items_pbct_cltr_stat_nm",
                table: "kamco_auction_items",
                column: "pbct_cltr_stat_nm");

            migrationBuilder.CreateIndex(
                name: "ix_kamco_auction_items_plnm_no_pbct_no_cltr_no",
                table: "kamco_auction_items",
                columns: new[] { "plnm_no", "pbct_no", "cltr_no" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kamco_auction_items");
        }
    }
}
