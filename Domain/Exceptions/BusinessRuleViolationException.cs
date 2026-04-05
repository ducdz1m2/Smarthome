namespace Domain.Exceptions
{
    public class BusinessRuleViolationException : DomainException
    {
        public string RuleName { get; }

        public BusinessRuleViolationException(string ruleName, string message)
            : base($"Vi phạm quy tắc '{ruleName}': {message}")
        {
            RuleName = ruleName;
        }
    }
}
