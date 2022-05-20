using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NG.DomainEvents.Common;
using NG.DomainEvents.Data;
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace NG.DomainEvents.Tests.Model;

[Table("article")]
public class ArticleDto : EntityBase {
    [Key]
    public int Id { get; set; }

    public string Title { get; set; }

    public bool Published { get; set; }

    public ArticleDto() {
        AddDomainEvent(new ArticleCreated());
    }

    public override string GetEntityId() {
        return Id.ToString();
    }
}

public class ArticleCreated : DomainEvent {
    public override void SetEntity(object entity)
    {
        var user = SetEntity<ArticleDto>(entity);
        EntityId = user.Id.ToString();
    }
}
