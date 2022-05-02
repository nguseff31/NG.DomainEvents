using NG.DomainEvents.Common;
using NG.DomainEvents.Example.Models.Domain;

namespace NG.DomainEvents.Example.Models.Events;

public class UserDomainEvent : DomainEvent
{
    public int UserId { get; set; }
    
    public override void SetEntity(object entity)
    {
        var user = SetEntity<UserDto>(entity);
        UserId = user.Id;
    }
}