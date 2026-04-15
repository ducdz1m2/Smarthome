using Domain.Entities.Installation;

namespace Domain.Events
{
    public record TechnicianRatingCreatedEvent(
        int RatingId,
        int TechnicianId,
        int UserId,
        string Content,
        int Rating
        ) : DomainEvent;
}
