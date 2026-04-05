namespace Domain.Exceptions
{
    public class InvalidQuantityException : DomainException
    {
        public int AttemptedQuantity { get; }
        public string Operation { get; }

        public InvalidQuantityException(int quantity, string operation)
            : base($"Số lượng {quantity} không hợp lệ cho thao tác '{operation}'")
        {
            AttemptedQuantity = quantity;
            Operation = operation;
        }
    }
}
