using DomainEvents.Hangfire.Models.Domain;
using NG.DomainEvents.Common;

namespace DomainEvents.Hangfire.Models.Events;

public class UserDomainEvent : DomainEvent
{
    public int UserId { get; set; }
    
    public override void SetEntity(object entity)
    {
        var user = SetEntity<UserDto>(entity);
        UserId = user.Id;
    }
}