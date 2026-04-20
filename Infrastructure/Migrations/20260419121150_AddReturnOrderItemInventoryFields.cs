using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnOrderItemInventoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDamaged",
                table: "ReturnOrderItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ReturnOrderItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ReturnedToInventory",
                table: "ReturnOrderItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "ReturnOrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "ReturnOrderItem",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDamaged",
                table: "ReturnOrderItem");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ReturnOrderItem");

            migrationBuilder.DropColumn(
                name: "ReturnedToInventory",
                table: "ReturnOrderItem");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "ReturnOrderItem");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "ReturnOrderItem");
        }
    }
}
