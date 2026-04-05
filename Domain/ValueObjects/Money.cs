using Domain.Exceptions;

namespace Domain.ValueObjects
{
    public sealed record Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new DomainException("Số tiền không thể âm");

            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new DomainException("Mã tiền tệ phải là 3 ký tự (VND, USD...)");

            Amount = amount;
            Currency = currency.ToUpper();
        }

        // Factory methods
        public static Money Create(decimal amount, string currency = "VND") =>
            new(amount, currency);

        public static Money Vnd(decimal amount) =>
            new(amount, "VND");

        public static Money Zero(string currency = "VND") =>
            new(0, currency);

        // Business operations
        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new DomainException($"Không thể cộng {Currency} với {other.Currency}");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new DomainException($"Không thể trừ {Currency} với {other.Currency}");

            var result = Amount - other.Amount;
            if (result < 0)
                throw new DomainException("Kết quả không thể âm");

            return new Money(result, Currency);
        }

        public Money ApplyDiscount(Percentage discount)
        {
            var discountAmount = Amount * (discount.Value / 100);
            return new Money(Amount - discountAmount, Currency);
        }

        public Money ApplyDiscount(decimal discountPercent)
        {
            if (discountPercent < 0 || discountPercent > 100)
                throw new DomainException("Phần trăm giảm giá phải từ 0-100");

            var discountAmount = Amount * (discountPercent / 100);
            return new Money(Amount - discountAmount, Currency);
        }

        public Money Multiply(int quantity)
        {
            if (quantity < 0)
                throw new DomainException("Số lượng không thể âm");

            return new Money(Amount * quantity, Currency);
        }

        // Comparisons
        public bool IsGreaterThan(Money other) =>
            Currency == other.Currency && Amount > other.Amount;

        public bool IsLessThan(Money other) =>
            Currency == other.Currency && Amount < other.Amount;

       
        // Formatting
        public string ToVndString() =>
            $"{Amount:N0} ₫";

        public override string ToString() =>
            $"{Amount:N2} {Currency}";

        // EF Core constructor
        private Money()
        {
            Currency = "VND";
        }
    }
}