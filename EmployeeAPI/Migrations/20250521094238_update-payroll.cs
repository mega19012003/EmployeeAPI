using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class updatepayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutDate",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Payrolls");

            migrationBuilder.AddColumn<int>(
                name: "DateWorked",
                table: "Payrolls",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateWorked",
                table: "Payrolls");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckoutDate",
                table: "Payrolls",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Payrolls",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
