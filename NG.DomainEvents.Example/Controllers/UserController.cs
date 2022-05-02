using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NG.DomainEvents.Example.Models.Domain;
using NG.DomainEvents.Example.ViewModel;

namespace NG.DomainEvents.Example.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly TestDbContext _context;

    public UserController(TestDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users.ToArrayAsync();
        return Ok(users);
    }
    
    [HttpPost]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromBody]UserCreate user)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        var dto = UserDto.CreateUser(
            firstName: user.FirstName,
            lastName: user.LastName,
            middleName: user.MiddleName,
            phone: user.Phone,
            email: user.Email,
            isDeleted: false);
        _context.Add(dto);
        await _context.SaveChangesAsync();
        var id = dto.Id;
        transaction.Commit();
        return Ok(id);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        user.Delete();
        await _context.SaveChangesAsync();
        transaction.Commit();
        return Ok(id);
    }
}