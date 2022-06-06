using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NG.DomainEvents.Example.Migrations
{
    public partial class AddQueueTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION domain_event_handler_queue_insert() RETURNS TRIGGER AS $$
    BEGIN
        IF (TG_OP = 'INSERT') THEN
            INSERT INTO domain_event_handler_queue (""HandlerType"", ""EventId"",""Order"", ""BucketId"", ""Created"")
                VALUES (NEW.""HandlerType"", NEW.""EventId"", NEW.""Order"", NEW.""BucketId"", NEW.""Created"")
                ON CONFLICT DO NOTHING;
        END IF;
        RETURN NEW;
    END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER domain_event_handler_queue_inserted
AFTER INSERT OR UPDATE OR DELETE ON domain_event_handler_queue
    FOR EACH ROW EXECUTE PROCEDURE domain_event_handler_queue_insert();
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS domain_event_handler_queue_inserted ON domain_event_handler_queue");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS domain_event_handler_queue_insert");
        }
    }
}
