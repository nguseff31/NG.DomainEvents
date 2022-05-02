using NG.DomainEvents.Common;
using NG.DomainEvents.Example.Models.Domain;

namespace NG.DomainEvents.Example.Models.Events;

public class UserCreated : DomainEvent
{
    public override void SetEntity(object entity)
    {
        var user = SetEntity<UserDto>(entity);
        EntityId = user.Id.ToString();
    }
}

