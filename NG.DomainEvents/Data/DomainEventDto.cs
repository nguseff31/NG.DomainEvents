using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using NG.DomainEvents.Common;

// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace NG.DomainEvents.Data;

[Table("domain_event")]
public class DomainEventDto {
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(256)]
    public string EventType { get; set; }

    [Required]
    public int Order { get; set; } = 0;

    [Required]
    [StringLength(36)]
    public string EntityId { get; set; }

    [Required]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "jsonb")]
    public string Data { get; set; }

    [Required]
    public string BucketId { get; set; } = Guid.Empty.ToString();

    [InverseProperty(nameof(DomainEventResultDto.DomainEvent))]
    public virtual ICollection<DomainEventResultDto> Results { get; set; } = new List<DomainEventResultDto>();

    public void SetEvent(DomainEvent domainEvent) {
        Data = JsonSerializer.Serialize(domainEvent);
    }

    public DomainEvent? GetEvent(Type eventType) {
        var @event = JsonSerializer.Deserialize(Data, eventType) as DomainEvent;
        if (@event != null) {
            @event.Id = Id;
        }

        return @event;
    }
}
