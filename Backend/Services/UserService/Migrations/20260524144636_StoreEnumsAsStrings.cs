using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class StoreEnumsAsStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Shippers",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "VerificationStatus",
                table: "ShipperRequest",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Merchants",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "VerificationStatus",
                table: "MerchantRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql(
                """
                UPDATE "Users"
                SET "Status" = CASE "Status"
                    WHEN '0' THEN 'Active'
                    WHEN '1' THEN 'Inactive'
                    WHEN '2' THEN 'Banned'
                    WHEN '3' THEN 'PendingVerification'
                    ELSE "Status"
                END;

                UPDATE "Shippers"
                SET "Status" = CASE "Status"
                    WHEN '0' THEN 'Pending'
                    WHEN '1' THEN 'Approved'
                    WHEN '2' THEN 'Rejected'
                    WHEN '3' THEN 'Suspended'
                    ELSE "Status"
                END;

                UPDATE "Merchants"
                SET "Status" = CASE "Status"
                    WHEN '0' THEN 'Pending'
                    WHEN '1' THEN 'Approved'
                    WHEN '2' THEN 'Rejected'
                    WHEN '3' THEN 'Suspended'
                    ELSE "Status"
                END;

                UPDATE "ShipperRequest"
                SET "VerificationStatus" = CASE "VerificationStatus"
                    WHEN '0' THEN 'Pending'
                    WHEN '1' THEN 'Approved'
                    WHEN '2' THEN 'Rejected'
                    ELSE "VerificationStatus"
                END;

                UPDATE "MerchantRequests"
                SET "VerificationStatus" = CASE "VerificationStatus"
                    WHEN '0' THEN 'Pending'
                    WHEN '1' THEN 'Approved'
                    WHEN '2' THEN 'Rejected'
                    ELSE "VerificationStatus"
                END;
                """);

            migrationBuilder.UpdateData(
                table: "Merchants",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "Status",
                value: "Approved");

            migrationBuilder.UpdateData(
                table: "ShipperRequest",
                keyColumn: "Id",
                keyValue: new Guid("34343434-3434-3434-3434-343434343434"),
                column: "VerificationStatus",
                value: "Approved");

            migrationBuilder.UpdateData(
                table: "Shippers",
                keyColumn: "Id",
                keyValue: new Guid("56565656-5656-5656-5656-565656565656"),
                column: "Status",
                value: "Approved");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "Status",
                value: "Active");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Status",
                value: "Active");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "Status",
                value: "Active");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "Status",
                value: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Users"
                SET "Status" = CASE "Status"
                    WHEN 'Active' THEN '0'
                    WHEN 'Inactive' THEN '1'
                    WHEN 'Banned' THEN '2'
                    WHEN 'PendingVerification' THEN '3'
                    ELSE "Status"
                END;

                UPDATE "Shippers"
                SET "Status" = CASE "Status"
                    WHEN 'Pending' THEN '0'
                    WHEN 'Approved' THEN '1'
                    WHEN 'Rejected' THEN '2'
                    WHEN 'Suspended' THEN '3'
                    ELSE "Status"
                END;

                UPDATE "Merchants"
                SET "Status" = CASE "Status"
                    WHEN 'Pending' THEN '0'
                    WHEN 'Approved' THEN '1'
                    WHEN 'Rejected' THEN '2'
                    WHEN 'Suspended' THEN '3'
                    ELSE "Status"
                END;

                UPDATE "ShipperRequest"
                SET "VerificationStatus" = CASE "VerificationStatus"
                    WHEN 'Pending' THEN '0'
                    WHEN 'Approved' THEN '1'
                    WHEN 'Rejected' THEN '2'
                    ELSE "VerificationStatus"
                END;

                UPDATE "MerchantRequests"
                SET "VerificationStatus" = CASE "VerificationStatus"
                    WHEN 'Pending' THEN '0'
                    WHEN 'Approved' THEN '1'
                    WHEN 'Rejected' THEN '2'
                    ELSE "VerificationStatus"
                END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Shippers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "VerificationStatus",
                table: "ShipperRequest",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Merchants",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "VerificationStatus",
                table: "MerchantRequests",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Merchants",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ShipperRequest",
                keyColumn: "Id",
                keyValue: new Guid("34343434-3434-3434-3434-343434343434"),
                column: "VerificationStatus",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Shippers",
                keyColumn: "Id",
                keyValue: new Guid("56565656-5656-5656-5656-565656565656"),
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "Status",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Status",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "Status",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "Status",
                value: 0);
        }
    }
}
