using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityUserField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Merchants",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                columns: new[] { "StoreBannerUrl", "StoreLogoUrl" },
                values: new object[] { "merchants/cccccccc-cccc-4ccc-8ccc-cccccccccccc/banner.png", "merchants/cccccccc-cccc-4ccc-8ccc-cccccccccccc/logo.png" });

            migrationBuilder.UpdateData(
                table: "ShipperRequest",
                keyColumn: "Id",
                keyValue: new Guid("34343434-3434-4434-8434-343434343434"),
                columns: new[] { "IdCardBackUrl", "IdCardFrontUrl", "LicenseBackUrl", "LicenseFrontUrl", "SelfieUrl" },
                values: new object[] { "shippers/99999999-9999-4999-9999-999999999999/id-back.jpg", "shippers/99999999-9999-4999-9999-999999999999/id-front.jpg", "shippers/99999999-9999-4999-9999-999999999999/license-back.jpg", "shippers/99999999-9999-4999-9999-999999999999/license-front.jpg", "shippers/99999999-9999-4999-9999-999999999999/selfie.jpg" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-4555-8555-555555555555"),
                column: "AvatarUrl",
                value: "users/55555555-5555-4555-8555-555555555555/avatars/admin.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-4999-9999-999999999999"),
                column: "AvatarUrl",
                value: "users/99999999-9999-4999-9999-999999999999/avatars/shipper.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa"),
                column: "AvatarUrl",
                value: "users/aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa/avatars/customer.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb"),
                column: "AvatarUrl",
                value: "users/bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb/avatars/merchant.png");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Merchants",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"),
                columns: new[] { "StoreBannerUrl", "StoreLogoUrl" },
                values: new object[] { "https://example.com/stores/banner.png", "https://example.com/stores/logo.png" });

            migrationBuilder.UpdateData(
                table: "ShipperRequest",
                keyColumn: "Id",
                keyValue: new Guid("34343434-3434-4434-8434-343434343434"),
                columns: new[] { "IdCardBackUrl", "IdCardFrontUrl", "LicenseBackUrl", "LicenseFrontUrl", "SelfieUrl" },
                values: new object[] { "https://example.com/shipper/id-back.jpg", "https://example.com/shipper/id-front.jpg", "https://example.com/shipper/license-back.jpg", "https://example.com/shipper/license-front.jpg", "https://example.com/shipper/selfie.jpg" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-4555-8555-555555555555"),
                column: "AvatarUrl",
                value: "https://example.com/avatars/admin.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-4999-9999-999999999999"),
                column: "AvatarUrl",
                value: "https://example.com/avatars/shipper.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa"),
                column: "AvatarUrl",
                value: "https://example.com/avatars/customer.png");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb"),
                column: "AvatarUrl",
                value: "https://example.com/avatars/merchant.png");
        }
    }
}
