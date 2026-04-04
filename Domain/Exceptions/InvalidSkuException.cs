namespace Domain.Exceptions
{
    public class InvalidSkuException : DomainException
    {
        public string AttemptedValue { get; }

        public InvalidSkuException(string attemptedValue, string reason)
            : base($"SKU không hợp lệ '{attemptedValue}': {reason}")
        {
            AttemptedValue = attemptedValue;
        }
    }
}