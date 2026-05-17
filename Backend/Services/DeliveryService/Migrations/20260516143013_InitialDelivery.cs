using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    public partial class InitialDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryTrackings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipperId = table.Column<Guid>(type: "uuid", nullable: true),
                    PickupLat = table.Column<decimal>(type: "numeric", nullable: false),
                    PickupLng = table.Column<decimal>(type: "numeric", nullable: false),
                    DeliveryLat = table.Column<decimal>(type: "numeric", nullable: false),
                    DeliveryLng = table.Column<decimal>(type: "numeric", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "numeric", nullable: false),
                    EstimatedTime = table.Column<int>(type: "integer", nullable: false),
                    ActualTime = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryTrackings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ProofUrl = table.Column<string[]>(type: "text[]", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Resolution = table.Column<string>(type: "text", nullable: false),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShipperAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "text", nullable: false),
                    ShipperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PickedUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectReason = table.Column<string>(type: "text", nullable: true),
                    PickupProofFileKey = table.Column<string>(type: "text", nullable: true),
                    DeliveryProofFileKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipperAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShipperAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentLat = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentLng = table.Column<decimal>(type: "numeric", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipperAvailabilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShipperLocationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipperLocationHistories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DeliveryTrackings",
                columns: new[] { "Id", "ActualTime", "CreatedAt", "DeletedAt", "DeliveryLat", "DeliveryLng", "DistanceKm", "EstimatedTime", "OrderId", "PickupLat", "PickupLng", "ShipperId", "Status", "UpdatedAt" },
                values: new object[] { new Guid("61111111-1111-1111-1111-111111111111"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 10.7700m, 106.6950m, 2.2m, 15, new Guid("62222222-2222-2222-2222-222222222222"), 10.7769m, 106.7009m, new Guid("63333333-3333-3333-3333-333333333333"), 1, null });

            migrationBuilder.InsertData(
                table: "ShipperAssignments",
                columns: new[] { "Id", "AcceptedAt", "AssignedAt", "CustomerId", "DeliveredAt", "DeliveryProofFileKey", "OrderId", "OrderNumber", "PickedUpAt", "PickupProofFileKey", "RejectReason", "RespondedAt", "ShipperId", "Status" },
                values: new object[] { new Guid("64444444-4444-4444-4444-444444444444"), new DateTime(2026, 1, 1, 0, 1, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("67777777-7777-7777-7777-777777777777"), null, null, new Guid("62222222-2222-2222-2222-222222222222"), "ORD-SEED-0001", null, null, null, null, new Guid("63333333-3333-3333-3333-333333333333"), 1 });

            migrationBuilder.InsertData(
                table: "ShipperAvailabilities",
                columns: new[] { "Id", "CreatedAt", "CurrentLat", "CurrentLng", "CurrentOrderId", "DeletedAt", "LastSeenAt", "ShipperId", "Status", "UpdatedAt" },
                values: new object[] { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10.7735m, 106.6975m, new Guid("62222222-2222-2222-2222-222222222222"), null, new DateTime(2026, 1, 1, 0, 5, 0, 0, DateTimeKind.Utc), new Guid("63333333-3333-3333-3333-333333333333"), 3, null });

            migrationBuilder.InsertData(
                table: "ShipperLocationHistories",
                columns: new[] { "Id", "CorrelationId", "CreatedAt", "DeletedAt", "Latitude", "Longitude", "OrderId", "RecordedAt", "ShipperId", "UpdatedAt" },
                values: new object[] { new Guid("65555555-5555-5555-5555-555555555555"), "seed-delivery", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 10.7735m, 106.6975m, new Guid("62222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 5, 0, 0, DateTimeKind.Utc), new Guid("63333333-3333-3333-3333-333333333333"), null });

            migrationBuilder.CreateIndex(
                name: "IX_ShipperLocationHistories_OrderId_RecordedAt",
                table: "ShipperLocationHistories",
                columns: new[] { "OrderId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ShipperLocationHistories_ShipperId_RecordedAt",
                table: "ShipperLocationHistories",
                columns: new[] { "ShipperId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryTrackings");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "ShipperAssignments");

            migrationBuilder.DropTable(
                name: "ShipperAvailabilities");

            migrationBuilder.DropTable(
                name: "ShipperLocationHistories");
        }
    }
}
