using System;
using DeliveryService.Persistences;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DeliveryDbContext))]
    [Migration("20260603052000_UseFileKeysInDeliverySeedData")]
    public partial class UseFileKeysInDeliverySeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Incidents"
                SET "ProofUrl" = ARRAY[
                    'deliveries/75555555-5555-4555-8555-555555555555/56565656-5656-4656-8656-565656565656/incident/order-2-photo-1.jpg',
                    'deliveries/75555555-5555-4555-8555-555555555555/56565656-5656-4656-8656-565656565656/incident/order-2-photo-2.jpg'
                ]::text[]
                WHERE "Id" = '77777777-7777-4777-8777-777777777777'::uuid;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Incidents"
                SET "ProofUrl" = ARRAY[
                    'https://example.com/incidents/order-2-photo-1.jpg',
                    'https://example.com/incidents/order-2-photo-2.jpg'
                ]::text[]
                WHERE "Id" = '77777777-7777-4777-8777-777777777777'::uuid;
                """);
        }
    }
}
