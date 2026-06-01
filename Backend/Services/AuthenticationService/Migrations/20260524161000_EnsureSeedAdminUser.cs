using AuthenticationService.Persistences;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationService.Migrations
{
    [DbContext(typeof(AuthenticationDbContext))]
    [Migration("20260524161000_EnsureSeedAdminUser")]
    public partial class EnsureSeedAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO "AspNetRoles" ("Id", "ConcurrencyStamp", "Name", "NormalizedName")
                SELECT
                    '44444444-4444-4444-8444-444444444444'::uuid,
                    'SEED-ADMIN-ROLE-CONCURRENCY-STAMP',
                    'Admin',
                    'ADMIN'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "AspNetRoles"
                    WHERE "Id" = '44444444-4444-4444-8444-444444444444'::uuid
                       OR "NormalizedName" = 'ADMIN'
                );

                INSERT INTO "AspNetUsers" (
                    "Id",
                    "AccessFailedCount",
                    "ConcurrencyStamp",
                    "CreatedAt",
                    "Email",
                    "EmailConfirmed",
                    "FullName",
                    "IsOtpVerified",
                    "LockoutEnabled",
                    "LockoutEnd",
                    "NormalizedEmail",
                    "NormalizedUserName",
                    "Otp",
                    "OtpExpiresAt",
                    "PasswordHash",
                    "PhoneNumber",
                    "PhoneNumberConfirmed",
                    "SecurityStamp",
                    "Status",
                    "TwoFactorEnabled",
                    "UpdatedAt",
                    "UserName"
                )
                SELECT
                    '55555555-5555-4555-8555-555555555555'::uuid,
                    0,
                    'SEED-ADMIN-CONCURRENCY-STAMP',
                    TIMESTAMPTZ '2026-01-01 00:00:00+00',
                    'admin@fooddelivery.local',
                    true,
                    'Seeded Admin',
                    true,
                    false,
                    NULL,
                    'ADMIN@FOODDELIVERY.LOCAL',
                    'ADMIN@FOODDELIVERY.LOCAL',
                    NULL,
                    NULL,
                    'AQAAAAIAAYagAAAAEHwR9ekhFFazFAUin52gHpisNjCdZFdVS0N+lAgSluDA+/uEYfuONXMTsB3L8jrAdA==',
                    NULL,
                    false,
                    'SEED-ADMIN-SECURITY-STAMP',
                    'Active',
                    false,
                    NULL,
                    'admin@fooddelivery.local'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "AspNetUsers"
                    WHERE "Id" = '55555555-5555-4555-8555-555555555555'::uuid
                );

                INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
                SELECT
                    '55555555-5555-4555-8555-555555555555'::uuid,
                    '44444444-4444-4444-8444-444444444444'::uuid
                WHERE EXISTS (
                    SELECT 1 FROM "AspNetUsers"
                    WHERE "Id" = '55555555-5555-4555-8555-555555555555'::uuid
                )
                AND EXISTS (
                    SELECT 1 FROM "AspNetRoles"
                    WHERE "Id" = '44444444-4444-4444-8444-444444444444'::uuid
                )
                AND NOT EXISTS (
                    SELECT 1 FROM "AspNetUserRoles"
                    WHERE "UserId" = '55555555-5555-4555-8555-555555555555'::uuid
                      AND "RoleId" = '44444444-4444-4444-8444-444444444444'::uuid
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "AspNetUserRoles"
                WHERE "UserId" = '55555555-5555-4555-8555-555555555555'::uuid
                  AND "RoleId" = '44444444-4444-4444-8444-444444444444'::uuid;

                DELETE FROM "AspNetUsers"
                WHERE "Id" = '55555555-5555-4555-8555-555555555555'::uuid;
                """);
        }
    }
}
