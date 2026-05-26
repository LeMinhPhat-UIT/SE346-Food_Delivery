using System;
using DeliveryService.Persistences;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DeliveryDbContext))]
    [Migration("20260525120000_AddDeliveryCompletionPayloadToAssignments")]
    public partial class AddDeliveryCompletionPayloadToAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "ShipperAssignments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DistanceKm",
                table: "ShipperAssignments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "ShipperAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.Sql("""
                UPDATE "ShipperAssignments"
                SET "DeliveryFee" = 21000,
                    "DistanceKm" = 2.2,
                    "MerchantId" = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'
                WHERE "Id" = '64444444-4444-4444-4444-444444444444';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "DistanceKm",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "ShipperAssignments");
        }
    }
}
