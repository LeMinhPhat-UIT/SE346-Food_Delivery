using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class InitialUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShipperId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    RecipientName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    AddressLine = table.Column<string>(type: "text", nullable: false),
                    Ward = table.Column<string>(type: "text", nullable: false),
                    District = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Lat = table.Column<decimal>(type: "numeric", nullable: true),
                    Lng = table.Column<decimal>(type: "numeric", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreName = table.Column<string>(type: "text", nullable: false),
                    StoreDescription = table.Column<string>(type: "text", nullable: false),
                    BusinessLicense = table.Column<string>(type: "text", nullable: false),
                    BusinessLicenseUrl = table.Column<string>(type: "text", nullable: false),
                    TaxId = table.Column<string>(type: "text", nullable: false),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    RejectedReason = table.Column<string>(type: "text", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantRequests_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MerchantRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreName = table.Column<string>(type: "text", nullable: false),
                    StoreDescription = table.Column<string>(type: "text", nullable: false),
                    StoreLogoUrl = table.Column<string>(type: "text", nullable: false),
                    StoreBannerUrl = table.Column<string>(type: "text", nullable: false),
                    BusinessLicense = table.Column<string>(type: "text", nullable: false),
                    TaxId = table.Column<string>(type: "text", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    OpeningTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ClosingTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    MinOrderAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    AvgPrepTime = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Merchants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipperRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseNumber = table.Column<string>(type: "text", nullable: false),
                    LicenseFrontUrl = table.Column<string>(type: "text", nullable: false),
                    LicenseBackUrl = table.Column<string>(type: "text", nullable: false),
                    IdCardFrontUrl = table.Column<string>(type: "text", nullable: false),
                    IdCardBackUrl = table.Column<string>(type: "text", nullable: false),
                    SelfieUrl = table.Column<string>(type: "text", nullable: false),
                    IdNumber = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    RejectedReason = table.Column<string>(type: "text", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipperRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipperRequest_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipperRequest_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressLine = table.Column<string>(type: "text", nullable: false),
                    Ward = table.Column<string>(type: "text", nullable: false),
                    District = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Lat = table.Column<decimal>(type: "numeric", nullable: true),
                    Lng = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantAddresses_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shippers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehiclePlate = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shippers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shippers_ShipperRequest_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ShipperRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shippers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "DeletedAt", "FullName", "MerchantId", "ShipperId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa"), "https://example.com/avatars/customer.png", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Seeded Customer", null, null, 0, null },
                    { new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb"), "https://example.com/avatars/merchant.png", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Seeded Merchant Owner", null, null, 0, null }
                });

            migrationBuilder.InsertData(
                table: "Addresses",
                columns: new[] { "Id", "AddressLine", "City", "CreatedAt", "DeletedAt", "District", "IsDefault", "Label", "Lat", "Lng", "Phone", "RecipientName", "UpdatedAt", "UserId", "Ward" },
                values: new object[,]
                {
                    { new Guid("dddddddd-dddd-4ddd-8ddd-dddddddddddd"), "1 Nguyen Hue", "Ho Chi Minh City", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "District 1", true, "Home", 10.7769m, 106.7009m, "0900000001", "Seeded Customer", null, new Guid("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa"), "Ben Nghe" },
                    { new Guid("eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"), "2 Le Loi", "Ho Chi Minh City", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "District 1", true, "Store Contact", 10.7722m, 106.6983m, "0900000002", "Seeded Merchant Owner", null, new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb"), "Ben Thanh" }
                });

            migrationBuilder.InsertData(
                table: "Merchants",
                columns: new[] { "Id", "AvgPrepTime", "BusinessLicense", "ClosingTime", "CreatedAt", "DeletedAt", "IsOpen", "MinOrderAmount", "OpeningTime", "Status", "StoreBannerUrl", "StoreDescription", "StoreLogoUrl", "StoreName", "TaxId", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"), 20, "BL-SEED-0001", new TimeSpan(0, 22, 0, 0, 0), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 30000m, new TimeSpan(0, 8, 0, 0, 0), 1, "https://example.com/stores/banner.png", "Default merchant store for local development.", "https://example.com/stores/logo.png", "Seeded Merchant Store", "TAX-SEED-0001", null, new Guid("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb") });

            migrationBuilder.InsertData(
                table: "MerchantAddresses",
                columns: new[] { "Id", "AddressLine", "City", "CreatedAt", "DeletedAt", "District", "Lat", "Lng", "MerchantId", "UpdatedAt", "Ward" },
                values: new object[] { new Guid("ffffffff-ffff-4fff-8fff-ffffffffffff"), "2 Le Loi", "Ho Chi Minh City", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "District 1", 10.7722m, 106.6983m, new Guid("cccccccc-cccc-4ccc-8ccc-cccccccccccc"), null, "Ben Thanh" });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantAddresses_MerchantId",
                table: "MerchantAddresses",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantRequests_ReviewedBy",
                table: "MerchantRequests",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantRequests_UserId",
                table: "MerchantRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_UserId",
                table: "Merchants",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipperRequest_ReviewedBy",
                table: "ShipperRequest",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ShipperRequest_UserId",
                table: "ShipperRequest",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_RequestId",
                table: "Shippers",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_UserId",
                table: "Shippers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "MerchantAddresses");

            migrationBuilder.DropTable(
                name: "MerchantRequests");

            migrationBuilder.DropTable(
                name: "Shippers");

            migrationBuilder.DropTable(
                name: "Merchants");

            migrationBuilder.DropTable(
                name: "ShipperRequest");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
