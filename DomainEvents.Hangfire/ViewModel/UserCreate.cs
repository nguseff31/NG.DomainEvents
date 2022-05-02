namespace DomainEvents.Hangfire.ViewModel;

public class UserCreate
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string? MiddleName { get; set; }
    
    public string Phone { get; set; }
    
    public string? Email { get; set; }
}