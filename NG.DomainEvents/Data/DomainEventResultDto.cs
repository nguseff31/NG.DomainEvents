using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using NG.DomainEvents.Common;

namespace NG.DomainEvents.Data;

[Table("domain_event_result")]
public class DomainEventResultDto
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public long DomainEventId { get; set; }

    [Required]
    [StringLength(256)]
    public string Handler { get; set; }

    [Required]
    public DateTime DateExecuted { get; set; } = DateTime.UtcNow;

    [Required]
    public TimeSpan Elapsed { get; set; }

    [Required]
    public bool Completed { get; set; }

    [Column(TypeName = "jsonb")]
    public string Data { get; protected set; }

    [ForeignKey(nameof(DomainEventId))]
    [InverseProperty(nameof(DomainEventDto.Results))]
    public DomainEventDto DomainEvent { get; set; }

    public T? GetResult<T>() where T : DomainEventResult
    {
        return JsonSerializer.Deserialize<T>(Data);
    }

    public void SetResult<T>(T result) where T : DomainEventResult
    {
        Data = JsonSerializer.Serialize(result);
    }
}