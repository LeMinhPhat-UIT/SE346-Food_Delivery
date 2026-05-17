using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthenticationService.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreDevelopmentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "2e2a574e-c366-4ccf-8070-84c92d3e1ac8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "9b3d1f32-bf61-4a0f-9b19-2a5eea11b963");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "21c23bfd-d356-4fae-a1a4-73fb97966d89");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "ConcurrencyStamp",
                value: "5d9a7718-e52a-458d-ba86-d784eb280437");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FullName", "IsOtpVerified", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "Otp", "OtpExpiresAt", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Status", "TwoFactorEnabled", "UpdatedAt", "UserName" },
                values: new object[,]
                {
                    { new Guid("99999999-9999-9999-9999-999999999999"), 0, "SEED-SHIPPER-CONCURRENCY-STAMP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "shipper@fooddelivery.local", true, "Seeded Shipper", true, false, null, "SHIPPER@FOODDELIVERY.LOCAL", "SHIPPER@FOODDELIVERY.LOCAL", null, null, "AQAAAAIAAYagAAAAEHwR9ekhFFazFAUin52gHpisNjCdZFdVS0N+lAgSluDA+/uEYfuONXMTsB3L8jrAdA==", null, false, "SEED-SHIPPER-SECURITY-STAMP", 0, false, null, "shipper@fooddelivery.local" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 0, "SEED-CUSTOMER-CONCURRENCY-STAMP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "customer@fooddelivery.local", true, "Seeded Customer", true, false, null, "CUSTOMER@FOODDELIVERY.LOCAL", "CUSTOMER@FOODDELIVERY.LOCAL", null, null, "AQAAAAIAAYagAAAAEHwR9ekhFFazFAUin52gHpisNjCdZFdVS0N+lAgSluDA+/uEYfuONXMTsB3L8jrAdA==", null, false, "SEED-CUSTOMER-SECURITY-STAMP", 0, false, null, "customer@fooddelivery.local" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, "SEED-MERCHANT-CONCURRENCY-STAMP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "merchant@fooddelivery.local", true, "Seeded Merchant Owner", true, false, null, "MERCHANT@FOODDELIVERY.LOCAL", "MERCHANT@FOODDELIVERY.LOCAL", null, null, "AQAAAAIAAYagAAAAEHwR9ekhFFazFAUin52gHpisNjCdZFdVS0N+lAgSluDA+/uEYfuONXMTsB3L8jrAdA==", null, false, "SEED-MERCHANT-SECURITY-STAMP", 0, false, null, "merchant@fooddelivery.local" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("99999999-9999-9999-9999-999999999999") },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("99999999-9999-9999-9999-999999999999") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "5e00d580-94e1-461d-b1b4-53799c830806");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "ConcurrencyStamp",
                value: "e291a0ba-4bc0-483f-a462-548edf727761");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "ConcurrencyStamp",
                value: "ad0cf025-e031-47d2-80ef-6d1d6e1c5f2e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "ConcurrencyStamp",
                value: "cb8ae184-e6d3-4ddd-8b29-37b836e2cd7f");
        }
    }
}
