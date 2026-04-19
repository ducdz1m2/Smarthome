using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDamagedProductHandlingToWarrantyRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarrantyRequestItem_WarrantyRequests_WarrantyRequestId",
                table: "WarrantyRequestItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WarrantyRequestItem",
                table: "WarrantyRequestItem");

            migrationBuilder.RenameTable(
                name: "WarrantyRequestItem",
                newName: "WarrantyRequestItems");

            migrationBuilder.RenameIndex(
                name: "IX_WarrantyRequestItem_WarrantyRequestId",
                table: "WarrantyRequestItems",
                newName: "IX_WarrantyRequestItems_WarrantyRequestId");

            migrationBuilder.AlterColumn<string>(
                name: "TechnicianNotes",
                table: "WarrantyRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WarrantyRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WarrantyRequestItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDamaged",
                table: "WarrantyRequestItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReturnedToInventory",
                table: "WarrantyRequestItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WarrantyRequestItems",
                table: "WarrantyRequestItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequests_OrderId",
                table: "WarrantyRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequests_Status",
                table: "WarrantyRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequestItems_OrderItemId",
                table: "WarrantyRequestItems",
                column: "OrderItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantyRequestItems_WarrantyRequests_WarrantyRequestId",
                table: "WarrantyRequestItems",
                column: "WarrantyRequestId",
                principalTable: "WarrantyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarrantyRequestItems_WarrantyRequests_WarrantyRequestId",
                table: "WarrantyRequestItems");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyRequests_OrderId",
                table: "WarrantyRequests");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyRequests_Status",
                table: "WarrantyRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WarrantyRequestItems",
                table: "WarrantyRequestItems");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyRequestItems_OrderItemId",
                table: "WarrantyRequestItems");

            migrationBuilder.DropColumn(
                name: "IsDamaged",
                table: "WarrantyRequestItems");

            migrationBuilder.DropColumn(
                name: "ReturnedToInventory",
                table: "WarrantyRequestItems");

            migrationBuilder.RenameTable(
                name: "WarrantyRequestItems",
                newName: "WarrantyRequestItem");

            migrationBuilder.RenameIndex(
                name: "IX_WarrantyRequestItems_WarrantyRequestId",
                table: "WarrantyRequestItem",
                newName: "IX_WarrantyRequestItem_WarrantyRequestId");

            migrationBuilder.AlterColumn<string>(
                name: "TechnicianNotes",
                table: "WarrantyRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WarrantyRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WarrantyRequestItem",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WarrantyRequestItem",
                table: "WarrantyRequestItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantyRequestItem_WarrantyRequests_WarrantyRequestId",
                table: "WarrantyRequestItem",
                column: "WarrantyRequestId",
                principalTable: "WarrantyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
