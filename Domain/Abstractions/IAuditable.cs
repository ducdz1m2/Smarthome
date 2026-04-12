namespace Domain.Abstractions;

/// <summary>
/// Interface for entities that support auditing.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; }
    string CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    string? UpdatedBy { get; }
}

/// <summary>
/// Interface for entities that support soft delete.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    string? DeletedBy { get; }
    void MarkAsDeleted(string deletedBy);
    void Restore();
}

/// <summary>
/// Interface for entities that support activation state.
/// </summary>
public interface IActivatable
{
    bool IsActive { get; }
    void Activate();
    void Deactivate();
}
