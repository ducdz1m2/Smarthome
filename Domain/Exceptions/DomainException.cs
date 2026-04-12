namespace Domain.Exceptions;

/// <summary>
/// Base exception for all domain-related errors.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }
    public string? PropertyName { get; }

    public DomainException(string message)
        : base(message)
    {
        ErrorCode = GetType().Name;
    }

    public DomainException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = GetType().Name;
    }

    public DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    protected DomainException(string errorCode, string message, string? propertyName)
        : base(message)
    {
        ErrorCode = errorCode;
        PropertyName = propertyName;
    }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with id '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message)
        : base($"Business rule violated: {ruleName}. {message}")
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string propertyName, string message)
        : base("ValidationFailed", $"{propertyName}: {message}", propertyName)
    {
        Errors = new Dictionary<string, string[]> { [propertyName] = new[] { message } };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("ValidationFailed", "One or more validation errors occurred.")
    {
        Errors = errors.AsReadOnly();
    }

    public ValidationException(string message)
        : base("ValidationFailed", message)
    {
        Errors = new Dictionary<string, string[]>();
    }
}

/// <summary>
/// Exception thrown when a concurrency conflict occurs.
/// </summary>
public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string entityType, object entityId)
        : base("ConcurrencyConflict", $"The {entityType} with id '{entityId}' was modified by another user.")
    {
    }
}

/// <summary>
/// Exception thrown when an invalid operation is attempted.
/// </summary>
public class InvalidOperationDomainException : DomainException
{
    public string Operation { get; }
    public string Reason { get; }

    public InvalidOperationDomainException(string operation, string reason)
        : base("InvalidOperation", $"Cannot perform '{operation}': {reason}")
    {
        Operation = operation;
        Reason = reason;
    }
}