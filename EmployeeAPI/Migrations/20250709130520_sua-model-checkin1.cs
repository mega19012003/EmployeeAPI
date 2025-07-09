using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class suamodelcheckin1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckinAfternoon",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CheckinAfternoonStatus",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CheckinMorning",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CheckinMorningStatus",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CheckoutAfternoonStatus",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "CheckoutMorningStatus",
                table: "Checkins");

            migrationBuilder.RenameColumn(
                name: "CheckoutMorning",
                table: "Checkins",
                newName: "CheckoutTime");

            migrationBuilder.RenameColumn(
                name: "CheckoutAfternoon",
                table: "Checkins",
                newName: "CheckinTime");

            migrationBuilder.AddColumn<int>(
                name: "LogStatus",
                table: "Checkins",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogStatus",
                table: "Checkins");

            migrationBuilder.RenameColumn(
                name: "CheckoutTime",
                table: "Checkins",
                newName: "CheckoutMorning");

            migrationBuilder.RenameColumn(
                name: "CheckinTime",
                table: "Checkins",
                newName: "CheckoutAfternoon");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckinAfternoon",
                table: "Checkins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CheckinAfternoonStatus",
                table: "Checkins",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckinMorning",
                table: "Checkins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CheckinMorningStatus",
                table: "Checkins",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckoutAfternoonStatus",
                table: "Checkins",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckoutMorningStatus",
                table: "Checkins",
                type: "int",
                nullable: true);
        }
    }
}
