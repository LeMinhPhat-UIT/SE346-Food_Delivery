using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreDevelopmentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "DeletedAt", "FullName", "MerchantId", "ShipperId", "Status", "UpdatedAt" },
                values: new object[] { new Guid("99999999-9999-4999-9999-999999999999"), "https://example.com/avatars/shipper.png", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Seeded Shipper", null, null, 0, null });

            migrationBuilder.InsertData(
                table: "Addresses",
                columns: new[] { "Id", "AddressLine", "City", "CreatedAt", "DeletedAt", "District", "IsDefault", "Label", "Lat", "Lng", "Phone", "RecipientName", "UpdatedAt", "UserId", "Ward" },
                values: new object[] { new Guid("12121212-1212-4212-8212-121212121212"), "3 Nguyen Trai", "Ho Chi Minh City", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "District 1", true, "Home", 10.7691m, 106.6824m, "0900000003", "Seeded Shipper", null, new Guid("99999999-9999-4999-9999-999999999999"), "Pham Ngu Lao" });

            migrationBuilder.InsertData(
                table: "ShipperRequest",
                columns: new[] { "Id", "CreatedAt", "DateOfBirth", "FullName", "IdCardBackUrl", "IdCardFrontUrl", "IdNumber", "LicenseBackUrl", "LicenseFrontUrl", "LicenseNumber", "RejectedReason", "ReviewedBy", "SelfieUrl", "UserId", "VerificationStatus", "VerifiedAt" },
                values: new object[] { new Guid("34343434-3434-4434-8434-343434343434"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1998, 4, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Seeded Shipper", "https://example.com/shipper/id-back.jpg", "https://example.com/shipper/id-front.jpg", "079202600001", "https://example.com/shipper/license-back.jpg", "https://example.com/shipper/license-front.jpg", "DL-SEED-0001", "", new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb"), "https://example.com/shipper/selfie.jpg", new Guid("99999999-9999-4999-9999-999999999999"), 1, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Shippers",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "RequestId", "Status", "UpdatedAt", "UserId", "VehiclePlate" },
                values: new object[] { new Guid("56565656-5656-4656-8656-565656565656"), new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("34343434-3434-4434-8434-343434343434"), 1, null, new Guid("99999999-9999-4999-9999-999999999999"), "59A-123.45" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Addresses",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-4212-8212-121212121212"));

            migrationBuilder.DeleteData(
                table: "Shippers",
                keyColumn: "Id",
                keyValue: new Guid("56565656-5656-4656-8656-565656565656"));

            migrationBuilder.DeleteData(
                table: "ShipperRequest",
                keyColumn: "Id",
                keyValue: new Guid("34343434-3434-4434-8434-343434343434"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-4999-9999-999999999999"));
        }
    }
}
