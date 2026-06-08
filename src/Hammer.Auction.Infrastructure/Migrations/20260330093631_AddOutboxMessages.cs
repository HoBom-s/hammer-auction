using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    payload = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at",
                filter: "processed_at IS NULL");

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION notify_outbox_ready()
                RETURNS trigger AS $$
                BEGIN
                    PERFORM pg_notify('outbox_ready', '');
                    RETURN NULL;
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER trg_outbox_notify
                AFTER INSERT ON outbox_messages
                FOR EACH STATEMENT
                EXECUTE FUNCTION notify_outbox_ready();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_outbox_notify ON outbox_messages;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS notify_outbox_ready();");

            migrationBuilder.DropTable(
                name: "outbox_messages");
        }
    }
}
