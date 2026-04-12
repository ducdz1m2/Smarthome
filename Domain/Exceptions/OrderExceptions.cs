namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when an order is in an invalid state for an operation.
/// </summary>
public class InvalidOrderStateException : DomainException
{
    public string CurrentState { get; }
    public string AttemptedOperation { get; }

    public InvalidOrderStateException(string currentState, string attemptedOperation)
        : base("InvalidOrderState", $"Cannot {attemptedOperation} order in '{currentState}' state.")
    {
        CurrentState = currentState;
        AttemptedOperation = attemptedOperation;
    }
}

/// <summary>
/// Exception thrown when payment fails.
/// </summary>
public class PaymentFailedException : DomainException
{
    public string PaymentMethod { get; }
    public decimal Amount { get; }

    public PaymentFailedException(string paymentMethod, decimal amount, string reason)
        : base("PaymentFailed", $"Payment of {amount} via {paymentMethod} failed: {reason}")
    {
        PaymentMethod = paymentMethod;
        Amount = amount;
    }
}

/// <summary>
/// Exception thrown when a coupon is invalid or expired.
/// </summary>
public class InvalidCouponException : DomainException
{
    public string CouponCode { get; }

    public InvalidCouponException(string couponCode, string reason)
        : base("InvalidCoupon", $"Coupon '{couponCode}' is invalid: {reason}")
    {
        CouponCode = couponCode;
    }
}
