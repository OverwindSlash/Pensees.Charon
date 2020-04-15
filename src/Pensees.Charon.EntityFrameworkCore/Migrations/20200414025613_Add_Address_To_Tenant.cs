using Microsoft.EntityFrameworkCore.Migrations;

namespace Pensees.Charon.Migrations
{
    public partial class Add_Address_To_Tenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AbpTenants",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "AbpTenants");
        }
    }
}
