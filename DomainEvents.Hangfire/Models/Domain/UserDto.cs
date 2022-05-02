using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DomainEvents.Hangfire.Models.Events;
using NG.DomainEvents.Data;

namespace DomainEvents.Hangfire.Models.Domain;

[Table("users")]
public class UserDto : EntityBase
{
    public UserDto(int id, string firstName, string lastName, string? middleName, string phone, string? email, bool isDeleted)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Phone = phone;
        Email = email;
        IsDeleted = isDeleted;
    }

    public static UserDto CreateUser(string firstName, string lastName, string? middleName, string phone, string? email, bool isDeleted)
    {
        var user = new UserDto
        (
            id: 0,
            firstName: firstName,
            lastName: lastName,
            middleName: middleName,
            email: email,
            phone: phone,
            isDeleted: isDeleted
        );
        user.AddDomainEvent(new UserCreated());
        return user;
    }

    [Key]
    public int Id { get; protected set; }
    
    [Required]
    public string FirstName { get; protected set; }
    
    [Required]
    public string LastName { get; protected set; }
    
    public string? MiddleName { get; protected set; }
    
    [Required]
    public string Phone { get; protected set; }
    
    public string? Email { get; protected set; }
    
    [Required]
    [DefaultValue(false)]
    public bool IsDeleted { get; protected set; }

    public void Delete()
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            AddDomainEvent(new UserDeleted());
        }
    }

    public override string GetEntityId()
    {
        return Id.ToString();
    }
}