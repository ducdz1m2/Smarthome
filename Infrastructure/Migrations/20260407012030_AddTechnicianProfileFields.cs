using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TechnicianProfiles_UserId",
                table: "TechnicianProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "TechnicianProfiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "TechnicianProfiles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "TechnicianProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "TechnicianProfiles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "TechnicianProfiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IdentityCard",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "TechnicianProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianProfiles_EmployeeCode",
                table: "TechnicianProfiles",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianProfiles_UserId",
                table: "TechnicianProfiles",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InstallationBookings_Orders_OrderId",
                table: "InstallationBookings",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstallationBookings_Orders_OrderId",
                table: "InstallationBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_TechnicianProfiles_EmployeeCode",
                table: "TechnicianProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TechnicianProfiles_UserId",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "BaseSalary",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "IdentityCard",
                table: "TechnicianProfiles");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "TechnicianProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "TechnicianProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianProfiles_UserId",
                table: "TechnicianProfiles",
                column: "UserId",
                unique: true);
        }
    }
}
