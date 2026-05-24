using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Migrations
{
    /// <inheritdoc />
    public partial class StoreEnumsAsStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DeviceType",
                table: "UserDevices",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql(
                """
                UPDATE "UserDevices"
                SET "DeviceType" = CASE "DeviceType"
                    WHEN '0' THEN 'Ios'
                    WHEN '1' THEN 'Android'
                    WHEN '2' THEN 'Web'
                    ELSE "DeviceType"
                END;
                """);

            migrationBuilder.UpdateData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("73333333-3333-3333-3333-333333333333"),
                column: "DeviceType",
                value: "Android");

            migrationBuilder.UpdateData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("76666666-6666-6666-6666-666666666666"),
                column: "DeviceType",
                value: "Web");

            migrationBuilder.UpdateData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "DeviceType",
                value: "Ios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "UserDevices"
                SET "DeviceType" = CASE "DeviceType"
                    WHEN 'Ios' THEN '0'
                    WHEN 'Android' THEN '1'
                    WHEN 'Web' THEN '2'
                    ELSE "DeviceType"
                END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "DeviceType",
                table: "UserDevices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("73333333-3333-3333-3333-333333333333"),
                column: "DeviceType",
                value: 1);

            migrationBuilder.UpdateData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("76666666-6666-6666-6666-666666666666"),
                column: "DeviceType",
                value: 2);

            migrationBuilder.UpdateData(
                table: "UserDevices",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "DeviceType",
                value: 0);
        }
    }
}
