using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthenticationService.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdditionalMerchants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb") });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FullName", "IsOtpVerified", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "Otp", "OtpExpiresAt", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Status", "TwoFactorEnabled", "UpdatedAt", "UserName" },
                values: new object[,]
                {
                    { new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2"), 0, "SEED-MERCHANT-TWO-CONCURRENCY-STAMP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "lina.tran@fooddelivery.local", true, "Lina Tran", true, false, null, "LINA.TRAN@FOODDELIVERY.LOCAL", "LINA.TRAN@FOODDELIVERY.LOCAL", null, null, "AQAAAAIAAYagAAAAEHwR9ekhFFazFAUin52gHpisNjCdZFdVS0N+lAgSluDA+/uEYfuONXMTsB3L8jrAdA==", null, false, "SEED-MERCHANT-TWO-SECURITY-STAMP", "Active", false, null, "lina.tran@fooddelivery.local" },
                    { new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3"), 0, "SEED-MERCHANT-THREE-CONCURRENCY-STAMP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "quang.pham@fooddelivery.local", true, "Quang Pham", true, false, null, "QUANG.PHAM@FOODDELIVERY.LOCAL", "QUANG.PHAM@FOODDELIVERY.LOCAL", null, null, "AQAAAAIAAYagAAAAEHwR9ekhFFazFAUin52gHpisNjCdZFdVS0N+lAgSluDA+/uEYfuONXMTsB3L8jrAdA==", null, false, "SEED-MERCHANT-THREE-SECURITY-STAMP", "Active", false, null, "quang.pham@fooddelivery.local" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2") },
                    { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2") },
                    { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3") },
                    { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("22222222-2222-4222-8222-222222222222"), new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3") });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("11111111-1111-4111-8111-111111111111"), new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb") });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b2b2b2b2-b2b2-4b2b-8b2b-b2b2b2b2b2b2"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b3b3b3b3-b3b3-4b3b-8b3b-b3b3b3b3b3b3"));
        }
    }
}
