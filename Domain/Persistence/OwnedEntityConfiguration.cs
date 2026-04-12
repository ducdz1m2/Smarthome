namespace Domain.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.ValueObjects;

/// <summary>
/// Extension methods for configuring Value Objects as owned entities in EF Core.
/// </summary>
public static class OwnedEntityConfiguration
{
    /// <summary>
    /// Configures Address as an owned entity with shadow property storage.
    /// </summary>
    public static OwnedNavigationBuilder<TEntity, Address> ConfigureAddress<TEntity>(
        this OwnedNavigationBuilder<TEntity, Address> builder) where TEntity : class
    {
        builder.Property(a => a.Street).HasColumnName("ShippingAddressStreet").HasMaxLength(200);
        builder.Property(a => a.Ward).HasColumnName("ShippingAddressWard").HasMaxLength(100);
        builder.Property(a => a.District).HasColumnName("ShippingAddressDistrict").HasMaxLength(100);
        builder.Property(a => a.City).HasColumnName("ShippingAddressCity").HasMaxLength(100);
        builder.Property(a => a.Country).HasColumnName("ShippingAddressCountry").HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasColumnName("ShippingAddressPostalCode").HasMaxLength(20);

        return builder;
    }

    /// <summary>
    /// Alternative: Configure Address stored as JSON in a single column.
    /// </summary>
    public static PropertyBuilder<Address> ConfigureAddressAsJson<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string propertyName,
        string columnName = "ShippingAddressJson") where TEntity : class
    {
        return builder
            .Property<Address>(propertyName)
            .HasConversion(
                a => System.Text.Json.JsonSerializer.Serialize(a, (System.Text.Json.JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? null!
                    : System.Text.Json.JsonSerializer.Deserialize<Address>(json, (System.Text.Json.JsonSerializerOptions?)null)!)
            .HasColumnName(columnName);
    }

    /// <summary>
    /// Configures Money as owned entity with Amount and Currency.
    /// </summary>
    public static OwnedNavigationBuilder<TEntity, Money> ConfigureMoney<TEntity>(
        this OwnedNavigationBuilder<TEntity, Money> builder,
        string amountColumnName,
        string currencyColumnName = "Currency") where TEntity : class
    {
        builder.Property(m => m.Amount).HasColumnName(amountColumnName).HasPrecision(18, 2);
        builder.Property(m => m.Currency).HasColumnName(currencyColumnName).HasMaxLength(3);

        return builder;
    }
}
