using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pensees.Charon.Migrations
{
    public partial class Add_OptionMember_To_User : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AbpUsers",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireDate",
                table: "AbpUsers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AbpUsers",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "AbpUsers",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficePhoneNumber",
                table: "AbpUsers",
                maxLength: 32,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "OfficePhoneNumber",
                table: "AbpUsers");
        }
    }
}
