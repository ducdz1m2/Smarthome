namespace Domain.Exceptions
{
    public class ValidationException : DomainException
    {
        public string PropertyName { get; }

        public ValidationException(string propertyName, string message)
            : base($"{propertyName}: {message}")
        {
            PropertyName = propertyName;
        }
    }
}
