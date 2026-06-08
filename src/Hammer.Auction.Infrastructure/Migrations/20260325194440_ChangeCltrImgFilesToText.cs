using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCltrImgFilesToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "cltr_img_files",
                table: "kamco_auction_items",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "cltr_img_files",
                table: "kamco_auction_items",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
