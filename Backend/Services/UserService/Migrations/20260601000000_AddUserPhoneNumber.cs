using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using UserService.Persistences;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(UserDbContext))]
    [Migration("20260601000000_AddUserPhoneNumber")]
    public partial class AddUserPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Users" SET "PhoneNumber" = '0900000000' WHERE "Id" = '55555555-5555-4555-8555-555555555555';
                UPDATE "Users" SET "PhoneNumber" = '0900000003' WHERE "Id" = '99999999-9999-4999-9999-999999999999';
                UPDATE "Users" SET "PhoneNumber" = '0900000001' WHERE "Id" = 'aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa';
                UPDATE "Users" SET "PhoneNumber" = '0900000002' WHERE "Id" = 'bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");
        }
    }
}
