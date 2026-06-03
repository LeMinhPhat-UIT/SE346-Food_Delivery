using System;
using DeliveryService.Persistences;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DeliveryDbContext))]
    [Migration("20260604120000_AlignSeedAvailabilityAssignment")]
    public partial class AlignSeedAvailabilityAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "ShipperAvailabilities"
                SET "CurrentOrderId" = NULL,
                    "CurrentAssignmentId" = NULL,
                    "CurrentOfferedAssignmentId" = NULL,
                    "OfferingExpiresAt" = NULL
                WHERE "Status" = 'ActiveIdle';
                """);

            migrationBuilder.Sql("""
                UPDATE "ShipperAvailabilities"
                SET "CurrentAssignmentId" = '64444444-4444-4444-8444-444444444444'::uuid
                WHERE "Id" = '66666666-6666-4666-8666-666666666666'::uuid;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "ShipperAvailabilities"
                SET "CurrentAssignmentId" = NULL
                WHERE "Id" = '66666666-6666-4666-8666-666666666666'::uuid;
                """);
        }
    }
}
