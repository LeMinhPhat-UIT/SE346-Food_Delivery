using System;
using DeliveryService.Persistences;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DeliveryDbContext))]
    [Migration("20260603150000_AddCustomerContactToAssignments")]
    public partial class AddCustomerContactToAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "ShipperAssignments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "ShipperAssignments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "ShipperAssignments"
                SET "CustomerName" = 'Seeded Customer',
                    "CustomerPhone" = '0900000001'
                WHERE "Id" = '64444444-4444-4444-8444-444444444444'::uuid;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "ShipperAssignments");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "ShipperAssignments");
        }
    }
}
