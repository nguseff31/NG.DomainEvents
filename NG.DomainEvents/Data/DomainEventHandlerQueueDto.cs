using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#pragma warning disable CS8618

namespace NG.DomainEvents.Data;

[Table("domain_event_handler_queue")]
public class DomainEventHandlerQueueDto {
    public string HandlerType { get; set; }

    public long EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public DomainEventDto Event { get; set; }

    [Required]
    public int Order { get; set; }

    [Required]
    public string BucketId { get; set; } = Guid.Empty.ToString();

    [Required]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime? Enqueued { get;set; }
}

