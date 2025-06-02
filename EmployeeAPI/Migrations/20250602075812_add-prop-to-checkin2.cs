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
                newName: "CheckoutStatus");

            migrationBuilder.AddColumn<int>(
                name: "CheckinStatus",
                table: "Checkins",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckinStatus",
                table: "Checkins");

            migrationBuilder.RenameColumn(
                name: "CheckoutStatus",
                table: "Checkins",
                newName: "Status");
        }
    }
}
