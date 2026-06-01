using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentDispatchLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentAssignmentId",
                table: "ShipperAvailabilities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentOfferedAssignmentId",
                table: "ShipperAvailabilities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferingExpiresAt",
                table: "ShipperAvailabilities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledReason",
                table: "ShipperAssignments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DropoffAddress",
                table: "ShipperAssignments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DropoffLatitude",
                table: "ShipperAssignments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DropoffLongitude",
                table: "ShipperAssignments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MerchantName",
                table: "ShipperAssignments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferExpiresAt",
                table: "ShipperAssignments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress",
                table: "ShipperAssignments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PickupLatitude",
                table: "ShipperAssignments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PickupLongitude",
                table: "ShipperAssignments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "ShipperAssignments",
                keyColumn: "Id",
                keyValue: new Guid("64444444-4444-4444-8444-444444444444"),
                columns: new[] { "CancelledReason", "DropoffAddress", "DropoffLatitude", "DropoffLongitude", "MerchantName", "OfferExpiresAt", "PickupAddress", "PickupLatitude", "PickupLongitude" },
                values: new object[] { null, "Seed dropoff address", 10.7700m, 106.6950m, "Seed Merchant", null, "Seed pickup address", 10.7769m, 106.7009m });

            migrationBuilder.UpdateData(
                table: "ShipperAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-4666-8666-666666666666"),
                columns: new[] { "CurrentAssignmentId", "CurrentOfferedAssignmentId", "OfferingExpiresAt" },
                values: new object[] { null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_ShipperAvailabilities_CurrentAssignmentId",
                table: "ShipperAvailabilities",
                column: "CurrentAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipperAvailabilities_CurrentOfferedAssignmentId",
                table: "ShipperAvailabilities",
                column: "CurrentOfferedAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipperAvailabilities_Status",
                table: "ShipperAvailabilities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ShipperAssignments_OrderId",
                table: "ShipperAssignments",
                column: "OrderId",
                unique: true,
                filter: "\"Status\" = 'Accepted'");

            migrationBuilder.CreateIndex(
                name: "IX_ShipperAssignments_OrderId_ShipperId",
                table: "ShipperAssignments",
                columns: new[] { "OrderId", "ShipperId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipperAssignments_Status",
                table: "ShipperAssignments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShipperAvailabilities_CurrentAssignmentId",
                table: "ShipperAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_ShipperAvailabilities_CurrentOfferedAssignmentId",
                table: "ShipperAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_ShipperAvailabilities_Status",
                table: "ShipperAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_ShipperAssignments_OrderId",
                table: "ShipperAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ShipperAssignments_OrderId_ShipperId",
                table: "ShipperAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ShipperAssignments_Status",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "CurrentAssignmentId",
                table: "ShipperAvailabilities");

            migrationBuilder.DropColumn(
                name: "CurrentOfferedAssignmentId",
                table: "ShipperAvailabilities");

            migrationBuilder.DropColumn(
                name: "OfferingExpiresAt",
                table: "ShipperAvailabilities");

            migrationBuilder.DropColumn(
                name: "CancelledReason",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "DropoffAddress",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "DropoffLatitude",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "DropoffLongitude",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "MerchantName",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "OfferExpiresAt",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "PickupAddress",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "PickupLatitude",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "PickupLongitude",
                table: "ShipperAssignments");
        }
    }
}
