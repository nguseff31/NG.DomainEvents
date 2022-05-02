using System;
using System.ComponentModel.DataAnnotations;
using NG.DomainEvents.Data;

namespace NG.DomainEvents.Tests.Model;

public class ArticleDto : EntityBase
{
    [Key]
    public int Id { get; set; }
    
    public string Title { get; set; }
    
    public string Content { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow; 
    
    public override string GetEntityId()
    {
        return Id.ToString();
    }
}