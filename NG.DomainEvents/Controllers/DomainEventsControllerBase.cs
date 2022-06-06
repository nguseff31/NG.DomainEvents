using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NG.DomainEvents.Data;

namespace NG.DomainEvents.Controllers;

public abstract class DomainEventsControllerBase<TDbContext> : ControllerBase
    where TDbContext : DomainEventsDbContext<TDbContext> {
    private TDbContext _dbContext;

    public DomainEventsControllerBase(TDbContext dbContext) {
        _dbContext = dbContext;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetEventInfo(long id, CancellationToken cancellationToken) {
        var @event = await _dbContext
            .Events
            .Include(e => e.Results)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return Ok(@event);
    }

    [HttpGet]
    public async Task<ActionResult> GetFailedEvents() {
        throw new NotImplementedException(); // todo need another property on event
    }
}
