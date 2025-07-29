using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class db7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "DutyDetail");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Duty");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Deadline",
                table: "DutyDetail",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DutyDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Duty",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "DutyDetail");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DutyDetail");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Duty");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Companies");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "DutyDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Duty",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
