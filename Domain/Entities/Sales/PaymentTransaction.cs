namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// PaymentTransaction entity - tracks payment status for an order.
/// </summary>
public class PaymentTransaction : Entity
    {
        public int OrderId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public PaymentMethod Method { get; private set; }
        public PaymentTransactionStatus Status { get; private set; } = PaymentTransactionStatus.Pending;
        public string? TransactionCode { get; private set; }
        public string? GatewayResponse { get; private set; }
        public DateTime? PaidAt { get; private set; }

        private PaymentTransaction() { }

        public static PaymentTransaction Create(int orderId, Money amount, PaymentMethod method)
        {
            if (amount.IsLessThanOrEqualTo(Money.Zero()))
                throw new ValidationException(nameof(amount), "Số tiền phải lớn hơn 0");

            return new PaymentTransaction
            {
                OrderId = orderId,
                Amount = amount,
                Method = method,
                Status = PaymentTransactionStatus.Pending
            };
        }

        // Legacy overload for backward compatibility
        public static PaymentTransaction Create(int orderId, decimal amount, PaymentMethod method)
        {
            return Create(orderId, Money.Vnd(amount), method);
        }

        public void MarkSuccess(string transactionCode, string? gatewayResponse = null)
        {
            Status = PaymentTransactionStatus.Success;
            TransactionCode = transactionCode;
            GatewayResponse = gatewayResponse;
            PaidAt = DateTime.UtcNow;
        }

        public void MarkFailed(string reason)
        {
            Status = PaymentTransactionStatus.Failed;
            GatewayResponse = reason;
        }

        public void MarkRefunded(string refundTransactionCode)
        {
            Status = PaymentTransactionStatus.Refunded;
            TransactionCode = refundTransactionCode;
        }
    }

    public enum PaymentTransactionStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Refunded = 3
    }
