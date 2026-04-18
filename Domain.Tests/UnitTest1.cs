using Domain.Entities.Catalog;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Khi_AddStock_Cho_Variant_Thi_Tong_Kho_Cua_Product_Phai_Cap_Nhat()
        {
            // 1. Arrange
            var product = Product.Create("IPhone 15", "IPH-001", 1, 1);
            var v1 = product.AddVariant("IPH-001-RED", Money.Vnd(1000000), new());
            var v2 = product.AddVariant("IPH-001-BLU", Money.Vnd(1000000), new());

            // 2. Act
            product.AddStockToVariant("IPH-001-RED", 10);
            product.AddStockToVariant("IPH-001-BLU", 5);

            // 3. Assert
            product.StockQuantity.Should().Be(15);
            v1.StockQuantity.Should().Be(10);
        }

        [Fact]
        public void Khi_ReserveStock_Vuot_Muc_Available_Cua_Variant_Thi_Phai_Bao_Loi()
        {
            var product = Product.Create("Test Product", "TST-001", 1, 1);
            product.AddVariant("TST-001-VAR", Money.Vnd(100000), new());
            product.AddStockToVariant("TST-001-VAR", 5);

            Action act = () => product.ReserveStockForVariant("TST-001-VAR", 10);

            act.Should().Throw<InsufficientStockException>();
        }

        [Fact]
        public void Product_Phi_Ban_Event_Sau_Khi_Dong_Bo_Stock()
        {
            var product = Product.Create("Test Product", "TST-002", 1, 1);
            product.AddVariant("TST-002-VAR", Money.Vnd(100000), new());

            product.AddStockToVariant("TST-002-VAR", 10);

            product.DomainEvents.Should().Contain(e => e is ProductStockSynchronizedEvent);
        }

        [Fact]
        public void Product_Phi_Co_Ten_Hop_Le()
        {

            string name = " Laptop Gaming ";
            string sku = "LPG-001";

            var product = Product.Create(name, sku, 1, 1);

            product.Name.Should().Be("Laptop Gaming");
            product.Sku.Value.Should().Be("LPG-001");

            product.DomainEvents.Should().Contain(e => e is ProductCreatedEvent);

        }
    }
}
