namespace Domain.Exceptions
{
    public class EntityNotFoundException : DomainException
    {
        public string EntityType { get; }
        public int EntityId { get; }

        public EntityNotFoundException(string entityType, int entityId)
            : base($"Không tìm thấy {entityType} với Id = {entityId}")
        {
            EntityType = entityType;
            EntityId = entityId;
        }
    }
}
