using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.ValueObjects;

namespace Domain.Persistence;

/// <summary>
/// Extension methods for configuring Domain entities with Value Objects.
/// </summary>
public static class EntityConfigurationExtensions
{
    /// <summary>
    /// Configures a Money property with converter.
    /// </summary>
    public static PropertyBuilder<Money> HasMoneyConversion(
        this PropertyBuilder<Money> builder,
        string columnName,
        int precision = 18,
        int scale = 2)
    {
        return builder
            .HasConversion(ValueConverters.MoneyConverter)
            .HasColumnName(columnName)
            .HasPrecision(precision, scale);
    }

    /// <summary>
    /// Configures a Sku property with converter.
    /// </summary>
    public static PropertyBuilder<Sku> HasSkuConversion(
        this PropertyBuilder<Sku> builder,
        string columnName,
        int maxLength = 50)
    {
        return builder
            .HasConversion(ValueConverters.SkuConverter)
            .HasColumnName(columnName)
            .HasMaxLength(maxLength);
    }

    /// <summary>
    /// Configures a PhoneNumber property with converter.
    /// </summary>
    public static PropertyBuilder<PhoneNumber> HasPhoneNumberConversion(
        this PropertyBuilder<PhoneNumber> builder,
        string columnName,
        int maxLength = 20)
    {
        return builder
            .HasConversion(ValueConverters.PhoneNumberConverter)
            .HasColumnName(columnName)
            .HasMaxLength(maxLength);
    }

    /// <summary>
    /// Configures an Email property with converter.
    /// </summary>
    public static PropertyBuilder<Email> HasEmailConversion(
        this PropertyBuilder<Email> builder,
        string columnName,
        int maxLength = 255)
    {
        return builder
            .HasConversion(ValueConverters.EmailConverter)
            .HasColumnName(columnName)
            .HasMaxLength(maxLength);
    }

    /// <summary>
    /// Configures a Percentage property with converter.
    /// </summary>
    public static PropertyBuilder<Percentage> HasPercentageConversion(
        this PropertyBuilder<Percentage> builder,
        string columnName,
        int precision = 5,
        int scale = 2)
    {
        return builder
            .HasConversion(ValueConverters.PercentageConverter)
            .HasColumnName(columnName)
            .HasPrecision(precision, scale);
    }

    /// <summary>
    /// Configures Address as owned entity with individual columns.
    /// </summary>
    public static EntityTypeBuilder<TEntity> OwnsAddress<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Address?>> navigationExpression,
        string prefix = "") where TEntity : class
    {
        var streetCol = string.IsNullOrEmpty(prefix) ? "AddressStreet" : $"{prefix}Street";
        var wardCol = string.IsNullOrEmpty(prefix) ? "AddressWard" : $"{prefix}Ward";
        var districtCol = string.IsNullOrEmpty(prefix) ? "AddressDistrict" : $"{prefix}District";
        var cityCol = string.IsNullOrEmpty(prefix) ? "AddressCity" : $"{prefix}City";
        var countryCol = string.IsNullOrEmpty(prefix) ? "AddressCountry" : $"{prefix}Country";
        var postalCodeCol = string.IsNullOrEmpty(prefix) ? "AddressPostalCode" : $"{prefix}PostalCode";

        builder.OwnsOne(navigationExpression, address =>
        {
            address.Property(a => a.Street).HasColumnName(streetCol).HasMaxLength(200);
            address.Property(a => a.Ward).HasColumnName(wardCol).HasMaxLength(100);
            address.Property(a => a.District).HasColumnName(districtCol).HasMaxLength(100);
            address.Property(a => a.City).HasColumnName(cityCol).HasMaxLength(100);
            address.Property(a => a.Country).HasColumnName(countryCol).HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName(postalCodeCol).HasMaxLength(20);
        });

        return builder;
    }

    /// <summary>
    /// Configures Address as JSON column (for databases that support JSON).
    /// </summary>
    public static PropertyBuilder<Address?> HasAddressJsonConversion<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Address?>> propertyExpression,
        string columnName = "AddressJson") where TEntity : class
    {
        return builder
            .Property(propertyExpression)
            .HasConversion(
                a => a == null
                    ? null
                    : System.Text.Json.JsonSerializer.Serialize(a, (System.Text.Json.JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? null
                    : System.Text.Json.JsonSerializer.Deserialize<Address>(json, (System.Text.Json.JsonSerializerOptions?)null))
            .HasColumnName(columnName);
    }
}
