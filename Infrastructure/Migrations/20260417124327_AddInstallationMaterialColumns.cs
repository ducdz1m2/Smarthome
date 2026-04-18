using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallationMaterialColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt",
                table: "InstallationMaterials",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "InstallationMaterials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "InstallationMaterials",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstallationMaterials_WarehouseId",
                table: "InstallationMaterials",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_InstallationMaterials_Warehouses_WarehouseId",
                table: "InstallationMaterials",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstallationMaterials_Warehouses_WarehouseId",
                table: "InstallationMaterials");

            migrationBuilder.DropIndex(
                name: "IX_InstallationMaterials_WarehouseId",
                table: "InstallationMaterials");

            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                table: "InstallationMaterials");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "InstallationMaterials");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "InstallationMaterials");
        }
    }
}
