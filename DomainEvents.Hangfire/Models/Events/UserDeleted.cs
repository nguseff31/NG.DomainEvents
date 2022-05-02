using DomainEvents.Hangfire.Models.Domain;
using NG.DomainEvents.Common;

namespace DomainEvents.Hangfire.Models.Events;

public class UserDeleted : DomainEvent
{
    public override void SetEntity(object entity)
    {
        var user = SetEntity<UserDto>(entity);
        EntityId = user.Id.ToString();
    }
}