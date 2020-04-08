using Microsoft.EntityFrameworkCore.Migrations;

namespace Pensees.Charon.Migrations
{
    public partial class Add_OptionalMember_To_Tenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "AbpTenants",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "AbpTenants",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "AbpTenants",
                maxLength: 32,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "AbpTenants");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "AbpTenants");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "AbpTenants");
        }
    }
}
