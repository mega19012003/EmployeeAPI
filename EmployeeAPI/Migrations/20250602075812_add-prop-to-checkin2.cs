using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class addproptocheckin2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Checkins",
                newName: "CheckoutAfternoonStatus");

            migrationBuilder.AddColumn<int>(
                name: "CheckinMorningStatus",
                table: "Checkins",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckinMorningStatus",
                table: "Checkins");

            migrationBuilder.RenameColumn(
                name: "CheckoutAfternoonStatus",
                table: "Checkins",
                newName: "Status");
        }
    }
}
