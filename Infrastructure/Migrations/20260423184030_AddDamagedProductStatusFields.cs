using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDamagedProductStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DamagedStatus",
                table: "WarrantyRequestItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RepairCost",
                table: "WarrantyRequestItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepairNotes",
                table: "WarrantyRequestItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "WarrantyRequestItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamagedStatus",
                table: "WarrantyRequestItems");

            migrationBuilder.DropColumn(
                name: "RepairCost",
                table: "WarrantyRequestItems");

            migrationBuilder.DropColumn(
                name: "RepairNotes",
                table: "WarrantyRequestItems");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "WarrantyRequestItems");
        }
    }
}
