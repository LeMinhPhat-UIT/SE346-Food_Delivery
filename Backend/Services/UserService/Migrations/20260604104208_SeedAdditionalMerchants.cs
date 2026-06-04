using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdditionalMerchants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MerchantAddresses",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-4fff-8fff-ffffffffffff"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.InsertData(
                table: "MerchantRequests",
                columns: new[] { "Id", "BusinessLicense", "BusinessLicenseUrl", "CreatedAt", "RejectedReason", "ReviewedBy", "StoreDescription", "StoreName", "TaxId", "UserId", "VerificationStatus", "VerifiedAt" },
                values: new object[] { new Guid("a1a1a1a1-a1a1-4a1a-8a1a-a1a1a1a1a1a1"), "BL-SEED-0001", "merchant-requests/bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb/business-license.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", new Guid("55555555-5555-4555-8555-555555555555"), "Default merchant store for local development.", "Seeded Merchant Store", "TAX-SEED-0001", new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb"), "Approved", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Merchants",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "DeletedAt", "FullName", "MerchantId", "PhoneNumber", "ShipperId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2"), "users/b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2/avatars/merchant.png", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Lina Tran", null, "0900000004", null, "Active", null },
                    { new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3"), "users/b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3/avatars/merchant.png", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Quang Pham", null, "0900000005", null, "Active", null }
                });

            migrationBuilder.InsertData(
                table: "Addresses",
                columns: new[] { "Id", "AddressLine", "City", "CreatedAt", "DeletedAt", "District", "IsDefault", "Label", "Lat", "Lng", "Phone", "RecipientName", "UpdatedAt", "UserId", "Ward" },
                values: new object[,]
                {
                    { new Guid("d2d2d2d2-d2d2-4d2d-8d2d-d2d2d2d2d2d2"), "15 Pasteur", "Ho Chi Minh City", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "District 1", true, "Store Contact", 10.7781m, 106.6993m, "0900000004", "Lina Tran", null, new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2"), "Ben Nghe" },
                    { new Guid("d3d3d3d3-d3d3-4d3d-8d3d-d3d3d3d3d3d3"), "42 Cach Mang Thang 8", "Ho Chi Minh City", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "District 3", true, "Store Contact", 10.7815m, 106.6843m, "0900000005", "Quang Pham", null, new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3"), "Vo Thi Sau" }
                });

            migrationBuilder.InsertData(
                table: "MerchantRequests",
                columns: new[] { "Id", "BusinessLicense", "BusinessLicenseUrl", "CreatedAt", "RejectedReason", "ReviewedBy", "StoreDescription", "StoreName", "TaxId", "UserId", "VerificationStatus", "VerifiedAt" },
                values: new object[,]
                {
                    { new Guid("a2a2a2a2-a2a2-4a2a-8a2a-a2a2a2a2a2a2"), "BL-SEED-0002", "merchant-requests/b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2/business-license.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", new Guid("55555555-5555-4555-8555-555555555555"), "Healthy rice bowls, salads, and fresh juices for busy lunches.", "Saigon Fresh Bowls", "TAX-SEED-0002", new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2"), "Approved", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a3a3a3a3-a3a3-4a3a-8a3a-a3a3a3a3a3a3"), "BL-SEED-0003", "merchant-requests/b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3/business-license.jpg", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", new Guid("55555555-5555-4555-8555-555555555555"), "Vietnamese sandwiches, coffee, and quick breakfast sets.", "Banh Mi Corner", "TAX-SEED-0003", new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3"), "Approved", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Merchants",
                columns: new[] { "Id", "AvgPrepTime", "BusinessLicense", "ClosingTime", "CreatedAt", "DeletedAt", "IsOpen", "MinOrderAmount", "OpeningTime", "Status", "StoreBannerUrl", "StoreDescription", "StoreLogoUrl", "StoreName", "TaxId", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("c2c2c2c2-c2c2-4c2c-8c2c-c2c2c2c2c2c2"), 18, "BL-SEED-0002", new TimeSpan(0, 21, 30, 0, 0), new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 45000m, new TimeSpan(0, 9, 0, 0, 0), "Approved", "merchants/c2c2c2c2-c2c2-4c2c-8c2c-c2c2c2c2c2c2/banner.png", "Healthy rice bowls, salads, and fresh juices for busy lunches.", "merchants/c2c2c2c2-c2c2-4c2c-8c2c-c2c2c2c2c2c2/logo.png", "Saigon Fresh Bowls", "TAX-SEED-0002", null, new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2") },
                    { new Guid("c3c3c3c3-c3c3-4c3c-8c3c-c3c3c3c3c3c3"), 12, "BL-SEED-0003", new TimeSpan(0, 20, 0, 0, 0), new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 25000m, new TimeSpan(0, 6, 30, 0, 0), "Approved", "merchants/c3c3c3c3-c3c3-4c3c-8c3c-c3c3c3c3c3c3/banner.png", "Vietnamese sandwiches, coffee, and quick breakfast sets.", "merchants/c3c3c3c3-c3c3-4c3c-8c3c-c3c3c3c3c3c3/logo.png", "Banh Mi Corner", "TAX-SEED-0003", null, new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3") }
                });

            migrationBuilder.InsertData(
                table: "MerchantAddresses",
                columns: new[] { "Id", "AddressLine", "City", "CreatedAt", "DeletedAt", "District", "Lat", "Lng", "MerchantId", "UpdatedAt", "Ward" },
                values: new object[,]
                {
                    { new Guid("e2e2e2e2-e2e2-4e2e-8e2e-e2e2e2e2e2e2"), "15 Pasteur", "Ho Chi Minh City", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "District 1", 10.7781m, 106.6993m, new Guid("c2c2c2c2-c2c2-4c2c-8c2c-c2c2c2c2c2c2"), null, "Ben Nghe" },
                    { new Guid("e3e3e3e3-e3e3-4e3e-8e3e-e3e3e3e3e3e3"), "42 Cach Mang Thang 8", "Ho Chi Minh City", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "District 3", 10.7815m, 106.6843m, new Guid("c3c3c3c3-c3c3-4c3c-8c3c-c3c3c3c3c3c3"), null, "Vo Thi Sau" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Addresses",
                keyColumn: "Id",
                keyValue: new Guid("d2d2d2d2-d2d2-4d2d-8d2d-d2d2d2d2d2d2"));

            migrationBuilder.DeleteData(
                table: "Addresses",
                keyColumn: "Id",
                keyValue: new Guid("d3d3d3d3-d3d3-4d3d-8d3d-d3d3d3d3d3d3"));

            migrationBuilder.DeleteData(
                table: "MerchantAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e2e2e2e2-e2e2-4e2e-8e2e-e2e2e2e2e2e2"));

            migrationBuilder.DeleteData(
                table: "MerchantAddresses",
                keyColumn: "Id",
                keyValue: new Guid("e3e3e3e3-e3e3-4e3e-8e3e-e3e3e3e3e3e3"));

            migrationBuilder.DeleteData(
                table: "MerchantRequests",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-a1a1-4a1a-8a1a-a1a1a1a1a1a1"));

            migrationBuilder.DeleteData(
                table: "MerchantRequests",
                keyColumn: "Id",
                keyValue: new Guid("a2a2a2a2-a2a2-4a2a-8a2a-a2a2a2a2a2a2"));

            migrationBuilder.DeleteData(
                table: "MerchantRequests",
                keyColumn: "Id",
                keyValue: new Guid("a3a3a3a3-a3a3-4a3a-8a3a-a3a3a3a3a3a3"));

            migrationBuilder.DeleteData(
                table: "Merchants",
                keyColumn: "Id",
                keyValue: new Guid("c2c2c2c2-c2c2-4c2c-8c2c-c2c2c2c2c2c2"));

            migrationBuilder.DeleteData(
                table: "Merchants",
                keyColumn: "Id",
                keyValue: new Guid("c3c3c3c3-c3c3-4c3c-8c3c-c3c3c3c3c3c3"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3"));

            migrationBuilder.UpdateData(
                table: "MerchantAddresses",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-4fff-8fff-ffffffffffff"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Merchants",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }
    }
}
