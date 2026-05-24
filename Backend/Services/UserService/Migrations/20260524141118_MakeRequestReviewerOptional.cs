using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class MakeRequestReviewerOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantRequests_Users_ReviewedBy",
                table: "MerchantRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipperRequest_Users_ReviewedBy",
                table: "ShipperRequest");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewedBy",
                table: "ShipperRequest",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewedBy",
                table: "MerchantRequests",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantRequests_Users_ReviewedBy",
                table: "MerchantRequests",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipperRequest_Users_ReviewedBy",
                table: "ShipperRequest",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MerchantRequests_Users_ReviewedBy",
                table: "MerchantRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipperRequest_Users_ReviewedBy",
                table: "ShipperRequest");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewedBy",
                table: "ShipperRequest",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReviewedBy",
                table: "MerchantRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MerchantRequests_Users_ReviewedBy",
                table: "MerchantRequests",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipperRequest_Users_ReviewedBy",
                table: "ShipperRequest",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
