using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NotificationService.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreDevelopmentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("72222222-2222-4222-8222-222222222222"),
                column: "UserId",
                value: new Guid("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "Body", "CreatedAt", "IsRead", "ReferenceId", "ReferenceType", "Title", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("74444444-4444-4444-8444-444444444444"), "The merchant has confirmed the order and it is being prepared for delivery.", new DateTime(2026, 1, 1, 0, 10, 0, 0, DateTimeKind.Utc), true, new Guid("62222222-2222-4222-8222-222222222222"), "delivery_tracking", "Your order is being prepared", "order_update", new Guid("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("75555555-5555-4555-8555-555555555555"), "You have a new order waiting for pickup.", new DateTime(2026, 1, 1, 0, 15, 0, 0, DateTimeKind.Utc), false, new Guid("64444444-4444-4444-8444-444444444444"), "delivery_assignment", "New delivery assignment", "assignment", new Guid("99999999-9999-4999-9999-999999999999") }
                });

            migrationBuilder.UpdateData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("73333333-3333-4333-8333-333333333333"),
                column: "UserId",
                value: new Guid("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.InsertData(
                table: "UserDevices",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "DeviceToken", "DeviceType", "IsActive", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("76666666-6666-4666-8666-666666666666"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "seed-merchant-device-token", 2, true, null, new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb") },
                    { new Guid("77777777-7777-4777-8777-777777777777"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "seed-shipper-device-token", 0, true, null, new Guid("99999999-9999-4999-9999-999999999999") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("74444444-4444-4444-8444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("75555555-5555-4555-8555-555555555555"));

            migrationBuilder.DeleteData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("76666666-6666-4666-8666-666666666666"));

            migrationBuilder.DeleteData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-4777-8777-777777777777"));

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("72222222-2222-4222-8222-222222222222"),
                column: "UserId",
                value: new Guid("71111111-1111-4111-8111-111111111111"));

            migrationBuilder.UpdateData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("73333333-3333-4333-8333-333333333333"),
                column: "UserId",
                value: new Guid("71111111-1111-4111-8111-111111111111"));
        }
    }
}
