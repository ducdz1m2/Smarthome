namespace Domain.Exceptions
{
    public class InvalidOrderStateException(string currentStatus, string attemptedAction) : DomainException($"Không thể {attemptedAction} khi đơn hàng đang ở trạng thái {currentStatus}")
    {
        public string CurrentStatus { get; } = currentStatus;
        public string AttemptedAction { get; } = attemptedAction;
    }
}