using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    public partial class StoreEnumsAsStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ShipperAvailabilities",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ShipperAssignments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Incidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "DeliveryTrackings",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql(
                """
                UPDATE "ShipperAvailabilities"
                SET "Status" = CASE "Status"
                    WHEN '0' THEN 'Offline'
                    WHEN '1' THEN 'ActiveIdle'
                    WHEN '2' THEN 'PendingAssignment'
                    WHEN '3' THEN 'Delivering'
                    ELSE "Status"
                END;

                UPDATE "ShipperAssignments"
                SET "Status" = CASE "Status"
                    WHEN '0' THEN 'Pending'
                    WHEN '1' THEN 'Accepted'
                    WHEN '2' THEN 'Rejected'
                    WHEN '3' THEN 'Timeout'
                    WHEN '4' THEN 'Cancelled'
                    ELSE "Status"
                END;

                UPDATE "Incidents"
                SET "Type" = CASE "Type"
                    WHEN '0' THEN 'WrongOrder'
                    WHEN '1' THEN 'MissingItem'
                    WHEN '2' THEN 'Damaged'
                    WHEN '3' THEN 'LateDelivery'
                    WHEN '4' THEN 'RudeBehavior'
                    WHEN '5' THEN 'Other'
                    ELSE "Type"
                END,
                "Status" = CASE "Status"
                    WHEN '0' THEN 'Pending'
                    WHEN '1' THEN 'Investigating'
                    WHEN '2' THEN 'Resolved'
                    WHEN '3' THEN 'Closed'
                    ELSE "Status"
                END;

                UPDATE "DeliveryTrackings"
                SET "Status" = CASE "Status"
                    WHEN '0' THEN 'Pending'
                    WHEN '1' THEN 'Assigned'
                    WHEN '2' THEN 'PickingUp'
                    WHEN '3' THEN 'PickedUp'
                    WHEN '4' THEN 'Delivering'
                    WHEN '5' THEN 'Delivered'
                    WHEN '6' THEN 'Failed'
                    ELSE "Status"
                END;
                """);

            migrationBuilder.UpdateData(
                table: "DeliveryTrackings",
                keyColumn: "Id",
                keyValue: new Guid("61111111-1111-1111-1111-111111111111"),
                column: "Status",
                value: "Assigned");

            migrationBuilder.UpdateData(
                table: "DeliveryTrackings",
                keyColumn: "Id",
                keyValue: new Guid("74444444-4444-4444-4444-444444444444"),
                column: "Status",
                value: "Delivered");

            migrationBuilder.UpdateData(
                table: "Incidents",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "Status", "Type" },
                values: new object[] { "Investigating", "MissingItem" });

            migrationBuilder.UpdateData(
                table: "ShipperAssignments",
                keyColumn: "Id",
                keyValue: new Guid("64444444-4444-4444-4444-444444444444"),
                column: "Status",
                value: "Accepted");

            migrationBuilder.UpdateData(
                table: "ShipperAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "Status",
                value: "Delivering");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "ShipperAvailabilities"
                SET "Status" = CASE "Status"
                    WHEN 'Offline' THEN '0'
                    WHEN 'ActiveIdle' THEN '1'
                    WHEN 'PendingAssignment' THEN '2'
                    WHEN 'Delivering' THEN '3'
                    ELSE "Status"
                END;

                UPDATE "ShipperAssignments"
                SET "Status" = CASE "Status"
                    WHEN 'Pending' THEN '0'
                    WHEN 'Accepted' THEN '1'
                    WHEN 'Rejected' THEN '2'
                    WHEN 'Timeout' THEN '3'
                    WHEN 'Cancelled' THEN '4'
                    ELSE "Status"
                END;

                UPDATE "Incidents"
                SET "Type" = CASE "Type"
                    WHEN 'WrongOrder' THEN '0'
                    WHEN 'MissingItem' THEN '1'
                    WHEN 'Damaged' THEN '2'
                    WHEN 'LateDelivery' THEN '3'
                    WHEN 'RudeBehavior' THEN '4'
                    WHEN 'Other' THEN '5'
                    ELSE "Type"
                END,
                "Status" = CASE "Status"
                    WHEN 'Pending' THEN '0'
                    WHEN 'Investigating' THEN '1'
                    WHEN 'Resolved' THEN '2'
                    WHEN 'Closed' THEN '3'
                    ELSE "Status"
                END;

                UPDATE "DeliveryTrackings"
                SET "Status" = CASE "Status"
                    WHEN 'Pending' THEN '0'
                    WHEN 'Assigned' THEN '1'
                    WHEN 'PickingUp' THEN '2'
                    WHEN 'PickedUp' THEN '3'
                    WHEN 'Delivering' THEN '4'
                    WHEN 'Delivered' THEN '5'
                    WHEN 'Failed' THEN '6'
                    ELSE "Status"
                END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ShipperAvailabilities",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ShipperAssignments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Incidents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Incidents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "DeliveryTrackings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "DeliveryTrackings",
                keyColumn: "Id",
                keyValue: new Guid("61111111-1111-1111-1111-111111111111"),
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DeliveryTrackings",
                keyColumn: "Id",
                keyValue: new Guid("74444444-4444-4444-4444-444444444444"),
                column: "Status",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Incidents",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "Status", "Type" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "ShipperAssignments",
                keyColumn: "Id",
                keyValue: new Guid("64444444-4444-4444-4444-444444444444"),
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ShipperAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "Status",
                value: 3);
        }
    }
}
