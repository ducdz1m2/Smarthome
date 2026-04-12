using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Domain.ValueObjects;

namespace Domain.Persistence;

/// <summary>
/// EF Core Value Converters for Domain Value Objects.
/// These converters enable storing Value Objects in the database.
/// </summary>
public static class ValueConverters
{
    /// <summary>
    /// Converter for Money Value Object.
    /// Stores as decimal amount (currency is assumed VND by default).
    /// </summary>
    public static ValueConverter<Money, decimal> MoneyConverter => new(
        money => money.Amount,
        amount => Money.Vnd(amount));

    /// <summary>
    /// Converter for Money with currency stored separately.
    /// Use this when you need to store both amount and currency.
    /// </summary>
    public static ValueConverter<Money, string> MoneyWithCurrencyConverter => new(
        money => $"{money.Amount}|{money.Currency}",
        value => ConvertToMoney(value));

    private static Money ConvertToMoney(string value)
    {
        var parts = value.Split('|');
        return Money.Create(decimal.Parse(parts[0]), parts[1]);
    }

    /// <summary>
    /// Converter for Sku Value Object.
    /// Stores as uppercase string.
    /// </summary>
    public static ValueConverter<Sku, string> SkuConverter => new(
        sku => sku.Value,
        value => Sku.Create(value));

    /// <summary>
    /// Converter for PhoneNumber Value Object.
    /// Stores as normalized string.
    /// </summary>
    public static ValueConverter<PhoneNumber, string> PhoneNumberConverter => new(
        phone => phone.Value,
        value => PhoneNumber.Create(value));

    /// <summary>
    /// Converter for Email Value Object.
    /// Stores as lowercase string.
    /// </summary>
    public static ValueConverter<Email, string> EmailConverter => new(
        email => email.Value,
        value => Email.Create(value));

    /// <summary>
    /// Converter for Percentage Value Object.
    /// Stores as decimal value.
    /// </summary>
    public static ValueConverter<Percentage, decimal> PercentageConverter => new(
        percentage => percentage.Value,
        value => Percentage.Create(value));

    /// <summary>
    /// Converter for Weight Value Object.
    /// Stores as decimal value in kg.
    /// </summary>
    public static ValueConverter<Weight, decimal> WeightConverter => new(
        weight => weight.ValueInKg,
        value => Weight.FromKilograms(value));
}

/// <summary>
/// EF Core Value Comparer for complex Value Objects.
/// </summary>
public static class ValueComparers
{
    /// <summary>
    /// Compares Address Value Objects by their components.
    /// </summary>
    public static ValueComparer<Address> AddressComparer => new(
        (a1, a2) => a1 != null && a2 != null && a1.ToFullString() == a2.ToFullString(),
        address => address.ToFullString().GetHashCode());

    /// <summary>
    /// Compares Money Value Objects by amount and currency.
    /// </summary>
    public static ValueComparer<Money> MoneyComparer => new(
        (m1, m2) => m1 != null && m2 != null && m1.Amount == m2.Amount && m1.Currency == m2.Currency,
        money => HashCode.Combine(money.Amount, money.Currency));

    /// <summary>
    /// Compares Sku Value Objects.
    /// </summary>
    public static ValueComparer<Sku> SkuComparer => new(
        (s1, s2) => s1 != null && s2 != null && s1.Value == s2.Value,
        sku => sku.Value.GetHashCode());
}
