using System.Text.Json.Serialization;
using MediatR;

namespace NG.DomainEvents.Common;

public abstract class DomainEvent : INotification
{
    [JsonIgnore]
    public long Id { get; set; }
    
    public string EntityId { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public abstract void SetEntity(object entity);

    protected TEntity SetEntity<TEntity>(object entity)
    {
        if (entity is not TEntity)
        {
            throw new ArgumentException();
        }

        return (TEntity)entity;
    }
}