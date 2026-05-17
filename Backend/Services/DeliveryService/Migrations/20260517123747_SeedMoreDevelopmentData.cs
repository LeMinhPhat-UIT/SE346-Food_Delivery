using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreDevelopmentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DeliveryTrackings",
                keyColumn: "Id",
                keyValue: new Guid("61111111-1111-1111-1111-111111111111"),
                column: "ShipperId",
                value: new Guid("56565656-5656-5656-5656-565656565656"));

            migrationBuilder.InsertData(
                table: "DeliveryTrackings",
                columns: new[] { "Id", "ActualTime", "CreatedAt", "DeletedAt", "DeliveryLat", "DeliveryLng", "DistanceKm", "EstimatedTime", "OrderId", "PickupLat", "PickupLng", "ShipperId", "Status", "UpdatedAt" },
                values: new object[] { new Guid("74444444-4444-4444-4444-444444444444"), 14, new DateTime(2025, 12, 31, 23, 0, 0, 0, DateTimeKind.Utc), null, 10.7770m, 106.7002m, 1.8m, 12, new Guid("75555555-5555-5555-5555-555555555555"), 10.7821m, 106.6925m, new Guid("56565656-5656-5656-5656-565656565656"), 5, null });

            migrationBuilder.InsertData(
                table: "Incidents",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Description", "OrderId", "ProofUrl", "ReportedBy", "Resolution", "ResolvedAt", "ResolvedBy", "Status", "Type", "UpdatedAt" },
                values: new object[] { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2025, 12, 31, 23, 0, 0, 0, DateTimeKind.Utc), null, "Customer reported one missing item from the delivered order.", new Guid("75555555-5555-5555-5555-555555555555"), new[] { "https://example.com/incidents/order-2-photo-1.jpg", "https://example.com/incidents/order-2-photo-2.jpg" }, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Awaiting support review and customer confirmation.", null, null, 1, 1, null });

            migrationBuilder.UpdateData(
                table: "ShipperAssignments",
                keyColumn: "Id",
                keyValue: new Guid("64444444-4444-4444-4444-444444444444"),
                columns: new[] { "CustomerId", "ShipperId" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("56565656-5656-5656-5656-565656565656") });

            migrationBuilder.UpdateData(
                table: "ShipperAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "ShipperId",
                value: new Guid("56565656-5656-5656-5656-565656565656"));

            migrationBuilder.UpdateData(
                table: "ShipperLocationHistories",
                keyColumn: "Id",
                keyValue: new Guid("65555555-5555-5555-5555-555555555555"),
                column: "ShipperId",
                value: new Guid("56565656-5656-5656-5656-565656565656"));

            migrationBuilder.InsertData(
                table: "ShipperLocationHistories",
                columns: new[] { "Id", "CorrelationId", "CreatedAt", "DeletedAt", "Latitude", "Longitude", "OrderId", "RecordedAt", "ShipperId", "UpdatedAt" },
                values: new object[] { new Guid("76666666-6666-6666-6666-666666666666"), "seed-delivery-completed", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 10.7785m, 106.6990m, new Guid("75555555-5555-5555-5555-555555555555"), new DateTime(2026, 1, 1, 0, 25, 0, 0, DateTimeKind.Utc), new Guid("56565656-5656-5656-5656-565656565656"), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeliveryTrackings",
                keyColumn: "Id",
                keyValue: new Guid("74444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Incidents",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "ShipperLocationHistories",
                keyColumn: "Id",
                keyValue: new Guid("76666666-6666-6666-6666-666666666666"));

            migrationBuilder.UpdateData(
                table: "DeliveryTrackings",
                keyColumn: "Id",
                keyValue: new Guid("61111111-1111-1111-1111-111111111111"),
                column: "ShipperId",
                value: new Guid("63333333-3333-3333-3333-333333333333"));

            migrationBuilder.UpdateData(
                table: "ShipperAssignments",
                keyColumn: "Id",
                keyValue: new Guid("64444444-4444-4444-4444-444444444444"),
                columns: new[] { "CustomerId", "ShipperId" },
                values: new object[] { new Guid("67777777-7777-7777-7777-777777777777"), new Guid("63333333-3333-3333-3333-333333333333") });

            migrationBuilder.UpdateData(
                table: "ShipperAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "ShipperId",
                value: new Guid("63333333-3333-3333-3333-333333333333"));

            migrationBuilder.UpdateData(
                table: "ShipperLocationHistories",
                keyColumn: "Id",
                keyValue: new Guid("65555555-5555-5555-5555-555555555555"),
                column: "ShipperId",
                value: new Guid("63333333-3333-3333-3333-333333333333"));
        }
    }
}
