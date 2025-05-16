using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class themcheckin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHoliday",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "IsOvertime",
                table: "Checkins");

            migrationBuilder.RenameColumn(
                name: "isLate",
                table: "Checkins",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "HolidayType",
                table: "Checkins",
                newName: "Status");

            migrationBuilder.AddColumn<bool>(
                name: "IsAbsent",
                table: "Payrolls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payrolls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Payrolls",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAbsent",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Payrolls");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Checkins",
                newName: "HolidayType");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Checkins",
                newName: "isLate");

            migrationBuilder.AddColumn<bool>(
                name: "IsHoliday",
                table: "Checkins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOvertime",
                table: "Checkins",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
