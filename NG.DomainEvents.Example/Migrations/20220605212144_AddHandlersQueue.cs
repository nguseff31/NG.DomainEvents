using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NG.DomainEvents.Example.Migrations
{
    public partial class AddHandlersQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityTableName",
                table: "domain_event");

            migrationBuilder.DropColumn(
                name: "ShouldExecute",
                table: "domain_event");

            migrationBuilder.RenameColumn(
                name: "Retries",
                table: "domain_event",
                newName: "Order");

            migrationBuilder.AddColumn<string>(
                name: "BucketId",
                table: "domain_event",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "domain_event_handler_queue",
                columns: table => new
                {
                    HandlerType = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    BucketId = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Enqueued = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domain_event_handler_queue", x => new { x.HandlerType, x.EventId });
                    table.ForeignKey(
                        name: "FK_domain_event_handler_queue_domain_event_EventId",
                        column: x => x.EventId,
                        principalTable: "domain_event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_domain_event_handler_queue_EventId",
                table: "domain_event_handler_queue",
                column: "EventId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_event_handler_queue");

            migrationBuilder.DropColumn(
                name: "BucketId",
                table: "domain_event");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "domain_event",
                newName: "Retries");

            migrationBuilder.AddColumn<string>(
                name: "EntityTableName",
                table: "domain_event",
                type: "character varying(63)",
                maxLength: 63,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShouldExecute",
                table: "domain_event",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
