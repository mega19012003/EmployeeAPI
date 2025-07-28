using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class db3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IP",
                table: "Checkins",
                newName: "CheckoutIP");

            migrationBuilder.AddColumn<string>(
                name: "CheckinIP",
                table: "Checkins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckinIP",
                table: "Checkins");

            migrationBuilder.RenameColumn(
                name: "CheckoutIP",
                table: "Checkins",
                newName: "IP");
        }
    }
}
