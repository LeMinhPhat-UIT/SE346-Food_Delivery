using System;
using DeliveryService.Persistences;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DeliveryDbContext))]
    [Migration("20260601192000_AddDeliveryFeePoliciesAndQuotes")]
    public partial class AddDeliveryFeePoliciesAndQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryFeePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MinFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SmallOrderThreshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SmallOrderSurcharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RushHourSurcharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryFeePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryFeeQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PickupLat = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: false),
                    PickupLng = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: false),
                    DropoffLat = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: false),
                    DropoffLng = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: false),
                    DistanceKm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsRushHour = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryFeeQuotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryFeeDistanceTiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromKm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ToKm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    FeePerKm = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryFeeDistanceTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryFeeDistanceTiers_DeliveryFeePolicies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "DeliveryFeePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryFeeQuoteDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DistanceFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SmallOrderSurcharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RushHourSurcharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RawFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsSmallOrder = table.Column<bool>(type: "boolean", nullable: false),
                    IsRushHour = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryFeeQuoteDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryFeeQuoteDetails_DeliveryFeePolicies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "DeliveryFeePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryFeeQuoteDetails_DeliveryFeeQuotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "DeliveryFeeQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "DeliveryFeePolicies"
                    ("Id", "Name", "BaseFee", "MinFee", "MaxFee", "SmallOrderThreshold", "SmallOrderSurcharge", "RushHourSurcharge", "IsActive", "CreatedAt", "UpdatedAt", "DeletedAt")
                VALUES
                    ('81111111-1111-4111-8111-111111111111', 'Default Delivery Fee Policy', 10000, 10000, 60000, 50000, 5000, 5000, TRUE, TIMESTAMPTZ '2026-01-01 00:00:00+00', NULL, NULL);
                """);

            migrationBuilder.Sql("""
                INSERT INTO "DeliveryFeeDistanceTiers"
                    ("Id", "PolicyId", "FromKm", "ToKm", "FeePerKm")
                VALUES
                    ('82222222-2222-4222-8222-222222222222', '81111111-1111-4111-8111-111111111111', 0, 2, 0),
                    ('83333333-3333-4333-8333-333333333333', '81111111-1111-4111-8111-111111111111', 2, 5, 4000),
                    ('84444444-4444-4444-8444-444444444444', '81111111-1111-4111-8111-111111111111', 5, 10, 5000),
                    ('85555555-5555-4555-8555-555555555555', '81111111-1111-4111-8111-111111111111', 10, NULL, 6000);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryFeeDistanceTiers_PolicyId",
                table: "DeliveryFeeDistanceTiers",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryFeePolicies_IsActive",
                table: "DeliveryFeePolicies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryFeeQuoteDetails_PolicyId",
                table: "DeliveryFeeQuoteDetails",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryFeeQuoteDetails_QuoteId",
                table: "DeliveryFeeQuoteDetails",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryFeeQuotes_OrderId",
                table: "DeliveryFeeQuotes",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryFeeDistanceTiers");

            migrationBuilder.DropTable(
                name: "DeliveryFeeQuoteDetails");

            migrationBuilder.DropTable(
                name: "DeliveryFeePolicies");

            migrationBuilder.DropTable(
                name: "DeliveryFeeQuotes");
        }
    }
}
