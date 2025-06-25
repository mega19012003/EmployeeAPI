using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class updatecheckintable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CheckinAfternoonStatus",
                table: "Checkins",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckoutAfternoon",
                table: "Checkins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckoutMorning",
                table: "Checkins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CheckoutMorningStatus",
                table: "Checkins",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckinAfternoonStatus",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CheckoutAfternoon",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CheckoutMorning",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CheckoutMorningStatus",
                table: "Checkins");
        }
    }
}
