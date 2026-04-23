using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDamagedStatusToReturnOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DamagedStatus",
                table: "ReturnOrderItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RepairCost",
                table: "ReturnOrderItem",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepairNotes",
                table: "ReturnOrderItem",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamagedStatus",
                table: "ReturnOrderItem");

            migrationBuilder.DropColumn(
                name: "RepairCost",
                table: "ReturnOrderItem");

            migrationBuilder.DropColumn(
                name: "RepairNotes",
                table: "ReturnOrderItem");
        }
    }
}
