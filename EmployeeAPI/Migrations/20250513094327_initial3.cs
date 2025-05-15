using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class initial3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Duties");

            migrationBuilder.DropColumn(
                name: "TimeAbsent",
                table: "Absents");

            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "Absents");

            migrationBuilder.DropColumn(
                name: "isFined",
                table: "Absents");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "DutyDetail",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "Absents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Absents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Absents");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "DutyDetail",
                newName: "Note");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Duties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "Absents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TimeAbsent",
                table: "Absents",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<double>(
                name: "TotalHours",
                table: "Absents",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isFined",
                table: "Absents",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
