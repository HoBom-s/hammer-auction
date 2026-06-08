using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hammer.Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionAuctionAndCodeInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "institution_auction_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plnm_no = table.Column<long>(type: "bigint", nullable: false),
                    pbct_no = table.Column<long>(type: "bigint", nullable: false),
                    plnm_kind_cd = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plnm_kind_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    bid_dvsn_cd = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bid_dvsn_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    plnm_nm = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    org_nm = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    plnm_dt = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    org_plnm_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    plnm_mnmt_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    bid_mtd_cd = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bid_mtd_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tot_amt_unpc_dvsn_cd = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tot_amt_unpc_dvsn_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dpsl_mtd_cd = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    dpsl_mtd_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    prpt_dvsn_cd = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    prpt_dvsn_nm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    pbct_begn_dtm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    pbct_cls_dtm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    pbct_exct_dtm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ctgr_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ctgr_full_nm = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_institution_auction_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "onbid_code_infos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ctgr_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ctgr_nm = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ctgr_hirk_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ctgr_hirk_nm = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_onbid_code_infos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_institution_auction_items_ctgr_id",
                table: "institution_auction_items",
                column: "ctgr_id");

            migrationBuilder.CreateIndex(
                name: "ix_institution_auction_items_org_nm",
                table: "institution_auction_items",
                column: "org_nm");

            migrationBuilder.CreateIndex(
                name: "ix_institution_auction_items_pbct_cls_dtm",
                table: "institution_auction_items",
                column: "pbct_cls_dtm");

            migrationBuilder.CreateIndex(
                name: "ix_institution_auction_items_plnm_no_pbct_no",
                table: "institution_auction_items",
                columns: new[] { "plnm_no", "pbct_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_onbid_code_infos_ctgr_id",
                table: "onbid_code_infos",
                column: "ctgr_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "institution_auction_items");

            migrationBuilder.DropTable(
                name: "onbid_code_infos");
        }
    }
}
