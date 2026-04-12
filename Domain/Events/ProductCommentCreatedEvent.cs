using Domain.Entities.Catalog;

namespace Domain.Events
{
    public record ProductCommentCreatedEvent(
        int CommentId,
        int ProductId,
        int UserId,
        string Content,
        int Rating
        ) : DomainEvent;
}