using Microsoft.AspNetCore.Components;
using NG.DomainEvents.Controllers;

namespace NG.DomainEvents.Example.Controllers;

[Route("domain_events")]
public class DomainEventsController : DomainEventsControllerBase<TestDbContext>
{
    public DomainEventsController(TestDbContext dbContext) : base(dbContext)
    {
    }
}